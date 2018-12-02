using Microsoft.Owin.Security;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
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
        public ActionResult Login(int? confirmed)
        {
            ViewBag.confirmed = confirmed == 1;
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
        [ValidateAntiForgeryToken]
        public ActionResult Register(Customer cust, string confirmPassword)
        {
            bool err = false;

            if (cust == null)
                AddModelError("Model", "Model object must not be null", ref err);

            if (string.IsNullOrEmpty(cust.CustName))
                AddModelError("CustName", "Name must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.CustEmail))
                AddModelError("CustEmail", "Email must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.CustPwdTmp) || cust.CustPwdTmp.Count() < 8)
                AddModelError("CustPwdTmp", "Password must be at least 8 characters long.", ref err);

            if (cust.CustPwdTmp != confirmPassword)
                AddModelError("confirmPassword", "Entered passwords do not match.", ref err);

            if (string.IsNullOrEmpty(cust.Street))
                AddModelError("Street", "Street must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.Plz))
                AddModelError("Plz", "ZIP code must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.City))
                AddModelError("City", "City must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.Country))
                AddModelError("Country", "Country must not be empty.", ref err);

            if (string.IsNullOrEmpty(cust.LanguageCode))
                AddModelError("LanguageCode", "Language code must not be empty.", ref err);

            if (cust.DataStoragePeriod <= 0)
                AddModelError("DataStoragePeriod", "Data storage period must be at least 1 month.", ref err);

            if (!new[] { 1, 3, 6, 12 }.Contains(cust.AccountingPeriod))
                AddModelError("AccountingPeriod", "Accounting period must either 1, 3, 6 or 12 months", ref err);

            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Any(c => c.CustEmail == cust.CustEmail))
                    AddModelError("CustEmail", "Email does already exist.", ref err);

                if (!db.PaymentMethods.Any(p => p.PaymentMethodId == cust.PaymentMethodId))
                    AddModelError("PaymentMethodId", "Payment method does not exist.", ref err);

                if (err) return Register();


                var custCode = db.sp_GenerateCustCode().First();
                var minGroupSize = db.DataConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").First().ConstrValue;

                cust.CustCode = custCode;
                cust.MinGroupSize = minGroupSize;
                cust.CostBalance = 0m;
                cust.EmailConfirmed = false;

                db.Customers.Add(cust);
                db.SaveChanges();

                var authToken = db.sp_GenerateAuthToken(cust.CustCode).First();

                var body = $"You registered successfully! Now please confirm your e-mail address.{Environment.NewLine}" +
                    $"Your customer code: {cust.CustCode}{Environment.NewLine}" +
                    $"Confirmation link: {Url.Action("ConfirmEmail", "Account", new { authToken }, Request.Url.Scheme)}{Environment.NewLine}" +
                    $"{Environment.NewLine}{Environment.NewLine}" +
                    $"Best regards{Environment.NewLine}" +
                    $"Your SimpleQ-Team";

                if (Email.Send("registration@simpleq.at", cust.CustEmail, "E-mail confirmation", body))
                {
                    ViewBag.custCode = cust.CustCode;
                    ViewBag.email = cust.CustEmail;
                    return View("Confirmation");
                }
                else
                {
                    var model = new ErrorModel { Title = "Unable to send confirmation e-mail", Message = "Sending failed due to internal error(s)." };
                    return View("AccountError", model);
                }
            }
        }

        [HttpGet]
        public ActionResult ConfirmEmail(string authToken)
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefault();
                if (cust == null)
                {
                    var model = new ErrorModel { Title = "Invalid confirmation link!", Message = "This e-mail confirmation link is not valid." };
                    return View("AccountError", model);
                }

                cust.EmailConfirmed = true;
                db.SaveChanges();
                return RedirectToAction("Login", new { confirmed = 1 });

            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string custCode, string password, bool remember)
        {
            bool err = false;
            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == custCode))
                    AddModelError("custCode", "Invalid customer code.", ref err);
                else if (!db.Customers.Any(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                    AddModelError("password", "Invalid password.", ref err);

                if (err) return View("Login");

                SignIn(db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefault(), remember);
                return RedirectToAction("Index", "SurveyCreation");
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            SignOut();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult ResetPassword(string authToken)
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefault();
                if (cust != null)
                {
                    if (DateTime.Now - cust.LastTokenGenerated > TimeSpan.FromMinutes(15))
                    {
                        var model = new ErrorModel { Title = "Expired reset link!", Message = "This password reset link has already expired." };
                        return View("AccountError", model);
                    }
                    else
                    {
                        ViewBag.authToken = authToken;
                        return View("ResetPassword");
                    }
                }
                else
                {
                    var model = new ErrorModel { Title = "Invalid reset link!", Message = "This password reset link is not valid." };
                    return View("AccountError", model);
                }
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(string custCode)
        {
            using (var db = new SimpleQDBEntities())
            {
                if (db.Customers.Any(c => c.CustCode == custCode))
                {
                    var cust = db.Customers.Where(c => c.CustCode == custCode).FirstOrDefault();
                    var authToken = db.sp_GenerateAuthToken(cust.CustCode).First();
                    var email = cust.CustEmail;

                    var body = $"To reset your email click the following link: {Url.Action("ResetPassword", "Account", new { authToken }, Request.Url.Scheme)}{Environment.NewLine}" +
                        $"The link is valid for 15 minutes.{Environment.NewLine}" +
                        $"{Environment.NewLine}{Environment.NewLine}" +
                        $"Best regards{Environment.NewLine}{Environment.NewLine}" +
                        $"Your SimpleQ-Team";

                    if (Email.Send("registration@simpleq.at", email, "Password-Reset", body))
                        return Http.Ok();
                    else
                        return Http.ServiceUnavailable("Sending failed due to internal error(s).");
                }
                else
                {
                    return Http.NotFound("Customer not found.");
                }
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(string authToken, string password)
        {
            using (var db = new SimpleQDBEntities())
            {
                var cust = db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefault();
                if (cust != null)
                {
                    cust.CustPwdTmp = password;
                    db.SaveChanges();

                    return Http.Ok();
                }
                else
                {
                    return Http.Conflict("Invalid reset link");
                }
            }
        }

        [HttpDelete]
        [CAuthorize]
        public ActionResult Unregister(string custCode, string password)
        {
            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == custCode))
                    return Http.NotFound("Customer not found");
                else if (!db.Customers.Any(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                    return Http.Conflict("Invalid password.");

                var cust = db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefault();

                db.Surveys.Where(s => s.CustCode == custCode).ToList().ForEach(s => db.sp_DeleteSurvey(s.SvyId));
                cust.SurveyCategories.Clear();
                cust.AnswerTypes.Clear();
                cust.Bills.Clear();
                db.People.RemoveRange(cust.Departments.SelectMany(d => d.People));
                cust.Departments.Clear();

                db.SaveChanges();

                return Http.Ok();
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