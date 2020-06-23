using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp.RuntimeBinder;

using SQLExerciser.Controllers;
using SQLExerciser.Models;
using SQLExerciser.Models.ViewModel;
using SQLExerciser.Models.DB;
using SQLExerciser.Tests.Framework;

using Xunit;
using Moq;
using VS = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLExerciser.Tests.E2E
{
    [VS.TestClass]
    public class EntityTests
    {
        readonly EmptyDb db = new EmptyDb();
        readonly IQueryExecutor executor;
        readonly IQueryTester tester;
        readonly IExerciser exerciser;

        public EntityTests()
        {
            executor = new QueryExecutor();
            tester = new QueryTester(executor);
            exerciser = new Exerciser(executor);
        }

        [VS.TestMethod]
        public async Task CreateDiagramSeedsAndExercise()
        {
            db.Mock.Setup(d => d.SaveAsync()).ReturnsAsync(1);
            {
                DiagramController diagramController = new DiagramController(db, tester);
                var diagrams = (await diagramController.Index()).Model as List<DbDiagram>;

                Assert.Empty(diagrams);

                const string q1 = "CREATE TABLE tab_1(id int PRIMARY KEY, name VARCHAR(30));";

                await diagramController.Create(new DbDiagram
                {
                    Name = "Diagram1",
                    CreationQuery = q1,
                    Diagram = new byte[] { 1, 2, 3 }
                });
                diagrams = (await diagramController.Index()).Model as List<DbDiagram>;

                Assert.Single(diagrams, d => d.Name == "Diagram1" && d.CreationQuery == q1);
            }
            {
                SeedController seedController = new SeedController(db, tester);
                var diagram = db.Diagrams.Single();
                var seeds = (await seedController.Index(diagram.DbDiagramId)).Model as List<Seed>;

                Assert.Empty(seeds);

                var model1 = seedController.Create(diagram.DbDiagramId).Result.Model as SeedCreateViewModel;
                Assert.Equal(model1.Diagram, new byte[] { 1, 2, 3 });
                Assert.Equal(model1.DiagramId, diagram.DbDiagramId);

                const string seedQuery = "INSERT INTO tab_1 (id, name) VALUES (1, 'aaa'), (2, 'bbb'), (3, 'ccc');";

                await seedController.Create(new SeedCreateViewModel
                {
                    DiagramId = diagram.DbDiagramId,
                    SeedQuery = seedQuery
                });

                Assert.Single(db.Seeds, s => s.Diagram == diagram && s.SeedQuery == seedQuery);
            }
            {
                ExerciseController exerciseController = new ExerciseController(db, exerciser, tester);
                var diagram = db.Diagrams.Single();
                var seed = db.Seeds.Single();
                const string query = "SELECT id FROM tab_1 WHERE name LIKE 'aaa';";

                var testQ = await exerciseController.TestQuery(query, diagram.DbDiagramId);
                await exerciseController.Create(new CreateExercise
                {
                    Diagram = diagram.DbDiagramId,
                    Seeds = seed.DataSeedId.ToString(),
                    Description = "Exercising",
                    Title = "Exercise_1",
                    SolutionQuery = query,
                    Difficulty = 30
                });

                var exercise = db.Exercises.Single();
                var result = await exerciseController.Solve(exercise.ExerciseId, "SELECT id FROM tab_1 WHERE name NOT LIKE 'bbb' AND name NOT LIKE 'ccc'");

                Assert.Single(db.Statuses, s => s.Accepted);

                result = await exerciseController.Solve(exercise.ExerciseId, "SELECT name FROM tab_1 WHERE name LIKE 'aaa'");

                Assert.All(db.Statuses, s => Assert.False(s.Resolution != null && s.Resolution.Contains("Error")));
                Assert.Single(db.Statuses, s => !s.Accepted);

                result = await exerciseController.Solve(exercise.ExerciseId, "SELECT sad");

                Assert.Single(db.Statuses, s => s.Resolution != null && s.Resolution.Contains("Error") && !s.Accepted);
            }
        }
    }
}
