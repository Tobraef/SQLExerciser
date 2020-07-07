using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.Entity;

using SQLExerciser.Controllers;
using SQLExerciser.Models.DB;
using SQLExerciser.Models;
using Moq;

using Xunit;
using System.Web;

namespace SQLExerciser.Tests.Controllers
{
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class DiagramControllerTests
    {
        private Mock<IExercisesContext> contextMocker = new Mock<IExercisesContext>();
        private Mock<IDbSet<DbDiagram>> itemsMocker = new Mock<IDbSet<DbDiagram>>();
        Mock<IQueryTester> testerMocker = new Mock<IQueryTester>();
        private DiagramController sut;

        private string SampleQuery => "CREATE TABLE HASH;";

        private DbDiagram SampleDiagram =>
            new DbDiagram { CreationQuery = SampleQuery, Diagram = new byte[] { 1, 2, 3 }, Name = "Hash", DbDiagramId = 0xc4 };

        private bool CompareSample(DbDiagram received, DbDiagram expected)
        {
            var toComp = expected;
            Assert.NotNull(received);
            return
                received.Name.Equals(toComp.Name) &&
                received.CreationQuery.Equals(received.Diagram);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task AddDiagramOnValidQuery()
        {
            itemsMocker.Setup(i => i.Add(It.Is<DbDiagram>(item => CompareSample(item, SampleDiagram))));

            contextMocker.Setup(c => c.Diagrams).Returns(itemsMocker.Object);
            testerMocker.Setup(c => c.TestTableSetup(
                It.Is<string>(query => SampleQuery.Equals(query))))
                .ReturnsAsync("OK");
            contextMocker.Setup(c => c.SaveAsync()).Returns(Task.Run(() => 1)).Verifiable();
            sut = new DiagramController(contextMocker.Object, testerMocker.Object);

            await sut.Create();
            await sut.Create(SampleDiagram, null);

            contextMocker.Verify(c => c.SaveAsync(), () => Times.Once());
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task ReturnToUserOnInvalidQuery()
        {
            itemsMocker.Setup(i => i.Add(It.Is<DbDiagram>(item => CompareSample(item, SampleDiagram))));
            contextMocker.Setup(c => c.Diagrams).Returns(itemsMocker.Object);
            testerMocker.Setup(c => c.TestTableSetup(
                It.Is<string>(query => SampleQuery.Equals(query))))
                .ReturnsAsync("ERROR");
            sut = new DiagramController(contextMocker.Object, testerMocker.Object);

            await sut.Create();
            await sut.Create(SampleDiagram, null);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task RemoveDiagram()
        {
            itemsMocker.Setup(i => i.Remove(It.Is<DbDiagram>(item => item.DbDiagramId == SampleDiagram.DbDiagramId)));

            contextMocker.Setup(c => c.Diagrams).Returns(itemsMocker.Object);
            sut = new DiagramController(contextMocker.Object, testerMocker.Object);

            await sut.Remove(SampleDiagram.DbDiagramId);
        }

        private DbDiagram OtherSampleDiagram =>
            new DbDiagram { CreationQuery = "cool query", Diagram = null, Name = "shah", DbDiagramId = 0xfa };

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task EditDiagram()
        {
            var firstDiagram = SampleDiagram;
            itemsMocker.Setup(i => i.Find(It.Is<int>(id => id == firstDiagram.DbDiagramId))).Returns(firstDiagram);
            itemsMocker.Setup(i => i.Remove(It.Is<DbDiagram>(item => CompareSample(item, firstDiagram))));
            itemsMocker.Setup(i => i.Add(It.Is<DbDiagram>(item => CompareSample(item, OtherSampleDiagram))));
            testerMocker.Setup(c => c.TestTableSetup(It.Is<string>(q => OtherSampleDiagram.CreationQuery.Equals(q)))).ReturnsAsync("OK").Verifiable();

            contextMocker.Setup(c => c.Diagrams).Returns(itemsMocker.Object);
            sut = new DiagramController(contextMocker.Object, testerMocker.Object);

            await sut.Edit(firstDiagram.DbDiagramId);
            await sut.Edit(OtherSampleDiagram);

            testerMocker.Verify(c => c.TestTableSetup(OtherSampleDiagram.CreationQuery), Times.Once);
        }
    }
}
