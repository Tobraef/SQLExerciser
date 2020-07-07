using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SQLExerciser.Tests.Framework;
using SQLExerciser.Controllers;
using SQLExerciser.Models.DB;
using SQLExerciser.Models.ViewModel;
using SQLExerciser.Models;
using Moq;
using Xunit;

using VS = Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SQLExerciser.Tests.Model
{
    [VS.TestClass]
    public class IdentityTests
    {
        readonly FakeDb db = new FakeDb();
        readonly Hasher hasher = new Hasher();
        readonly Mock<IMailService> mailMock = new Mock<IMailService>();
        IdentityProvider sut;

        [VS.TestMethod]
        public void HashingTest()
        {
            string password = "Sadelko123!";
            
            var hashed = hasher.HashText(password);

            Assert.True(hasher.CompareAgainst(hashed, password));
            Assert.False(hasher.CompareAgainst(hashed, "Sadelko123@"));
            Assert.False(hasher.CompareAgainst(hashed, ""));
            Assert.False(hasher.CompareAgainst(hashed, "1 == 1"));
        }

        const string receiver = "myMail@gmail.com";
        const string accName = "MyAcc";
        const string password = "MyPass123!";
        string capturedMailBody;

        void ExpectMail() =>
            mailMock.Setup(m => m.SendMail(
                It.Is<string>(s => s.Equals(receiver)),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Callback<string, string, string>((a, b, c) => capturedMailBody = c);

        void VerifyMailSent() =>
            mailMock.Verify(m => m.SendMail(
                It.Is<string>(s => s.Equals(receiver)),
                It.IsAny<string>(),
                It.IsAny<string>()));

        [VS.TestMethod]
        public void CreateAccountsWithSameAccoutName()
        {
            db.Users.Remove(db.Users.Single());
            sut = new IdentityProvider(db, hasher, mailMock.Object);

            ExpectMail();
            var result = sut.RegisterUser(accName, password, receiver);
            Assert.Equal(AccountCreate.OkMessage, result);
            VerifyMailSent();

            result = sut.RegisterUser(accName, password, receiver);
            Assert.NotEqual(AccountCreate.OkMessage, result);

            ExpectMail();
            result = sut.RegisterUser(accName + "1", password, receiver);
            Assert.Equal(AccountCreate.OkMessage, result);
            VerifyMailSent();
        }

        [VS.TestMethod]
        public void CreateAccountFailToLoginWithoutMailConfirmed()
        {
            var hashMock = new Mock<IHasher>();
            hashMock.Setup(h => h.HashText(It.Is<string>(pas => pas.Equals(password))));
            db.Users.Remove(db.Users.Single());
            sut = new IdentityProvider(db, hashMock.Object, mailMock.Object);

            ExpectMail();
            var result = sut.RegisterUser(accName, password, receiver);
            Assert.Equal(AccountCreate.OkMessage, result);
            VerifyMailSent();

            Assert.False(sut.LogIn(accName, "not"));
            Assert.False(sut.LogIn(accName, password));
        }

        [VS.TestMethod]
        public void CreateAccountAndLoginWithConfirmation()
        {
            db.Users.Remove(db.Users.Single());
            sut = new IdentityProvider(db, hasher, mailMock.Object);

            ExpectMail();
            var result = sut.RegisterUser(accName, password, receiver);
            Assert.Equal(AccountCreate.OkMessage, result);
            VerifyMailSent();
            string hashedAccount = capturedMailBody.Substring(capturedMailBody.IndexOf("Account\\MailConfirmation") + 
                @"Account\MailConfirmation".Length + 1);

            const string otherAcc = "asdf";
            const string otherPass = "qwe";

            ExpectMail();
            result = sut.RegisterUser(otherAcc, otherPass, receiver);
            Assert.Equal(AccountCreate.OkMessage, result);
            VerifyMailSent();

            Assert.Equal(2, db.Users.Count());
            Assert.False(sut.ConfimMail("RandomGibberish="));
            Assert.True(sut.ConfimMail(hashedAccount));
            Assert.False(sut.ConfimMail(hashedAccount));

            Assert.False(sut.LogIn(accName + "1", password));
            Assert.True(sut.LogIn(accName, password));
            Assert.False(sut.LogIn(otherAcc, otherPass));

            hashedAccount = capturedMailBody.Substring(capturedMailBody.IndexOf("Account\\MailConfirmation") +
                @"Account\MailConfirmation".Length + 1);
            Assert.True(sut.ConfimMail(hashedAccount));

            Assert.True(sut.LogIn(otherAcc, otherPass));
        }
    }
}
