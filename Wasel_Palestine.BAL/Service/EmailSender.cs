using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Wasel_Palestine.BLL.Service
{
    public class EmailSender : IEmailSender
    {
       
        private readonly string _host = "smtp.gmail.com";
        private readonly int _port = 587;
        private readonly string _username = "sdwikat93@gmail.com";
        private readonly string _password = "kpzu vnhs coiy ekve"; 
        private readonly string _from = "sdwikat93@gmail.com";


        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using var client = new SmtpClient(_host, _port)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_username, _password)
            };

            using var mail = new MailMessage(_from, email, subject, htmlMessage)
            {
                IsBodyHtml = true
            };

            await client.SendMailAsync(mail);
        }
    }
}