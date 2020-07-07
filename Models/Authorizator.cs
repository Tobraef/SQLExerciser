using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using SQLExerciser.Models.DB;

namespace SQLExerciser.Models
{
    public class Authorizator : RoleProvider
    {
        readonly IExercisesContext _context = new ExercisesContext();

        public override string ApplicationName { get => AppDomain.CurrentDomain.ApplicationIdentity.FullName; set { } }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return _context.Users.Where(u => u.Role.Name.Equals(roleName)).Select(u => u.Account).ToArray();
        }

        public override string[] GetAllRoles()
        {
            return _context.Roles.Select(r => r.Name).ToArray();
        }

        public override string[] GetRolesForUser(string username)
        {
            var role = _context.Users.Single(u => u.Account.Equals(username)).Role;
            var parsed = role.Name.ParseToEnum<UserRoles>();
            return parsed.AllBelow();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            return _context.Users.Where(u => u.Role.Name.Equals(roleName)).Select(u => u.Account).ToArray();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Any(r => r.Equals(roleName));
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            return _context.Roles.Any(r => r.Name.Equals(roleName));
        }
    }
}