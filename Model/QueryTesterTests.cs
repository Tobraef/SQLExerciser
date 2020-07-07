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
    public class QueryTesterTests
    {
        QueryTester sut;
        Mock<IQueryExecutor> executor = new Mock<IQueryExecutor>();

        [VS.TestMethod]
        public void ReturnErrorOnInvalidInput()
        {
            string setupQuery = "CREATE TABLE";
            executor.Setup(e => e.ExecuteWithRollback(It.IsAny<IEnumerable<string>>(), It.IsAny<IEnumerable<string>>())).Throws(new Exception("Bad format"));
            executor.Setup(e => e.ExecuteSetup(It.Is<string>(r => r == setupQuery))).Throws(new Exception("Bad format"));
            executor.Setup(e => e.ExecuteCRUD(It.Is<string>(r => r == setupQuery))).Throws(new Exception("Bad format"));
            executor.Setup(e => e.ExecuteQuery(It.Is<string>(r => r == setupQuery))).Throws(new Exception("Bad format"));
            sut = new QueryTester(executor.Object);

            var r1 = sut.TestTableSetup(setupQuery);
            var r2 = sut.TestCRUD(setupQuery, "INSERT");
            var r3 = sut.TestSelect(setupQuery, new string[] { "INSERT" }, "SELECT");

            Assert.StartsWith("ERROR", r1.Result);
            Assert.StartsWith("ERROR", r2.Result);
            Assert.StartsWith("ERROR", r3.Result);
        }

        [VS.TestMethod]
        public void ReturnValidOutputOnValidInput()
        {
            string setupQuery = "CREATE TABLE";
            executor.Setup(e => e.ExecuteWithRollback(It.IsAny<IEnumerable<string>>(),
                It.IsAny<IEnumerable<string>>())).Returns(new List<List<RowResult>> { new List<RowResult> { new RowResult() } });
            executor.Setup(e => e.ExecuteSetup(It.Is<string>(r => r == setupQuery)));
            executor.Setup(e => e.ExecuteCRUD(It.Is<string>(r => r == "INSERT"))).Returns(5);
            executor.Setup(e => e.ExecuteQuery(It.Is<string>(r => r == "SELECT"))).Returns(() => {
                var row = new RowResult();
                row.Ints.Add(1);
                return new List<RowResult> { row };
            });
            sut = new QueryTester(executor.Object);

            var r1 = sut.TestTableSetup(setupQuery);
            var r2 = sut.TestCRUD(setupQuery, "INSERT");
            var r3 = sut.TestSelect(setupQuery, new string[] { "INSERT" }, "SELECT");

            Assert.DoesNotMatch("ERROR.*", r1.Result);
            Assert.DoesNotMatch("ERROR.*", r2.Result);
            Assert.DoesNotMatch("ERROR.*", r3.Result);
        }
    }
}
