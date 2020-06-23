using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

using Moq;
using SQLExerciser.Models.DB;

namespace SQLExerciser.Tests.Framework
{
    internal class EmptyDb : IExercisesContext
    {
        public EmptyDb() =>
            Users.Add(new User { Account = "ACC", Password = "PASS" });

        public Mock<IExercisesContext> Mock { get; } = new Mock<IExercisesContext>();

        public User CurrentUser => Users.First();

        public IDbSet<User> Users { get; } = new FakeDbSet<User>();

        public IDbSet<ExerciseStatus> Statuses { get; } = new FakeDbSet<ExerciseStatus>();

        public IDbSet<DbDiagram> Diagrams { get; } = new FakeDbSet<DbDiagram>();

        public IDbSet<DataSeed> Seeds { get; } = new FakeDbSet<DataSeed>();

        public IDbSet<Exercise> Exercises { get; } = new FakeDbSet<Exercise>();

        public IDbSet<Judge> Judges { get; } = new FakeDbSet<Judge>();

        public Task<int> SaveAsync() => Mock.Object.SaveAsync();
    }
}
