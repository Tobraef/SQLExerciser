using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace SQLExerciser.Models
{
    public interface IMailService
    {
        void SendMail(string receipents, string title, string body);
    }

    public class MailService : IMailService
    {
        public void SendMail(string receipents, string title, string body)
        {
            using (var client = new SmtpClient("smtp.gmail.com"))
            {
                MailMessage mail = new MailMessage(new MailAddress("przemyslaw.majocha@gmail.com"), new MailAddress(receipents))
                {
                    Body = body,
                    Subject = title
                };
                client.Port = 587;
                client.Credentials = new System.Net.NetworkCredential("przemyslaw.majocha@gmail.com", "sadelko123");
                client.EnableSsl = true;
                client.Send(mail);
            }
        }
    }
}