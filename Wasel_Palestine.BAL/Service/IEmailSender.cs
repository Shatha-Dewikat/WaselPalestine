using System;
using System.Collections.Generic;
using System.Text;

namespace Wasel_Palestine.BAL.Service
{
    public interface IEmailSender
    {
       
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
