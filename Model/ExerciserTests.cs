using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLExerciser.Tests.Framework;
using SQLExerciser.Controllers;
using SQLExerciser.Models.DB;
using SQLExerciser.Models;
using Moq;
using Xunit;

using VS = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLExerciser.Tests.Model
{
    [VS.TestClass]
    public class ExerciserTests
    {
        Exerciser sut;
        readonly FakeDb db = new FakeDb();

        [VS.TestMethod]
        public async Task SuccessOnValidQuerySelectExercise()
        {
            var exercise = db.EmployeesExercises.First();
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, exercise.Judge.AnswerQuery, db.EmployeesSetup);

            Assert.True(result.Key);
        }

        [VS.TestMethod]
        public async Task SuccessOnValidQueryDML()
        {
            var exercise = new Exercise
            {
                Judge = new Judge
                {
                    AnswerQuery = "INSERT INTO employees (id, name) VALUES (50, 'joj');",
                    VerifyQuery = "SELECT * FROM employees WHERE name LIKE 'joj'",
                    Diagram = new DbDiagram { DbDiagramId = 1 }
                }
            };
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, exercise.Judge.AnswerQuery, new List<string>
            {
                "CREATE TABLE employees (id INT PRIMARY KEY, name VARCHAR(max));"
            });

            if (!result.Key)
            {
                Assert.Equal("Nothin", result.Value);
            }
            Assert.True(result.Key);
        }

        [VS.TestMethod]
        public async Task ReturnFailureOnBadSql()
        {
            var exercise = db.EmployeesExercises.First();
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, "BadQuery", new List<string> { "CREATE TABLE tab_1 (id INT PRIMARY KEY);" });

            Assert.False(result.Key);
            Assert.StartsWith("Error", result.Value, StringComparison.OrdinalIgnoreCase);
        }

        [VS.TestMethod]
        public async Task ReturnFailureOnBadSqlDML()
        {
            var exercise = db.EmployeesExercises.First();
            exercise.Judge.VerifyQuery = "AAA";
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, "BadQuery", new List<string> { "CREATE TABLE tab_1 (id INT PRIMARY KEY);" });

            Assert.False(result.Key);
            Assert.Contains("Error", result.Value, StringComparison.OrdinalIgnoreCase);
        }

        [VS.TestMethod]
        public async Task FailureOnWrongAnswer()
        {
            var exercise = db.EmployeesExercises.First();
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, "SELECT * FROM employees WHERE name LIKE 'adda_Dsa_d_Sa_'", db.EmployeesSetup);

            Assert.False(result.Key);
            Assert.DoesNotContain("Error", result.Value, StringComparison.OrdinalIgnoreCase);
        }

        [VS.TestMethod]
        public async Task FailureOnWrongAnswerDML()
        {
            var exercise = new Exercise
            {
                Judge = new Judge
                {
                    AnswerQuery = "INSERT INTO employees (id, name, date, departmentId) VALUES (21, 'joj', GETDATE(), 1);",
                    VerifyQuery = "SELECT * FROM employees WHERE name LIKE 'joj'",
                    Diagram = new DbDiagram { DbDiagramId = 1 }
                }
            };
            sut = new Exerciser(new QueryExecutor());

            var result = await sut.ExecuteExercise(exercise, "INSERT INTO employees (id, name) VALUES (28, 'kokojumbo');", db.EmployeesSetup);

            Assert.False(result.Key);
            Assert.DoesNotContain("Error", result.Value, StringComparison.OrdinalIgnoreCase);
        }
    }
}
