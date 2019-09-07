using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity;
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
using System.Threading.Tasks;

namespace SimpleQ.Webinterface.Controllers
{
    [CAuthorize]
    public class GroupAdministrationController : BaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            try
            {
                logger.Debug($"Loading group administration: {CustCode}");
                using (var db = new SimpleQDBEntities())
                {
                    if (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync() == null)
                    {
                        logger.Warn($"Loading failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    var model = new GroupAdministrationModel
                    {
                        Departments = await db.Departments.Where(d => d.CustCode == CustCode).ToDictionaryAsync(d => d, d => d.People.Count())
                    };

                    ViewBag.emailConfirmed = (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync()).EmailConfirmed;
                    logger.Debug("Group administration loaded successfully");
                    return View("GroupAdministration", model);
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Index: Unexpected error");
                return View("Error", model);
            }
        }

        [HttpPost]
        public async Task<ActionResult> SendInvitations(GroupAdministrationModel req)
        {
            try
            {
                //req = new GroupAdministrationModel
                //{
                //    DepId = 1,
                //    Emails = new List<string> { "dev@simpleq.at", "lukas.schendlinger@a1.net" },
                //    InvitationText = "sads ma BITTE NED BES"
                //};

                logger.Debug($"Requested to send invitations: {CustCode}");
                bool err = false;

                if (req == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);

                if (req.Emails == null || req.Emails.Count == 0)
                    AddModelError("Emails", BackendResources.EmailAdressesEmpty, ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                    {
                        logger.Warn($"Sending invitation e-mails failed. Customer not found: {CustCode}");
                        return View("Error", new ErrorModel { Title = BackendResources.CustomerNotFoundTitle, Message = BackendResources.CustomerNotFoundMsg });
                    }

                    if (await db.Departments.Where(d => d.DepId == req.DepId && d.CustCode == CustCode).CountAsync() == 0)
                        AddModelError("DepId", BackendResources.DepNotFound, ref err);

                    if (err)
                    {
                        logger.Debug("SendInvitations validation failed. Exiting action");
                        return await Index();
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

                    string subject = "";
                    string body = "";

                    if (string.IsNullOrWhiteSpace(req.InvitationSubject))
                        subject = BackendResources.GroupInvitationEmailSubject;
                    else
                        subject = req.InvitationSubject;

                    if (string.IsNullOrWhiteSpace(req.InvitationText))
                        body = BackendResources.GroupInvitationEmailMessage
                            .Replace("{RegCode}", $"{CustCode}{req.DepId}");
                    else
                        body = BackendResources.GroupInvitationEmailMessageCustom
                            .Replace("{RegCode}", $"{CustCode}{req.DepId}")
                            .Replace("{CustomMessage}", req.InvitationText);

                    if (await Email.Send("invitation@simpleq.at", req.Emails.ToArray(), subject, body, true, new Attachment(new MemoryStream(b), contentType)))
                    {
                        logger.Debug($"Invitation e-mails sent successfully for: {CustCode}");
                        return RedirectToAction("Index", "GroupAdministration");
                    }
                    else
                    {
                        logger.Error($"Failed to send invitation e-mails for: {CustCode}");
                        var model = new ErrorModel { Title = BackendResources.GroupInvitationSendingFailedTitle, Message = BackendResources.GroupInvitationSendingFailedMsg };
                        return View("Error", model);
                    }
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]SendInvitations: Unexpected error");
                return View("Error", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpGet]
        public async Task<ActionResult> Create(string depName)
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
                    if (await db.Customers.Where(c => c.CustCode == CustCode).FirstOrDefaultAsync() == null)
                    {
                        logger.Warn($"Group creation failed. Customer not found: {CustCode}");
                        return Http.NotFound("Customer not found.");
                    }

                    var query = db.Departments.Where(d => d.CustCode == CustCode);

                    var dep = new Department
                    {
                        DepId = (await query.CountAsync() == 0) ? 1 : await query.MaxAsync(d => d.DepId) + 1,
                        DepName = depName,
                        CustCode = CustCode
                    };
                    db.Departments.Add(dep);
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> Modify(int depId, string depName)
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
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                        logger.Warn($"Group modification failed. Customer not found: {CustCode}");

                    var dep = await db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefaultAsync();
                    if (dep == null)
                    {
                        logger.Debug("Group modification failed. Department not found");
                        return Http.NotFound("Department not found.");
                    }

                    dep.DepName = depName;
                    await db.SaveChangesAsync();
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
        public async Task<ActionResult> Delete(int depId)
        {
            try
            {
                logger.Debug($"Group delete requested: {CustCode} (DepId: {depId})");
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == CustCode))
                        logger.Warn($"Group deleting failed. Customer not found: {CustCode}");

                    var dep = await db.Departments.Where(d => d.DepId == depId && d.CustCode == CustCode).FirstOrDefaultAsync();
                    if (dep == null)
                    {
                        logger.Debug("Group deleting failed. Department not found");
                        return Http.NotFound("Department not found.");
                    }

                    if (dep.People.Count() != 0)
                    {
                        logger.Debug("Group deleting failed. Has to be empty");
                        return Http.Conflict("Department has to be empty.");
                    }

                    if (dep.Surveys.Any(s => s.EndDate >= DateTime.Now))
                    {
                        logger.Debug("Group deleting failed. There are still running surveys");
                        return Http.Conflict("There are still running surveys.");
                    }


                    foreach (var s in dep.Surveys.ToList())
                    {
                        dep.Surveys.Remove(s);
                    }

                    db.Departments.Remove(dep);
                    await db.SaveChangesAsync();
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
    }
}