using Microsoft.Owin.Security;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class AccountController : Controller
    {
        #region MVC-Actions
        [HttpGet]
        public ActionResult Login()
        {
            return View("Login");
        }

        [HttpGet]
        public ActionResult Register()
        {
            using (var db = new SimpleQDBEntities())
            {
                ViewBag.PaymentMethods = db.PaymentMethods.ToList();
                return View("Register");
            }
        }

        [HttpPost]
        public ActionResult Register(Customer c)
        {
            bool err = false;

            if (string.IsNullOrEmpty(c.CustName))
                AddModelError("CustName", "Name must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.CustEmail))
                AddModelError("CustEmail", "Email must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.CustPwdTmp))
                AddModelError("CustPwdTmp", "Password must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.Street))
                AddModelError("Street", "Street must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.Plz))
                AddModelError("Plz", "ZIP code must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.City))
                AddModelError("City", "City must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.Country))
                AddModelError("Country", "Country must not be empty.", ref err);

            if (string.IsNullOrEmpty(c.LanguageCode))
                AddModelError("LanguageCode", "Language code must not be empty.", ref err);

            if (c.DataStoragePeriod <= 0)
                AddModelError("DataStoragePeriod", "Data storage period must be at least 1 month.", ref err);

            if (!new[] { 1, 3, 6, 12 }.Contains(c.AccountingPeriod))
                AddModelError("AccountingPeriod", "Accounting period must either 1, 3, 6 or 12 months", ref err);

            using (var db = new SimpleQDBEntities())
            {
                if (!db.PaymentMethods.Any(p => p.PaymentMethodId == c.PaymentMethodId))
                    AddModelError("PaymentMethodId", "Payment method does not exist.", ref err);

                if (err) return View("Login");


                var custCode = db.sp_GenerateCustCode().First();
                var minGroupSize = db.DataConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").First().ConstrValue;

                c.CustCode = custCode;
                c.MinGroupSize = minGroupSize;
                c.CostBalance = 0m;
                c.EmailConfirmed = false;

                db.Customers.Add(c);
                db.SaveChanges();

                Session["custCode"] = c.CustCode;
                Session["email"] = c.CustEmail;
                return RedirectToAction("SendEmailConfirmation");
            }
        }

        [HttpPost]
        public ActionResult Login(string custCode, string password, bool remember)
        {
            bool err = false;
            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == custCode))
                    AddModelError("custCode", "Invalid customer code.", ref err);
                else if (!db.Customers.Any(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                    AddModelError("password", "Invalid password.", ref err);
                else if (!db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).First().EmailConfirmed)
                    AddModelError("EmailConfirmation", "Please confirm your email before logging in.", ref err);

                if (err) return View("Login");

                SignIn(db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefault(), remember);
                return RedirectToAction("Index", "SurveyCreation");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            SignOut();
            return View("Login");
        }

        [HttpGet]
        public ActionResult SendEmailConfirmation() // Add timer!
        {
            var custCode = Session["custCode"] as string;
            var email = Session["email"] as string;
            try
            {
                MailMessage msg = new MailMessage("registration@simpleq.at", email)
                {
                    Subject = "E-Mail Confirmation",
                    Body = $"You registered successfully! Now please confirm your e-mail address.{Environment.NewLine}" +
                        $"Your customer code: {custCode}{Environment.NewLine}" +
                        $"Confirmation link: {Url.Action("ConfirmEmail", "Account", new { custCode, email}, Request.Url.Scheme)}{Environment.NewLine}" +
                        $"{Environment.NewLine}{Environment.NewLine}" +
                        $"Best regards{Environment.NewLine}" +
                        $"Your SimpleQ-Team"
                };
                SmtpClient client = new SmtpClient("smtp.1und1.de", 587)
                {
                    Credentials = new System.Net.NetworkCredential("registration@simpleq.at", "123SimpleQ..."),
                    EnableSsl = true
                };
                client.Send(msg);

                ViewBag.email = email;
                return View("Confirmation");
            }
            catch (SmtpException)
            {
                return Http.ServiceUnavailable("Sending failed due to internal error(s).");
            }
        }

        [HttpGet]
        public ActionResult ConfirmEmail(string custCode, string email)
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.CustCode == custCode).FirstOrDefault();
                if (cust == null)
                    return View("InvalidConfirmation");
                else if (cust.CustEmail != email)
                    return View("InvalidConfirmation");
                else
                {
                    cust.EmailConfirmed = true;
                    db.SaveChanges();
                    return RedirectToAction("Login");
                }
            }
        }
        #endregion

        #region Helpers
        private void SignIn(Customer cust, bool remember)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, cust.CustCode),
                new Claim(ClaimTypes.Name, cust.CustName),
                new Claim(ClaimTypes.Email, cust.CustEmail)
            };

            var identity = new ClaimsIdentity(claims, "ApplicationCookie");

            AuthManager.SignIn(new AuthenticationProperties
            {
                AllowRefresh = true,
                IsPersistent = remember
            }, identity);
        }

        private void SignOut()
        {
            AuthManager.SignOut("ApplicationCookie");
        }

        private void AddModelError(string key, string errorMessage, ref bool error)
        {
            ModelState.AddModelError(key, errorMessage);
            error = true;
        }

        private IAuthenticationManager AuthManager => Request.GetOwinContext().Authentication;
        #endregion
    }
}