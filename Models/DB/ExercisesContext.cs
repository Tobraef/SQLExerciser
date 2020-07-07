using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
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
        public virtual IDbSet<Role> Roles { get; set; }

        public Task<int> SaveAsync() => SaveChangesAsync();

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
            int v = 1;
            var roles = Enum.GetNames(typeof(UserRoles)).Select(e =>
            {
                int i = v++;
                return new Role { Name = e, Id = i };
            }).ToList();
            roles.ForEach(r => context.Roles.Add(r));

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
                SeedQuery = "INSERT INTO library (id, name) VALUES (1, 'aaa'), (2, 'bbb'), (3, 'ccc');",
                DataSeedId = 1
            };
            context.Seeds.Add(ds1);

            var hashed = new Hasher().HashText("1234");
            var u1 = new User
            {
                Account = "Ja",
                Password = hashed,
                Id = 1,
                MailConfirmed = true,
                Role = roles.Last(),
                Created = DateTime.Now
            };
            context.Users.Add(u1);

            var j1 = new Judge
            {
                AnswerQuery = "SELECT * FROM library;",
                Diagram = diagram1,
                JudgeId = 1
            };
            context.Judges.Add(j1);

            var e1 = new Exercise
            {
                Title = "Title of the exercise",
                Description = "Description of the exercise",
                ExerciseId = 1,
                Difficulty = 15,
                Judge = j1
            };
            context.Exercises.Add(e1);

            context.SaveChanges();
        }
    }
}