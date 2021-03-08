using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Mail;

namespace TrackerLibrary
{
    public static class EmailLogic
    {
        public static void SendEmail(string to, string subject, string body)
        {
            // Set up the actaul mail message
            MailAddress fromMailAddress = new MailAddress(GlobalConfig.AppKeyLoopup("senderEmail"), GlobalConfig.AppKeyLoopup("senderDisplayName"));

            MailMessage mail = new MailMessage();
            mail.To.Add(to);
            mail.From = fromMailAddress;
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;  // this is where you set whether or not you're sending HTML enabled emails.

            SmtpClient client = new SmtpClient();
            client.Send(mail);

            // we haven't told it what our mail server is, where it talked to it  what the credentials are, what ports talk to.
            // well we don't have to do it right here, because Microsoft built a system dotnet mail grouping. they have create a spot
            // in the app config to put email configuration.
        }
    }
}
