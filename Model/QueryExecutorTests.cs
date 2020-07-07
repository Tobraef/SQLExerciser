using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Web;

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
    public class QueryExecutorTests
    {
        QueryExecutor sut;

        [VS.TestMethod]
        public void FullSetupSampleDB()
        {
            sut = new QueryExecutor();


            sut.ExecuteSetup("IF object_id('table_1') is null CREATE TABLE table_1 (" +
                "id INT PRIMARY KEY," +
                "name VARCHAR(255) NOT NULL);");
            sut.ExecuteCRUD("INSERT INTO table_1 (id, name) VALUES (1, 'aaa'), (2, 'bbb');");
            var result = sut.ExecuteQuery("SELECT * FROM table_1");
            sut.Dispose();


            Assert.ThrowsAny<SqlException>(() => sut.ExecuteQuery("SELECT * FROM table_1"));
            Assert.Contains(result, i => i.Ints.Contains(1) && i.Strings.Contains("aaa"));
            Assert.Contains(result, i => i.Ints.Contains(2) && i.Strings.Contains("bbb"));
        }

        [VS.TestMethod]
        public void ThrowOnInvalidInput()
        {
            sut = new QueryExecutor();

            Assert.Throws<SqlException>(() => sut.ExecuteSetup("CREATE TABLE table CREATE TABLE;"));
            Assert.Throws<SqlException>(() => sut.ExecuteCRUD("INSERT INTO table (id, name) VALUES (1, 'aaa'), (2, 'bbb');"));
            Assert.Throws<SqlException>(() => sut.ExecuteQuery("SELECT SELECT FROM table"));
        }

        [VS.TestMethod]
        public void ClearAllTablesOnDispose()
        {
            sut = new QueryExecutor();
            sut.ExecuteSetup("CREATE TABLE tab_1 (id int PRIMARY KEY); CREATE TABLE tab_2 (id int," +
                "fki int FOREIGN KEY REFERENCES tab_1(id));");
            sut.ExecuteCRUD("INSERT INTO tab_1 VALUES (1); INSERT INTO tab_2 VALUES (2, 1)");

            var r1 = sut.ExecuteQuery("SELECT * FROM tab_1").Single().Ints.First();
            var r2 = sut.ExecuteQuery("SELECT * FROM tab_2").Single().Ints.First();

            Assert.Equal(1, r1);
            Assert.Equal(2, r2);

            sut.Dispose();

            Assert.Throws<SqlException>(() => sut.ExecuteQuery("SELECT * FROM tab_1"));
            Assert.Throws<SqlException>(() => sut.ExecuteQuery("SELECT * FROM tab_2"));

            sut.ExecuteSetup("CREATE TABLE tab_2 (id int);");

            Assert.Throws<SqlException>(() => sut.ExecuteSetup("CREATE TABLE tab_2 (id int);"));

            sut.Dispose();
        }

        [VS.TestMethod]
        public void RollbackProperly()
        {
            sut = new QueryExecutor();
            sut.ExecuteSetup("CREATE TABLE tab_1 (id int PRIMARY KEY);");
            var r1 = sut.ExecuteWithRollback(new List<string> { "INSERT INTO tab_1 (id) VALUES (1); INSERT INTO tab_1 (id) VALUES (2);" },
                new List<string> { "SELECT * FROM tab_1;" });
            
            Assert.Equal(2, r1.Single().Count());
            Assert.Contains(r1, r => r.All(row => row.Ints.Contains(1) || row.Ints.Contains(2)));

            var r2 = sut.ExecuteQuery("SELECT * FROM tab_1");

            Assert.Empty(r2);

            sut.ExecuteCRUD("INSERT INTO tab_1 (id) VALUES (1);");
            Assert.Throws<SqlException>(() => sut.ExecuteCRUD("INSERT INTO tab_1 (id) VALUES (1);"));
            sut.Dispose();
        }
    }
}
