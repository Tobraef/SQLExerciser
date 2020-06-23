using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

using SQLExerciser.Models.DB;
using SQLExerciser.Models;

namespace SQLExerciser.Controllers
{
    public class SeedController : Controller
    {
        readonly IExercisesContext _context;
        readonly IQueryTester _tester;

        [HttpGet]
        public async Task<ViewResult> Index(int diagramId)
        {
            var relatedExercises = (from e in _context.Exercises
                                   where e.Judge.Diagram.DbDiagramId == diagramId
                                   select e.Title).ToList();
            var seeds = (from e in _context.Seeds
                        where e.Diagram.DbDiagramId == diagramId
                        select e).ToList();
            return View(await Task.Run(() => seeds.Select(e => new Seed(e.SeedQuery, diagramId, e.DataSeedId, relatedExercises)).ToList()));
        }

        Task<DbDiagram> FindDiagram(int diagramId) =>
            Task.Run(() => _context.Diagrams.First(d => d.DbDiagramId == diagramId));

        [HttpGet]
        public Task<ViewResult> Create(int diagramId)
        {
            return Task.Run(() => View(new SeedCreateViewModel { DiagramId = diagramId }));
        }

        [HttpPost]
        public async Task<ActionResult> Create(SeedCreateViewModel seed)
        {
            if (ModelState.IsValid)
            {
                var result = await TestQuery(seed.SeedQuery, seed.DiagramId);
                if (((dynamic)result.Data).result.ToString().StartsWith("ERROR: "))
                {
                    return View(seed);
                }
                else
                {
                    _context.Seeds.Add(new DataSeed
                    {
                        Diagram = await FindDiagram(seed.DiagramId),
                        SeedQuery = seed.SeedQuery,
                    });
                    await _context.SaveAsync();
                    return RedirectToAction(nameof(Index), new { diagramId = seed.DiagramId });
                }
            }
            else
            {
                return View(seed);
            }
        }

        [HttpPost]
        public async Task<RedirectToRouteResult> Remove(int id)
        {
            _context.Seeds.Remove(new DataSeed { DataSeedId = id });
            await _context.SaveAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<JsonResult> TestQuery(string query, int diagramId)
        {
            var diagram = await FindDiagram(diagramId);
            var output = await _tester.TestCRUD(diagram.CreationQuery, query);
            return Json(new {
                success = true,
                result = string.IsNullOrEmpty(output) ? "OK" : output
            }, JsonRequestBehavior.AllowGet);
        }

        public SeedController(IExercisesContext ctx, IQueryTester tester)
        {
            _tester = tester;
            _context = ctx;
        }
    }
}