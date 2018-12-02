using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Email
    {
        public static bool Send(string from, string to, string subject, string body, bool isHtml = false, params Attachment[] attachments)
        {
            return Send(from, new string[] { to }, subject, body, isHtml, attachments);
        }

        public static bool Send(string from, string[] to, string subject, string body, bool isHtml = false, params Attachment[] attachments)
        {
            try
            {
                MailMessage msg = new MailMessage
                {
                    From = new MailAddress(from),
                    Subject = subject,
                    Body = body
                };
                to.ToList().ForEach(t => msg.To.Add(t));
                attachments.ToList().ForEach(a => msg.Attachments.Add(a));
                msg.IsBodyHtml = isHtml;

                SmtpClient client = new SmtpClient("smtp.1und1.de", 587)
                {
                    Credentials = new System.Net.NetworkCredential(from, "123SimpleQ..."),
                    EnableSsl = true
                };

                client.Send(msg);

                return true;
            }
            catch (SmtpException)
            {
                return false;
            }
        }
    }
}