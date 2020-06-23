using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Sql;
using System.Data.Entity;
using System.Web.Mvc;
using System.Threading.Tasks;

using SQLExerciser.Models;
using SQLExerciser.Models.DB;
using SQLExerciser.Models.ViewModel;
using System.Data.SqlClient;

namespace SQLExerciser.Controllers
{
    public class ExerciseController : Controller
    {
        readonly IExercisesContext _context;
        readonly IQueryTester _tester;
        readonly IExerciser _exerciser;

        public async Task<ViewResult> Index()
        {
            return View(await Task.Run(() => _context.Exercises.ToList()));
        }

        [HttpGet]
        public async Task<JsonResult> TestQuery(string query, int diagramId)
        {
            var diagram = await Task.Run(() => _context.Diagrams.Find(diagramId));
            var seeds = await Task.Run(() => _context.Seeds.Where(s => s.Diagram.DbDiagramId == diagram.DbDiagramId).ToList());
            var result = await _tester.TestSelect(diagram.CreationQuery, seeds.Select(s => s.SeedQuery), query);
            return Json(new { success = true, result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<JsonResult> TestQueryDML(string solution, string verify, int diagramId)
        {
            var diagram = await Task.Run(() => _context.Diagrams.Find(diagramId));
            var seeds = await Task.Run(() => _context.Seeds.Where(s => s.Diagram.DbDiagramId == diagram.DbDiagramId).ToList());
            seeds.Add(new DataSeed { SeedQuery = solution });
            var result = await _tester.TestSelect(diagram.CreationQuery, seeds.Select(s => s.SeedQuery), verify);
            return Json(new { success = true, result }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public Task<ViewResult> Create(int diagramId)
        {
            return Task.Run(() => View(new CreateExercise { Diagram = diagramId }));
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateExercise exercise)
        {
            if (ModelState.IsValid)
            {
                dynamic result;
                if (string.IsNullOrEmpty(exercise.VerificationQuery))
                {
                    result = (await TestQuery(exercise.SolutionQuery, exercise.Diagram)).Data;
                }
                else
                {
                    result = (await TestQueryDML(exercise.SolutionQuery, exercise.VerificationQuery, exercise.Diagram)).Data;
                }
                if (((string)result.result).StartsWith("ERROR"))
                {
                    return View(exercise);
                }
                var toInJudge = new Judge
                {
                    VerifyQuery = exercise.VerificationQuery,
                    AnswerQuery = exercise.SolutionQuery,
                    Diagram = _context.Diagrams.Find(exercise.Diagram)
                };
                var toIn = new Exercise
                {
                    Description = exercise.Description,
                    Difficulty = exercise.Difficulty,
                    Title = exercise.Title,
                    Judge = toInJudge
                };
                await Task.Run(() => _context.Judges.Add(toInJudge));
                await Task.Run(() => _context.Exercises.Add(toIn));
                await _context.SaveAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return View(exercise);
            }
        }

        [HttpGet]
        public ViewResult Solve(int id)
        {
            return View(_context.Exercises.Include(x => x.Judge.Diagram).FirstOrDefault(e => e.ExerciseId == id));
        }

        [HttpPost]
        public async Task<ViewResult> Solve(int id, string query)
        {
            var exercise = await Task.Run(() => _context.Exercises
                .Include(e => e.Judge.Diagram)
                .Single(e => e.ExerciseId == id));
            List<string> setupQueries = new List<string>();
            setupQueries.Add(exercise.Judge.Diagram.CreationQuery);
            setupQueries
                .AddRange(_context.Seeds
                    .Where(seed => seed.Diagram.DbDiagramId == exercise.Judge.Diagram.DbDiagramId)
                    .Select(seed => seed.SeedQuery));
            var output = await _exerciser.ExecuteExercise(exercise, query, setupQueries);
            if (output.Key)
            {
                RegisterSuccess(exercise, query);
                return View("Success", new { exercise, query });
            }
            else
            {
                RegisterFailure(exercise, query, output.Value);
                return View("Fail", new FailedSubmission
                {
                    Exercise = exercise,
                    Query = query,
                    Result = output.Value
                });
            }
        }

        void RegisterSuccess(Exercise exercise, string query)
        {
            var user = _context.CurrentUser;
            _context.Statuses.Add(new ExerciseStatus
            {
                Accepted = true,
                Solution = query,
                Exercise = exercise,
                User = user
            });
        }

        void RegisterFailure(Exercise exercise, string query, string output)
        {
            var user = _context.CurrentUser;
            _context.Statuses.Add(new ExerciseStatus
            {
                Accepted = false,
                Resolution = output,
                Solution = query,
                Exercise = exercise,
                User = user
            });
        }

        public ExerciseController(IExercisesContext ctx, IExerciser exerciser, IQueryTester tester)
        {
            _context = ctx;
            _exerciser = exerciser;
            _tester = tester;
        }
    }
}
