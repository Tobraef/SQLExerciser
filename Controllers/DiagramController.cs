using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.IO;

using System.Data.Entity;
using SQLExerciser.Models.DB;
using SQLExerciser.Models;

namespace SQLExerciser.Controllers
{
    [Authorize(Roles = Role.ExerciserRoles)]
    public class DiagramController : Controller
    {
        readonly IExercisesContext _context;
        readonly IQueryTester _tester;

        public DiagramController(IExercisesContext ctx, IQueryTester tester)
        {
            _context = ctx;
            _tester = tester;
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            return View(await Task.Run(() =>  _context.Diagrams.ToList()));
        }

        [HttpGet]
        public async Task<ViewResult> Details(int id)
        {
            var diagram = await Task.Run(() => _context.Diagrams.Find(id));
            return View(new DiagramDetails
            {
                DbDiagramId = id,
                Diagram = diagram.Diagram,
                Exercises = _context.Exercises.Where(e => e.Judge.Diagram.DbDiagramId == id).ToList(),
                Name = diagram.Name
            });
        }

        [HttpGet]
        public async Task<ViewResult> Edit(int id)
        {
            return View(await _context.Diagrams.FindAsync(id));
        }

        [HttpGet]
        public Task<ViewResult> Create()
        {
            return Task.Run(() => View());
        }

        byte[] HardcodedDiagram
        {
            get
            {
                using (var fStream = new FileStream(@"C:\Users\przem\Downloads\casualPhoto.jpg", FileMode.Open))
                {
                    MemoryStream mStream = new MemoryStream();
                    fStream.CopyTo(mStream);
                    return mStream.ToArray();
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> Create(DbDiagram receivedDiagram, HttpPostedFileBase image)
        {
            if (ModelState.IsValid)
            {
                dynamic queryResult = (await TryQuery(receivedDiagram.CreationQuery)).Data;
                if (((string)queryResult.result).StartsWith("ERROR"))
                {
                    return View(receivedDiagram);
                }
                if (image == null)
                {
                    receivedDiagram.Diagram = receivedDiagram.Diagram ?? HardcodedDiagram;
                }
                else
                {
                    using (MemoryStream mStream = new MemoryStream())
                    {
                        image.InputStream.CopyTo(mStream);
                        receivedDiagram.Diagram = mStream.ToArray();
                    }
                }
                _context.Diagrams.Add(receivedDiagram);
                await _context.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(receivedDiagram);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Edit(DbDiagram diagram)
        {
            if (ModelState.IsValid)
            {
                dynamic queryResult = (await TryQuery(diagram.CreationQuery)).Data;
                if (((string)queryResult.result).StartsWith("ERROR: "))
                {
                    return View(diagram);
                }
            }
            _context.Diagrams.Remove(diagram);
            _context.Diagrams.Add(diagram);
            await _context.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpDelete]
        public async Task<RedirectToRouteResult> Remove(int id)
        {
            _context.Diagrams.Remove(new DbDiagram { DbDiagramId = id });
            await _context.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<JsonResult> TryQuery(string query)
        {
            var output = await _tester.TestTableSetup(query);
            return Json(new
            {
                result = output.StartsWith("ERROR") ? output : "OK",
                success = true
            }, JsonRequestBehavior.AllowGet);
        }

        public FileResult ReadImage(int diagramId)
        {
            var diagram = _context.Diagrams.Find(diagramId);
            return File(diagram.Diagram, ".png");
        }
    }
}