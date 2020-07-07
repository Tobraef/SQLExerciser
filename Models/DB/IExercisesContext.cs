using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Data.Entity;

namespace SQLExerciser.Models.DB
{
    public interface IExercisesContext
    {
        IDbSet<DbDiagram> Diagrams { get; }
        IDbSet<DataSeed> Seeds { get; }
        IDbSet<Exercise> Exercises { get; }
        IDbSet<Judge> Judges { get; }
        IDbSet<User> Users { get; }
        IDbSet<ExerciseStatus> Statuses { get; }
        IDbSet<Role> Roles { get; set; }

        Task<int> SaveAsync();
    }

    public static class Extensions
    {
        public static Task<T> FindAsync<T>(this IDbSet<T> stuff, object pk) where T: class
        {
            return Task.Run(() => stuff.Find(pk));
        }
    }
}