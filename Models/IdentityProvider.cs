using SQLExerciser.Models.DB;
using SQLExerciser.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web.Security;

namespace SQLExerciser.Models
{
    public interface IIdentintyProvider
    {
        bool LogIn(string accountName, string password);

        void LogOut();

        string RegisterUser(string accountName, string password, string email);

        bool ConfimMail(string hashedAccountName);
    }

    public class IdentityProvider : IIdentintyProvider
    {
        readonly IExercisesContext _context;
        readonly IHasher _hasher;
        readonly IMailService _mailService;

        void Authenticate(string accountName)
        {
            try { FormsAuthentication.SetAuthCookie(accountName, true); }
            catch (Exception) { }
        }

        void SignOut() => FormsAuthentication.SignOut();

        public bool LogIn(string accountName, string password)
        {
            var account = _context.Users.SingleOrDefault(u => u.Account.Equals(accountName));
            if (account == null || !account.MailConfirmed)
            {
                return false;
            }
            if (_hasher.CompareAgainst(account.Password, password))
            {
                Authenticate(accountName);
                return true;
            }
            return false;
        }

        public void LogOut()
        {
            SignOut();
        }

        public bool ConfimMail(string hashedAccountName)
        {
            var notConfirmedUsers = _context.Users
                .Where(u => !u.MailConfirmed)
                .ToList();
            var confirmed = notConfirmedUsers.Find(u => _hasher.CompareAgainst(hashedAccountName, u.Account));
            if (confirmed == null || confirmed.MailConfirmed)
            {
                return false;
            }
            confirmed.MailConfirmed = true;
            _context.SaveAsync().Wait();
            return true;
        }

        const string welcomeMailSubject = "SQLExerciser account registration";

        string WelcomeMailBody(string accountName) =>
            $"Dear {accountName},\n" +
            $"thank you for registering under our website, I wish you happy solving and improving your skills!\n" +
            $"Here is the activationLink: {Storage.WebsiteName}\\Account\\MailConfirmation\\{_hasher.HashText(accountName)}";

        public string RegisterUser(string accountName, string password, string email)
        {
            if (_context.Users.SingleOrDefault(u => u.Account.Equals(accountName)) != null)
            {
                return "There is already a user with such account name!";
            }
            try
            {
                _mailService.SendMail(email, welcomeMailSubject, WelcomeMailBody(accountName));
            } catch (SmtpFailedRecipientException e)
            {
                return "Failed during sending an email, reason: " + e.Message;
            }
            _context.Users.Add(new User
            {
                Account = accountName,
                Password = _hasher.HashText(password),
                MailConfirmed = false,
                Role = _context.Roles.First(),
                Created = DateTime.Now
            });
            _context.SaveAsync().Wait();
            return AccountCreate.OkMessage;
        }

        public IdentityProvider(IExercisesContext context, IHasher hasher, IMailService service)
        {
            _context = context;
            _hasher = hasher;
            _mailService = service;
        }
    }
}