using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using MailKit.Net.Smtp;
using MimeKit;
using MailKit.Security;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Resources;

namespace AuthenticationProject.Services
{
    public class MessageServices
    {
        public async static Task SendEmail(string toEmail, string subject, string message)
        {
            try
            {
                var senderEmail = ConfigurationManager.AppSettings["SenderEmail"];
                var senderPassword = ConfigurationManager.AppSettings["SenderPassword"];
                var displayName = "CoDevRun";
                MimeMessage mailMessage = new MimeMessage();
                mailMessage.To.Add(new MailboxAddress(toEmail));
                mailMessage.From.Add(new MailboxAddress(displayName, senderEmail));
                mailMessage.Subject = subject; 
                mailMessage.Body = new TextPart("html") { Text = message};

                 
                using (SmtpClient smtp = new SmtpClient())
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
                    {
                        return true;
                    };
                    smtp.Connect("smtp.gmail.com", 587, useSsl: false);
                    smtp.Authenticate(senderEmail, senderPassword);
                    await smtp.SendAsync(mailMessage);
                    await smtp.DisconnectAsync(true);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
