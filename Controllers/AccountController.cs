using SQLExerciser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using System.Threading.Tasks;

using SQLExerciser.Models.DB;
using SQLExerciser.Models.ViewModel;
using System.Web.UI;

namespace SQLExerciser.Controllers
{
    public class AccountController : UserAccessController
    {
        readonly IIdentintyProvider _identityProvider;
        readonly IExercisesContext _context;

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult MailConfirmation(string accountName)
        {
            if (_identityProvider.ConfimMail(accountName))
            {
                return RedirectToAction("Index", "Home");
            }
            return HttpNotFound();
        }

        [HttpGet]
        public ActionResult Create()
        {
            return View(new AccountCreate { CreationMessage = "" });
        }

        [HttpPost]
        public ActionResult Create(AccountCreate account)
        {
            if (ModelState.IsValid)
            {
                var registerResult = _identityProvider.RegisterUser(account.AccountName, account.Password, account.Email);
                account.CreationMessage = registerResult;
                if (!registerResult.Equals(AccountCreate.OkMessage))
                {
                    ModelState.AddModelError("CreationMessage", account.CreationMessage);
                }
            }
            return View(account);
        }

        [HttpGet]
        
        public ActionResult Logout()
        {
            _identityProvider.LogOut();
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(Login viewModel)
        {
            if (ModelState.IsValid)
            {
                if (_identityProvider.LogIn(viewModel.AccountName, viewModel.Password))
                {
                    return Redirect(viewModel.ReturnUrl);
                }
                ModelState.AddModelError("Given credentials don't match any in the database.", "Given credentials don't match any in the database.");
            }
            return View(viewModel);
        }

        [Authorize(Roles = Role.AdminRoles)]
        [HttpPost]
        public async Task<ActionResult> MarkExerciser(string accountName)
        {
            var user = _context.Users.SingleOrDefault(u => u.Account.Equals(accountName));
            if (user != null)
            {
                user.Role = _context.Roles.Single(r => r.Name.Equals(UserRoles.Exerciser.ToString()));
                await _context.SaveAsync();
                return Content($"User's account: {accountName} has been upgraded to Exerciser role.");
            }
            else
            {
                return HttpNotFound();
            }
        }

        [HttpGet]
        public ActionResult Hidden() => View();

        [HttpGet]
        [Authorize(Roles = Role.SolverRoles)]
        public ActionResult MyAccount()
        {
            var usr = CurrentUser;
            var submits = _context.Statuses
                .Include(x => x.Exercise)
                .Where(s => s.User.Id == usr.Id)
                .ToList();
            return View(new AccountResume
            {
                AccountName = usr.Account,
                Submissions = submits,
                Role = usr.Role.Name,
                Created = usr.Created
            });
        }

        public AccountController(IIdentintyProvider provider, IExercisesContext ctx) 
            : base(ctx)
        {
            _identityProvider = provider;
            _context = ctx;
        }
    }
}