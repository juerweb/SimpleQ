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
using NLog;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class GroupAdministrationController : Controller
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            try
            {
                logger.Debug($"Loading group administration: {CustCode}");
                using (var db = new SimpleQDBEntities())
                {
                    if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    var model = new GroupAdministrationModel
                    {
                        Departments = db.Departments.Where(d => d.CustCode == CustCode).ToDictionary(d => d, d => d.People.Count())
                    };

                    ViewBag.emailConfirmed = db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault().EmailConfirmed;
                    logger.Debug("Group administration loaded successfully");
                    return View("GroupAdministration", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public ActionResult SendInvitations(GroupAdministrationModel req)
        {
            try
            {
                logger.Debug($"Requested to send invitations: {CustCode}");
                bool err = false;
                req = new GroupAdministrationModel
                {
                    DepId = 1,
                    Emails = new List<string> { "dev@simpleq.at", "lukas.schendlinger@a1.net" },
                    InvitationText = "sads ma BITTE NED BES"
                };

                if (req == null)
                    AddModelError("Model", "Model object must not be null.", ref err);

                if (req.Emails == null || req.Emails.Count == 0)
                    AddModelError("Emails", "Email addreses must not be empty.", ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Sending invitation e-mails failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = "Customer not found", Message = "The current customer was not found." });
                    }

                    if (db.Departments.Where(d => d.DepId == req.DepId && d.CustCode == CustCode).Count() == 0)
                        AddModelError("DepId", "Department not found.", ref err);

                    if (err)
                    {
                        logger.Debug("SendInvitations validation failed. Exiting action");
                        return Index();
                    }


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

                    if (Email.Send("invitation@simpleq.at", req.Emails.ToArray(), req.InvitationSubject,
                        $"REGISTRATION CODE: {CustCode}{req.DepId}{Environment.NewLine}{req.InvitationText}",
                        true,
                        new Attachment(new MemoryStream(b), contentType)))
                    {
                        logger.Debug($"Invitation e-mails sent successfully for: {CustCode}");
                        return Index();
                    }
                    else
                    {
                        logger.Error($"Failed to send invitation e-mails for: {CustCode}");
                        var model = new ErrorModel { Title = "Unable to send e-mail", Message = "Sending failed due to internal error(s)." };
                        return View("Error", model);
                    }
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = "Error", Message = "Something went wrong. Please try again later." };
                logger.Error(ex, "[POST]SendInvitations: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public ActionResult Create(string depName)
        {
            try
            {
                logger.Debug($"Group create requested: {CustCode} (DepName: {depName})");
                if (string.IsNullOrEmpty(depName))
                {
                    logger.Debug("Group creation failed. Name was null or empty.");
                    return Http.BadRequest("Department name must not be empty.");
                }

                using (var db = new SimpleQDBEntities())
                {
                    if (db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefault() == null)
                    {
                        logger.Warn($"Group creation failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var query = db.Departments.Where(d => d.CustCode == CustCode);

                    var dep = new Department
                    {
                        DepId = (query.Count() == 0) ? 1 : query.Max(d => d.DepId) + 1,
                        DepName = depName,
                        CustCode = CustCode
                    };
                    db.Departments.Add(dep);
                    db.SaveChanges();
                    logger.Debug("Group created successfully");

                    return Json(new { dep.DepId, RegCode = $"{CustCode}{dep.DepId}" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]Create: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult Modify(int depId, string depName)
        {
            try
            {
                logger.Debug($"Group modify requested: {CustCode} (DepId: {depId}, DepName: {depName})");
                if (string.IsNullOrEmpty(depName))
                {
                    logger.Debug("Group modification failed. Name was null or empty.");
                    return Http.BadRequest("Department name must not be empty.");
                }

                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                        logger.Warn($"Group modification failed. Customer not found: {CustCode}");

                    var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                    if (dep == null)
                    {
                        logger.Debug("Group modification failed. Department not found");
                        return Http.NotFound("Department not found.");
                    }

                    dep.DepName = depName;
                    db.SaveChanges();
                    logger.Debug("Group modified successfully");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]Modify: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpGet]
        public ActionResult Delete(int depId)
        {
            try
            {
                logger.Debug($"Group delete requested: {CustCode} (DepId: {depId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!db.Customers.Any(c => c.CustCode == CustCode))
                        logger.Warn($"Group deleting failed. Customer not found: {CustCode}");

                    var dep = db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefault();
                    if (dep == null)
                    {
                        logger.Debug("Group deleting failed. Department not found.");
                        return Http.NotFound("Department not found.");
                    }

                    db.Departments.Remove(dep);
                    db.SaveChanges();
                    logger.Debug("Group deleted successfully");

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[GET]Delete: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }
        #endregion

        #region Helpers
        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }

        private string CustCode
        {
            get
            {
                try
                {
                    return HttpContext.GetOwinContext().Authentication.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault().Value;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "CustCode: Unexpected error");
                    throw ex;
                }
            }
        }
        #endregion
    }
}