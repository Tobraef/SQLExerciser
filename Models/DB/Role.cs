using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SQLExerciser.Models.DB
{
    enum UserRoles : int
    {
        Solver = 1,
        Exerciser,
        Admin
    }

    public class Role
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, NameValidator]
        public string Name { get; set; }

        public const string AdminRoles = "Admin";
        public const string ExerciserRoles = "Exerciser";
        public const string SolverRoles = "Solver";

        private class NameValidatorAttribute : ValidationAttribute
        {
            public override bool RequiresValidationContext => false;

            public override bool IsValid(object value)
            {
                return Enum.TryParse((string)value, out UserRoles _);
            }
        }
    }

    internal static class RoleExtensions
    {
        internal static string[] AllBelow(this UserRoles role)
        {
            var names = Enum.GetNames(typeof(UserRoles));
            return names.Take((int)role).ToArray();
        }

        internal static TEnum ParseToEnum<TEnum>(this string text)
        {
            return (TEnum)Enum.Parse(typeof(TEnum), text);
        }
    }
}