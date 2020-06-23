using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;

using System.Threading.Tasks;
using System.Data.Entity;

namespace SQLExerciser.Models.DB
{
    public class ExercisesContext : DbContext, IExercisesContext
    {
        public virtual IDbSet<DbDiagram> Diagrams { get; set; }
        public virtual IDbSet<DataSeed> Seeds { get; set; }
        public virtual IDbSet<Exercise> Exercises { get; set; }
        public virtual IDbSet<Judge> Judges { get; set; }
        public virtual IDbSet<User> Users { get; set; }
        public virtual IDbSet<ExerciseStatus> Statuses { get; set; }

        public Task<int> SaveAsync() => SaveChangesAsync();

        public User CurrentUser => Users.First();

        public ExercisesContext() :
            base("name=ExercisesConnectionString")
        {
        }
    }

    public class DBSeed : DropCreateDatabaseAlways<ExercisesContext>
    {
        string FakeDiagramImagePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "App_Data", "o.png");

        protected override void Seed(ExercisesContext context)
        {
            base.Seed(context);
            byte[] picture;
            using (var fStream = new FileStream(FakeDiagramImagePath, FileMode.Open))
            {
                using (MemoryStream mStream = new MemoryStream())
                {
                    fStream.CopyTo(mStream);
                    picture = mStream.ToArray();
                }
            }
            var diagram1 = new DbDiagram
            {
                CreationQuery = "CREATE TABLE library (id INT PRIMARY KEY, name VARCHAR(100));",
                DbDiagramId = 1,
                Name = "Library",
                Diagram = picture
            };
            context.Diagrams.Add(diagram1);

            var ds1 = new DataSeed
            {
                Diagram = diagram1,
                SeedQuery = "INSERT INTO library VALUES (1, 'aaa'), (2, 'bbb'), (3, 'ccc');",
                DataSeedId = 1
            };
            context.Seeds.Add(ds1);

            var u1 = new User
            {
                Account = "Ja",
                Password = "1234",
                Id = 1
            };
            context.Users.Add(u1);

            context.SaveChanges();
        }
    }
}