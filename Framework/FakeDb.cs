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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity;

namespace SQLExerciser.Tests.Framework
{
    internal class FakeDb : IExercisesContext
    {
        internal readonly Mock<IExercisesContext> _dbMock;
        IExercisesContext _mockedObject;

        readonly FakeDbSet<DbDiagram> _diagrams = new FakeDbSet<DbDiagram>();
        readonly FakeDbSet<DataSeed> _seeds = new FakeDbSet<DataSeed>();
        readonly FakeDbSet<Exercise> _exercises = new FakeDbSet<Exercise>();
        readonly FakeDbSet<Judge> _judges = new FakeDbSet<Judge>();
        readonly FakeDbSet<User> _users = new FakeDbSet<User>();
        readonly FakeDbSet<ExerciseStatus> _statuses = new FakeDbSet<ExerciseStatus>();

        public User CurrentUser => _users.First();

        public IDbSet<User> Users => _users;

        public IDbSet<ExerciseStatus> Statuses => _statuses;

        public IDbSet<DbDiagram> Diagrams => _diagrams;

        public IDbSet<DataSeed> Seeds => _seeds;

        public IDbSet<Exercise> Exercises => _exercises;

        public IDbSet<Judge> Judges => _judges;

        public DbDiagram EmployeesDiagram => _diagrams.First();
        public DbDiagram DatasDiagram => _diagrams.Last();

        public List<DataSeed> EmployeesSeeds => _seeds.Take(2).ToList();
        public List<DataSeed> DatasSeeds => _seeds.Skip(2).ToList();
        public DataSeed BigEmployeeDataSeed => _seeds.ElementAt(0);
        public DataSeed SmallEmployeeDataSeed => _seeds.ElementAt(1);
        public DataSeed DatasSeed => _seeds.ElementAt(2);

        public List<Judge> EmployeesJudges => _judges.Take(1).ToList();
        public List<Judge> DatasJudges => _judges.Skip(1).ToList();

        public List<Exercise> EmployeesExercises => _exercises.Take(1).ToList();
        public List<Exercise> DatasExercises => _exercises.Skip(1).ToList();

        public List<string> EmployeesSetup
        {
            get
            {
                var l = new List<string>();
                l.Add(EmployeesDiagram.CreationQuery);
                l.AddRange(EmployeesSeeds.Select(s => s.SeedQuery));
                return l;
            }
        }

        public List<string> DatasSetup
        {
            get
            {
                var l = new List<string>();
                l.Add(DatasDiagram.CreationQuery);
                l.AddRange(DatasSeeds.Select(s => s.SeedQuery));
                return l;
            }
        }

        public Mock<IExercisesContext> DbMock => _dbMock;

        private void EnableMock() =>
            _mockedObject = _mockedObject ?? _dbMock.Object;


        public Task<int> SaveAsync()
        {
            EnableMock();
            return _mockedObject.SaveAsync();
        }

        public FakeDb()
        {
            _dbMock = new Mock<IExercisesContext>();
            _diagrams.Add(new DbDiagram
            {
                DbDiagramId = 1,
                CreationQuery =
                "IF object_id('departments') is null CREATE TABLE departments (" +
                "   id int PRIMARY KEY," +
                "   name varchar(100) NOT NULL," +
                "   type varchar(10) " +
                ");" +
                "IF object_id('employees') is null CREATE TABLE employees (" +
                "   id int PRIMARY KEY," +
                "   name varchar(100) NOT NULL," +
                "   date DATE," +
                "   departmentId int FOREIGN KEY REFERENCES departments(id)" +
                ");",
                Diagram = new byte[] { 1, 2, 3 },
                Name = "Employees diagram"
            });
            _diagrams.Add(new DbDiagram
            {
                DbDiagramId = 2,
                CreationQuery =
                "IF object_id('datas') is null CREATE TABLE datas (" +
                "   id int PRIMARY KEY," +
                "   data varchar(255) NOT NULL" +
                ");",
                Diagram = null,
                Name = "Data diagram"
            });

            _seeds.Add(new DataSeed
            {
                Diagram = _diagrams.First(),
                DataSeedId = 1,
                SeedQuery =
                "INSERT INTO departments (id, name, type) VALUES\n" +
                "(1, 'first', 'AAA')," +
                "(2, 'second', 'BBB');" + 
                "INSERT INTO employees (id, name, date, departmentId) VALUES\n" +
                "(1, 'josh', '2020/11/01', 1)," +
                "(2, 'ara', '2021/12/02', 1)," +
                "(3, 'muki', '2005/10/04', 2);"
            });
            _seeds.Add(new DataSeed
            {
                Diagram = _diagrams.First(),
                DataSeedId = 2,
                SeedQuery =
                "INSERT INTO departments (id, name, type) VALUES\n" +
                "(3, 'third', 'CCC')," +
                "(4, 'fourth', 'DDD');" +
                "INSERT INTO employees (id, name, date, departmentId) VALUES\n" +
                "(4, 'josh', '2020/11/01', 3);"
            });
            _seeds.Add(new DataSeed
            {
                Diagram = _diagrams.Last(),
                DataSeedId = 3,
                SeedQuery = "INSERT INTO datas VALUES (id,data)\n" +
                "(1, 'aaaa')," +
                "(2, 'bbbb')," +
                "(3, 'aacbb');"
            });

            _judges.Add(new Judge
            {
                AnswerQuery = "SELECT * FROM employees WHERE name LIKE 'josh'",
                VerifyQuery = "",
                JudgeId = 1,
                Diagram = EmployeesDiagram
            });
            _judges.Add(new Judge
            {
                AnswerQuery = "SELECT id FROM datas WHERE data LIKE 'aa*'",
                VerifyQuery = "",
                JudgeId = 2,
                Diagram = DatasDiagram
            });

            _exercises.Add(new Exercise
            {
                Description = "Find all employees with name josh",
                Difficulty = 10,
                ExerciseId = 1,
                Judge = _judges.First(),
                Title = "Simple search"
            });
            _exercises.Add(new Exercise
            {
                Description = "Find all records, starting with letters aa",
                Difficulty = 11,
                ExerciseId = 2,
                Judge = _judges.Last(),
                Title = "Searching string"
            });

            _users.Add(new User
            {
                Account = "Me",
                Password = "Me123",
                Id = 1
            });
        }
    }
}
