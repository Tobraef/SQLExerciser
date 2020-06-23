using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using SQLExerciser.Tests.Framework;
using SQLExerciser.Controllers;
using SQLExerciser.Models.DB;
using SQLExerciser.Models;
using Moq;
using Xunit;

namespace SQLExerciser.Tests.Controllers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class SeedControllerTests
    {
        Mock<IDbSet<DataSeed>> seedMock = new Mock<IDbSet<DataSeed>>();
        Mock<IExercisesContext> dbMock = new Mock<IExercisesContext>();
        FakeDbSet<DbDiagram> diagramMock = new FakeDbSet<DbDiagram>();
        Mock<IQueryTester> testerMock = new Mock<IQueryTester>();
        SeedController sut;

        void SetupDbMock()
        {
            List<DbDiagram> diagrams = new List<DbDiagram> {
                SampleDiagram,
                new DbDiagram { CreationQuery = "BBB", DbDiagramId = 2 }
            };
            diagramMock.Add(diagrams.First());
            diagramMock.Add(diagrams.Last());
            dbMock.Setup(d => d.Diagrams).Returns(diagramMock);
            dbMock.Setup(d => d.Seeds).Returns(seedMock.Object);
        }

        private DbDiagram SampleDiagram =>
            new DbDiagram { DbDiagramId = 1, CreationQuery = "AAA", Diagram = new byte[] { 0 }, Name = "Name" };
        private DataSeed SampleSeed =>
            new DataSeed { DataSeedId = 0, Diagram = SampleDiagram, Judges = new List<Judge>(), SeedQuery = "seed" };

        bool CompareSeeds(DataSeed actualy, DataSeed expected)
        {
            Assert.NotNull(actualy);

            Assert.Equal(expected.SeedQuery, actualy.SeedQuery);
            Assert.Equal(expected.Diagram.DbDiagramId, actualy.Diagram.DbDiagramId);
            Assert.Equal(expected.DataSeedId, actualy.DataSeedId);
            return true;
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task CreateSeedOnValidInput()
        {
            seedMock.Setup(m => m.Add(It.Is<DataSeed>(i => CompareSeeds(i, SampleSeed))));
            SetupDbMock();
            testerMock.Setup(m => m.TestTableSetup(It.Is<string>(t => t.Equals("AAA")))).ReturnsAsync("OK");
            testerMock.Setup(m => m.TestTableSetup(It.Is<string>(t => t.Equals("seed")))).ReturnsAsync("OK");

            sut = new SeedController(dbMock.Object, testerMock.Object);
            await sut.Create(1);
            await sut.Create(new SeedCreateViewModel
            {
                DiagramId = SampleDiagram.DbDiagramId,
                SeedQuery = "seed"
            });

            dbMock.VerifyAll();
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public void ParsingInsertQueryTest()
        {
            var exercises = new List<string> { "Find ids", "Date diff", "Starts with faculty" };
            string insertQuery = "INSERT INTO employees (id, name, date, faculty) VALUES\n" +
                "(1, 'josh', '20-01-21', 'nebrasca'),\n" +
                "(2, 'jan', '11-05-08', 'colonesca'),\n" +
                "(3, 'opoi', '20-01-21', 'brunesca'),\n" +
                "(4, 'xaq', '02-10-12', 'francesca');" +
                "INSERT INTO wagabah (representation, employeeId, supremacy) VALUES\n" +
                "('binary', 1, 'superior'),\n" +
                "('nonbinary', 2, 'inferior');";
            Seed seed = new Seed(insertQuery, new byte[] { 1, 2, 3 }, 5, exercises);

            var tables = seed.TableSeeds;
            var employeeTable = tables.First();
            var wagabahTable = tables.Last();

            Assert.Equal(4, employeeTable.Rows.Count);
            Assert.Equal(new string[] { "id", "name", "date", "faculty" }, employeeTable.TableHeaders);
            Assert.Equal(new string[] { "1", "josh", "20-01-21", "nebrasca" }, employeeTable.Rows.First());
            Assert.Equal(new string[] { "4", "xaq", "02-10-12", "francesca" }, employeeTable.Rows.Last());

            Assert.Equal(2, wagabahTable.Rows.Count);
            Assert.Equal(new string[] { "representation", "employeeId", "supremacy" }, wagabahTable.TableHeaders);
            Assert.Equal(new string[] { "binary", "1", "superior" }, wagabahTable.Rows.First());
            Assert.Equal(new string[] { "nonbinary", "2", "inferior"}, wagabahTable.Rows.Last());
        }
    }
}
