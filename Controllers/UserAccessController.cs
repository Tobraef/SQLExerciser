using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using System.Web.Mvc;
using SQLExerciser.Models.DB;

namespace SQLExerciser.Controllers
{
    public class UserAccessController : Controller
    {
        readonly IExercisesContext _context;

        protected User CurrentUser => _context.Users.SingleOrDefault(u => u.Account.Equals(User.Identity.Name));

        public UserAccessController(IExercisesContext ctx)
        {
            _context = ctx;
        }
    }
}