using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using SQLExerciser.Models;
using SQLExerciser.Models.DB;

namespace SQLExerciser.Controllers
{
    public class PickerController : Controller
    {
        readonly IExercisesContext _context;
        readonly IStorage _storage;

        public async Task<ViewResult> PickDiagram(string returnUrl)
        {
            _storage.StoreValue(returnUrl);
            var diagrams = await Task.Run(() => _context.Diagrams.ToList());
            return View(diagrams);
        }

        public ActionResult PickDiagram(int id)
        {
            _storage.StoreValue(new DbDiagram { DbDiagramId = id });
            var redirect = _storage.GetValue<string>();
            _storage.ClearValue<string>();
            return Redirect(redirect);
        }

        [HttpGet]
        public async Task<ViewResult> PickSeed(int diagramId, string returnUrl)
        {
            _storage.StoreValue(returnUrl);
            var relatedExercises = from e in _context.Exercises
                                   where e.Judge.Diagram.DbDiagramId == diagramId
                                   select e.Title;
            var seeds = from e in _context.Seeds
                        where e.Diagram.DbDiagramId == diagramId
                        select new Seed(e.SeedQuery, e.DataSeedId, e.Diagram.DbDiagramId, relatedExercises);
            return View(await Task.Run(() => seeds.ToList()));
        }

        [HttpPost]
        public Task<RedirectResult> PickSeed(int seedId)
        {
            _storage.StoreValue((from e in _context.Seeds
                                 where e.DataSeedId == seedId
                                 select e).First());
            var returnUrl = _storage.GetValue<string>();
            _storage.ClearValue<string>();
            return Task.Run(() => Redirect(returnUrl));
        }

        public PickerController(IExercisesContext ctx, IStorage storage)
        {
            _storage = storage;
            _context = ctx;
            _storage.Session = Session;
        }
    }
}