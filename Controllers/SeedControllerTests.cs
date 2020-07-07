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
using SQLExerciser.Models.ViewModel;
using Moq;
using Xunit;

namespace SQLExerciser.Tests.Controllers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class SeedControllerTests
    {
        FakeDbSet<DataSeed> seedMock = new FakeDbSet<DataSeed>();
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
            dbMock.Setup(d => d.Seeds).Returns(seedMock);
        }

        private DbDiagram SampleDiagram =>
            new DbDiagram { DbDiagramId = 1, CreationQuery = "AAA", Diagram = new byte[] { 0 }, Name = "Name" };
        private DataSeed SampleSeed =>
            new DataSeed { DataSeedId = 0, Diagram = SampleDiagram, SeedQuery = "seed" };

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
            SetupDbMock();
            testerMock.Setup(m => m.TestCRUD(It.IsAny<string>(), It.Is<string>(s => s.Contains("seed")))).ReturnsAsync("OK");

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
            string insertQuery = "INSERT INTO employees (id, name, date, faculty) VALUES\n" +
                "(1, 'josh', '20-01-21', 'nebrasca'),\n" +
                "(2, 'jan', '11-05-08', 'colonesca'),\n" +
                "(3, 'opoi', '20-01-21', 'brunesca'),\n" +
                "(4, 'xaq', '02-10-12', 'francesca');" +
                "INSERT INTO wagabah (representation, employeeId, supremacy) VALUES\n" +
                "('binary', 1, 'superior'),\n" +
                "('nonbinary', 2, 'inferior');";
            Seed seed = new Seed(insertQuery, 1, 5);

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
