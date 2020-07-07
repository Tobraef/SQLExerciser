using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

using SQLExerciser.Models.DB;
using SQLExerciser.Models.ViewModel;
using SQLExerciser.Models;

namespace SQLExerciser.Controllers
{
    [Authorize(Roles = Role.ExerciserRoles)]
    public class SeedController : Controller
    {
        readonly IExercisesContext _context;
        readonly IQueryTester _tester;

        [HttpGet]
        public async Task<ViewResult> Details(int diagramId)
        {
            var seeds = (from e in _context.Seeds
                        where e.Diagram.DbDiagramId == diagramId
                        select e).ToList();
            return View(await Task.Run(() => seeds.Select(e => new Seed(e.SeedQuery, diagramId, e.DataSeedId)).ToList()));
        }

        [HttpGet]
        public async Task<ViewResult> Index()
        {
            var seeds = _context.Seeds
                .Include(s => s.Diagram)
                .GroupBy(s => s.Diagram.DbDiagramId)
                .ToList();
            var parsedSeeds = seeds.SelectMany(gr => gr.Select(s => new Seed(s.SeedQuery, s.DataSeedId, gr.Key)));
            return View(await Task.Run(() => parsedSeeds.GroupBy(s => s.DiagramId)));
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
                    if (seed.SeedId.HasValue)
                    {
                        var dbSeed = await Task.Run(() => _context.Seeds.Find(seed.SeedId.Value));
                        dbSeed.SeedQuery = seed.SeedQuery;
                    }
                    else
                    {
                        _context.Seeds.Add(new DataSeed
                        {
                            Diagram = await FindDiagram(seed.DiagramId),
                            SeedQuery = seed.SeedQuery,
                        });
                    }
                    await _context.SaveAsync();
                    return RedirectToAction(nameof(Index), new { diagramId = seed.DiagramId });
                }
            }
            else
            {
                return View(seed);
            }
        }

        [HttpGet]
        public async Task<ActionResult> Edit(int id)
        {
            var seed = await Task.Run(() => _context.Seeds.Find(id));
            return View("Create", new SeedCreateViewModel { SeedId = id, DiagramId = seed.Diagram.DbDiagramId, SeedQuery = seed.SeedQuery });
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
            var otherSeeds = await Task.Run(() => _context.Seeds
                .Include(s => s.Diagram)
                .Where(s => s.Diagram.DbDiagramId == diagramId));
            var diagram = otherSeeds.FirstOrDefault()?.Diagram ?? _context.Diagrams.Find(diagramId);
            var allSeeds = otherSeeds.Select(s => s.SeedQuery).ToList();
            allSeeds.Add(query);
            var output = await _tester.TestCRUD(diagram.CreationQuery, allSeeds);
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