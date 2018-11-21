using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using QRCoder;
using System.Drawing;
using System.IO;
using System.Net.Mime;

namespace SimpleQ.Webinterface.Controllers
{
    public class GroupAdministrationController : Controller
    {
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found");

                var model = new GroupAdministrationModel
                {
                    Departments = db.Departments.Where(d => d.CustCode == CustCode).ToList()
                };

                return View(viewName: "GroupAdministration", model: model);
            }
        }

        [HttpGet]
        public ActionResult Create(string depName)
        {
            if (string.IsNullOrEmpty(depName))
                return Http.BadRequest("Department name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return Http.NotFound("Customer not found.");

                var query = db.Departments.Where(d => d.CustCode == CustCode);

                var dep = new Department
                {
                    DepId = (query.Count() == 0) ? 1 : query.Max(d => d.DepId) + 1,
                    DepName = depName,
                    CustCode = CustCode
                };
                db.Departments.Add(dep);
                db.SaveChanges();

                return Content($"{dep.DepId}", "text/plain");
            }
        }

        [HttpGet]
        public ActionResult Modify(int depId, string depName)
        {
            if (string.IsNullOrEmpty(depName))
                return Http.BadRequest("Department name must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                if (dep == null)
                    return Http.NotFound("Department not found.");

                dep.DepName = depName;
                db.SaveChanges();
            }
            return Http.Ok();
        }

        [HttpGet]
        public ActionResult Delete(int depId)
        {
            using (var db = new SimpleQDBEntities())
            {
                var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                if (dep == null)
                    return Http.NotFound("Department not found.");

                db.Departments.Remove(dep);
                db.SaveChanges();
            }
            return Http.Ok();
        }

        [HttpPost]
        public ActionResult SendInvitations(GroupAdministrationModel req)
        {

            req = new GroupAdministrationModel
            {
                DepId = 1,
                Emails = new List<string> {"dev@simpleq.at", "lukas.schendlinger@a1.net"},
                InvitationText = "sads ma BITTE NED BES"
            };

            if (req == null)
                return Http.BadRequest("Model object must not be null.");

            if (req.Emails == null || req.Emails.Count == 0)
                return Http.BadRequest("Email addresses must not be empty.");

            using (var db = new SimpleQDBEntities())
            {
                if (db.Departments.Where(d => d.DepId == req.DepId && d.CustCode == CustCode).Count() == 0)
                    return Http.NotFound("Department not found.");

                try
                {
                    var gen = new QRCodeGenerator();
                    var qr = new QRCode(gen.CreateQrCode($"{CustCode}{req.DepId}", QRCodeGenerator.ECCLevel.Q));
                    var img = qr.GetGraphic(20);

                    MailMessage msg = new MailMessage
                    {
                        From = new MailAddress("invitation@simpleq.at"),
                        Subject = "SimpleQ Invitation",
                        Body = $"REGISTRATION CODE: {CustCode}{req.DepId}{Environment.NewLine}{req.InvitationText}"
                    };
                    req.Emails.ForEach(e => msg.To.Add(e));

                    var ic = new ImageConverter();
                    byte[] b = (byte[])ic.ConvertTo(img, typeof(byte[]));
                    var contentType = new ContentType
                    {
                        MediaType = MediaTypeNames.Image.Jpeg,
                        Name = "QR Code"
                    };
                    msg.Attachments.Add(new Attachment(new MemoryStream(b), contentType));

                    SmtpClient client = new SmtpClient("smtp.1und1.de", 587)
                    {
                        Credentials = new System.Net.NetworkCredential("invitation@simpleq.at", "123SimpleQ..."),
                        EnableSsl = true
                    };
                    client.Send(msg);

                    return Index();

                }
                catch (Exception ex) when (ex is SmtpException || ex is SmtpFailedRecipientsException)
                {
                    return Http.ServiceUnavailable("Sending failed due to internal error(s).");
                }
            }
        }

        private string CustCode
        {
            get
            {
                return Session["custCode"] as string;
            }
        }
    }
}