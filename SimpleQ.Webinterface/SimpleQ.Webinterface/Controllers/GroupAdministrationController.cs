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
using System.Security.Claims;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class GroupAdministrationController : Controller
    {
        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                var model = new GroupAdministrationModel
                {
                    Departments = db.Departments.Where(d => d.CustCode == CustCode).ToList()
                };

                ViewBag.emailConfirmed = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().EmailConfirmed;
                return View("GroupAdministration", model);
            }
        }

        [HttpPost]
        public ActionResult SendInvitations(GroupAdministrationModel req)
        {
            bool err = false;
            //req = new GroupAdministrationModel
            //{
            //    DepId = 1,
            //    Emails = new List<string> { "dev@simpleq.at", "lukas.schendlinger@a1.net" },
            //    InvitationText = "sads ma BITTE NED BES"
            //};

            if (req == null)
                AddModelError("Model", "Model object must not be null.", ref err);

            if (req.Emails == null || req.Emails.Count == 0)
                AddModelError("Emails", "Email addreses must not be empty.", ref err);

            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == CustCode))
                    return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });

                if (db.Departments.Where(d => d.DepId == req.DepId && d.CustCode == CustCode).Count() == 0)
                    AddModelError("DepId", "Department not found.", ref err);

                if (err) return Index();


                var gen = new QRCodeGenerator();
                var qr = new QRCode(gen.CreateQrCode($"{CustCode}{req.DepId}", QRCodeGenerator.ECCLevel.Q));
                var img = qr.GetGraphic(20);
                var ic = new ImageConverter();
                byte[] b = (byte[])ic.ConvertTo(img, typeof(byte[]));
                var contentType = new ContentType
                {
                    MediaType = MediaTypeNames.Image.Jpeg,
                    Name = "QR Code"
                };

                if (Email.Send("invitation@simpleq.at", req.Emails.ToArray(), "SimpleQ Invitation",
                    $"REGISTRATION CODE: {CustCode}{req.DepId}{Environment.NewLine}{req.InvitationText}",
                    new Attachment(new MemoryStream(b), contentType)))
                {
                    return Index();
                }
                else
                {
                    var model = new ErrorModel { Title = "Unable to send e-mail", Message = "Sending failed due to internal error(s)." };
                    return View("Error", model);
                }
            }
        }
        #endregion

        #region AJAX-Methods
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
        #endregion

        #region Helpers
        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }

        private string CustCode => HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
        #endregion
    }
}