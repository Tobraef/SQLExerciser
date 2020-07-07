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
    public class ExerciseController : UserAccessController
    {
        readonly IExercisesContext _context;
        readonly IQueryTester _tester;
        readonly IExerciser _exerciser;

        public async Task<ViewResult> Index()
        {
            var usr = CurrentUser;
            if (usr != null)
            {
                var user = CurrentUser.Id;
                var exercises = _context.Exercises
                    .ToList();
                var submissions = _context.Statuses
                    .Where(s => s.User.Id == user)
                    .ToList();
                return View(await Task.Run(() => exercises.Select(e => new ExerciseWSubmissions
                {
                    Exercise = e,
                    Statuses = submissions.Where(s => s.Exercise.ExerciseId == e.ExerciseId)
                })));
            }
            else
            {
                var emptyList = new List<ExerciseStatus>();
                return View(await Task.Run(() => _context.Exercises.Select(e => new ExerciseWSubmissions
                {
                    Exercise = e,
                    Statuses = emptyList
                })));
            }
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
        [Authorize(Roles = Role.ExerciserRoles)]
        public Task<ViewResult> Create(int diagramId)
        {
            return Task.Run(() => View(new CreateExercise { Diagram = diagramId }));
        }

        [HttpPost]
        [Authorize(Roles = Role.ExerciserRoles)]
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
        [Authorize(Roles = Role.SolverRoles)]
        public ViewResult Solve(int id)
        {
            return View(_context.Exercises.Include(x => x.Judge.Diagram).FirstOrDefault(e => e.ExerciseId == id));
        }

        [HttpPost]
        [Authorize(Roles = Role.SolverRoles)]
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
                await RegisterSuccess(exercise, query);
                return View("Success", new KeyValuePair<Exercise, string>(exercise, query));
            }
            else
            {
                await RegisterFailure(exercise, query, output.Value);
                return View("Fail", new FailedSubmission
                {
                    Exercise = exercise,
                    Query = query,
                    Result = output.Value
                });
            }
        }

        Task RegisterSuccess(Exercise exercise, string query)
        {
            var user = CurrentUser;
            _context.Statuses.Add(new ExerciseStatus
            {
                Accepted = true,
                Solution = query,
                Exercise = exercise,
                User = user,
                SubmittedOn = DateTime.Now
            });
            return _context.SaveAsync();
        }

        Task RegisterFailure(Exercise exercise, string query, string output)
        {
            var user = CurrentUser;
            _context.Statuses.Add(new ExerciseStatus
            {
                Accepted = false,
                Resolution = output,
                Solution = query,
                Exercise = exercise,
                User = user,
                SubmittedOn = DateTime.Now
            });
            return _context.SaveAsync();
        }

        public ExerciseController(IExercisesContext ctx, IExerciser exerciser, IQueryTester tester) :
            base(ctx)
        {
            _context = ctx;
            _exerciser = exerciser;
            _tester = tester;
        }
    }
}
