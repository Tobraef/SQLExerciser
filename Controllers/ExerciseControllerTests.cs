using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using SQLExerciser.Controllers;
using SQLExerciser.Models.DB;
using SQLExerciser.Models;
using Moq;
using Xunit;

using SQLExerciser.Tests.Framework;

namespace SQLExerciser.Tests.Controllers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class ExerciseControllerTests
    {
        readonly Mock<IExerciser> exerciserMock = new Mock<IExerciser>();
        readonly Mock<IQueryTester> testerMock = new Mock<IQueryTester>();
        readonly FakeDb db = new FakeDb();

        ExerciseController sut;

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task AddExercise()
        {
            testerMock.Setup(t => t.TestSelect(
                It.Is<string>(s => s == db.EmployeesDiagram.CreationQuery),
                It.Is<IEnumerable<string>>(seeds => seeds.Any(s => s == "SOL")),
                It.Is<string>(s => s == "ANS"))).ReturnsAsync("OK");
            sut = new ExerciseController(db, exerciserMock.Object, testerMock.Object);


            var view = await sut.Create(db.EmployeesDiagram.DbDiagramId);
            var vm = view.Model as CreateExercise;
            Assert.Equal(vm.Diagram, db.EmployeesDiagram.DbDiagramId);
            var response = await sut.Create(new CreateExercise
            {
                VerificationQuery = "ANS",
                SolutionQuery = "SOL",
                Title = "TIT",
                Description = "DESC",
                Difficulty = 33,
                Diagram = db.EmployeesDiagram.DbDiagramId
            });

            Assert.Single(db.Exercises,
                e => e.Difficulty == 33 && e.Title.Equals("TIT"));
            Assert.Contains(db.Judges,
                j => j.AnswerQuery == "SOL" && j.VerifyQuery == "ANS");
            Assert.Equal(db.Exercises.Single(e => e.Difficulty == 33).Judge, db.Judges.Single(j => j.AnswerQuery == "SOL"));
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task MarkSuccessOnValidQuery()
        {
            var exercise = db.EmployeesExercises.First();
            exerciserMock.Setup(e => e.ExecuteExercise(It.Is<Exercise>(s => s == exercise), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(() => new KeyValuePair<bool, string>(true, ""));
            sut = new ExerciseController(db, exerciserMock.Object, null);


            var result = await sut.Solve(exercise.ExerciseId, "good");


            Assert.Single(db.Statuses, s => s.Exercise == exercise && s.Accepted && s.User == db.CurrentUser && s.Solution == "good");
            Assert.Equal("Success", result.ViewName);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task MarkFailureOnInvalidQuery()
        {
            var exercise = db.EmployeesExercises.First();
            exerciserMock.Setup(e => e.ExecuteExercise(It.Is<Exercise>(s => s == exercise), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
                .ReturnsAsync(() => new KeyValuePair<bool, string>(false, "Bad answer"));
            sut = new ExerciseController(db, exerciserMock.Object, null);


            var result = await sut.Solve(exercise.ExerciseId, "Not correct");


            Assert.Single(db.Statuses, s => s.Exercise == exercise && !s.Accepted && s.User == db.CurrentUser && 
                s.Solution == "Not correct" && s.Resolution == "Bad answer");
            Assert.Contains("Fail", result.ViewName);
        }
    }
}
