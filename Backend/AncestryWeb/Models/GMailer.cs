using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Reflection;

namespace AncestryWeb.Models
{
    public class GMailer
    {
        public static string GmailUsername { get; set; }
        public static string GmailPassword { get; set; }
        public static string GmailHost { get; set; }
        public static int GmailPort { get; set; }
        public static bool GmailSSL { get; set; }

        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }

        public GMailer()
        {
            GmailHost = "smtp.gmail.com";
            GmailPort = 25;
            GmailSSL = true;
            GmailUsername = "oldspun167@gmail.com";
            GmailPassword = "38712jesus";
            

        }

        public void Send(string ToEmail, string Subject, string Body)
        {
            SmtpClient smtp = new SmtpClient();
            smtp.Host = GmailHost;
            smtp.Port = GmailPort;
            smtp.EnableSsl = GmailSSL;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(GmailUsername, GmailPassword);

            using (var message = new MailMessage(GmailUsername, ToEmail))
            {
                message.Subject = Subject;
                message.Body = Body;
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
        }
    }
}