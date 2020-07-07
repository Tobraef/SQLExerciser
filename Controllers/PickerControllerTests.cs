using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web;

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
    public class PickerControllerTests
    {
        PickerController sut;

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task PickSeedTest()
        {
            const string returnUrl = "Home/Index";
            var repository = new FakeDb();
            var storageMock = new Mock<IStorage>();
            storageMock.Setup(s => s.StoreValue(It.Is<DataSeed>(seed => repository.EmployeesSeeds.Last().Equals(seed))));
            storageMock.Setup(s => s.StoreValue(It.Is<string>(t => t.Equals(returnUrl))));
            storageMock.Setup(s => s.GetValue<string>()).Returns(returnUrl);
            sut = new PickerController(repository, storageMock.Object);


            var model = (await sut.PickSeed(repository.EmployeesDiagram.DbDiagramId, returnUrl)).Model as List<Seed>;
            var redirect = await sut.PickSeed(repository.EmployeesSeeds.Last().DataSeedId);


            Assert.Equal(returnUrl, redirect.Url);
            Assert.True(model.All(m => m.DiagramId == repository.EmployeesDiagram.DbDiagramId));
            Assert.Equal(2, model.Count);
        }

        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task PickDiagramTest()
        {
            const string returnUrl = "Home/Index";
            var repository = new FakeDb();
            var storageMock = new Mock<IStorage>();
            storageMock.Setup(s => s.StoreValue(It.Is<DbDiagram>(diagram => repository.EmployeesDiagram.DbDiagramId == diagram.DbDiagramId)));
            storageMock.Setup(s => s.StoreValue(It.Is<string>(t => t.Equals(returnUrl))));
            storageMock.Setup(s => s.GetValue<string>()).Returns(returnUrl);
            sut = new PickerController(repository, storageMock.Object);


            var model = (await sut.PickDiagram(returnUrl)).Model as List<DbDiagram>;
            var redirect = sut.PickDiagram(repository.EmployeesDiagram.DbDiagramId) as RedirectResult;


            Assert.Equal(returnUrl, redirect.Url);
            Assert.Equal(repository.EmployeesDiagram.DbDiagramId, model.First().DbDiagramId);
            Assert.Equal(repository.DatasDiagram.DbDiagramId, model.Last().DbDiagramId);
        }
    }
}
