using Microsoft.Owin.Security;
using NLog;
using SimpleQ.Webinterface.Attributes;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;
using SimpleQ.Webinterface.Properties;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    [Globalization]
    public class AccountController : BaseController
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region MVC-Actions
        [HttpGet]
        public ActionResult Index()
        {
            if (AuthManager.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault() != null)
                return RedirectToAction("Index", "SurveyCreation");
            else
                return RedirectToAction("Login");
        }

        [HttpGet]
        public ActionResult Login(int? confirmed)
        {
            try
            {
                ViewBag.confirmed = confirmed == 1;
                logger.Trace($"Loading login page {(confirmed == 1 ? " after confirmation" : "")}");
                Response.AppendHeader("spn", "sam, dean, castiel and jack were here");
                return View("Login");
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Login: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpGet]
        public ActionResult Register()
        {
            try
            {
                using (var db = new SimpleQDBEntities())
                {
                    ViewBag.PaymentMethods = db.PaymentMethods.ToList();
                    logger.Trace("Loading registration page.");
                    Response.AppendHeader("get-stoned", "it's always 4:20 somewhere");
                    return View("Register");
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Register: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(Customer cust, string confirmPassword, int day)
        {
            try
            {
                logger.Debug("Registration requested");
                bool err = false;

                if (cust == null)
                    AddModelError("Model", BackendResources.ModelNull, ref err);

                if (string.IsNullOrEmpty(cust.CustName))
                    AddModelError("CustName", BackendResources.NameEmpty, ref err);

                if (string.IsNullOrEmpty(cust.CustEmail))
                    AddModelError("CustEmail", BackendResources.EmailEmpty, ref err);

                if (string.IsNullOrEmpty(cust.CustPwdTmp) || cust.CustPwdTmp.Count() < 8)
                    AddModelError("CustPwdTmp", BackendResources.PwdTooShort, ref err);

                if (cust.CustPwdTmp != confirmPassword)
                    AddModelError("confirmPassword", BackendResources.PwdDontMatch, ref err);

                if (string.IsNullOrEmpty(cust.Street))
                    AddModelError("Street", BackendResources.StreetEmpty, ref err);

                if (string.IsNullOrEmpty(cust.Plz))
                    AddModelError("Plz", BackendResources.PlzEmpty, ref err);

                if (string.IsNullOrEmpty(cust.City))
                    AddModelError("City", BackendResources.CityEmpty, ref err);

                if (string.IsNullOrEmpty(cust.Country))
                    AddModelError("Country", BackendResources.CountryEmpty, ref err);

                if (string.IsNullOrEmpty(cust.LanguageCode))
                    AddModelError("LanguageCode", BackendResources.LangCodeEmpty, ref err);

                if (cust.DataStoragePeriod <= 0)
                    AddModelError("DataStoragePeriod", BackendResources.DataStoragePeriodInvalid, ref err);

                if (!new[] { 1, 3, 6, 12 }.Contains(cust.AccountingPeriod))
                    AddModelError("AccountingPeriod", BackendResources.AccountingPeriodInvalid, ref err);

                if (day < 1 || day > 31)
                    AddModelError("day", BackendResources.DayInvalid, ref err);

                using (var db = new SimpleQDBEntities())
                {
                    if (await db.Customers.AnyAsync(c => c.CustEmail == cust.CustEmail))
                        AddModelError("CustEmail", BackendResources.EmailAlreadyExists, ref err);

                    if (!await db.PaymentMethods.AnyAsync(p => p.PaymentMethodId == cust.PaymentMethodId))
                        AddModelError("PaymentMethodId", BackendResources.PaymentMethodInvalid, ref err);


                    if (err)
                    {
                        logger.Debug("Register validation failed. Exiting action");
                        return Register();
                    }
                    logger.Debug("Register validation successful");

                    var custCode = await Task.Run(() => db.sp_GenerateCustCode().First());
                    var minGroupSize = db.DataConstraints.Where(d => d.ConstrName == "MIN_GROUP_SIZE").First().ConstrValue;

                    cust.CustCode = custCode;
                    cust.MinGroupSize = minGroupSize;
                    cust.CostBalance = 0m;
                    cust.EmailConfirmed = false;

                    var date = DateTime.Now;
                    day = (DateTime.DaysInMonth(date.Year, date.Month) < day) ? DateTime.DaysInMonth(date.Year, date.Month) : day;
                    var old = cust.AccountingDate;
                    cust.AccountingDate = new DateTime(old.Year, old.Month, day);
                    logger.Debug($"AccountingDate for {custCode}: {cust.AccountingDate}");

                    db.Customers.Add(cust);
                    await db.SaveChangesAsync();

                    async Task<int> maxCatId()
                    {
                        await db.SaveChangesAsync();
                        return await db.SurveyCategories
                            .Where(c => c.CustCode == custCode)
                            .Select(c => c.CatId)
                            .DefaultIfEmpty(0)
                            .MaxAsync();
                    }

                    async Task<int> maxDepId()
                    {
                        await db.SaveChangesAsync();
                        return await db.Departments
                            .Where(d => d.CustCode == custCode)
                            .Select(d => d.DepId)
                            .DefaultIfEmpty(0)
                            .MaxAsync();
                    }

                    db.SurveyCategories.Add(new SurveyCategory { CatId = await maxCatId() + 1, CustCode = custCode, CatName = "Employee satisfaction", Deactivated = false });
                    db.SurveyCategories.Add(new SurveyCategory { CatId = await maxCatId() + 1, CustCode = custCode, CatName = "Workplace design", Deactivated = false });
                    db.Departments.Add(new Department { DepId = await maxDepId() + 1, CustCode = custCode, DepName = "Everyone" });
                    await db.SaveChangesAsync();

                    logger.Info($"Customer registered: {custCode}");
                    Response.AppendHeader("msg", "so close, no matter how far");

                    var authToken = await Task.Run(() => db.sp_GenerateAuthToken(cust.CustCode).First());

                    var body = BackendResources.RegistrationEmailMessage
                        .Replace("{CustCode}", cust.CustCode)
                        .Replace("{Url}", Url.Action("ConfirmEmail", "Account", new { authToken }, Request.Url.Scheme));

                    if (await Email.Send("registration@simpleq.at", cust.CustEmail, BackendResources.RegistrationEmailSubject, body))
                    {
                        ViewBag.custCode = cust.CustCode;
                        ViewBag.email = cust.CustEmail;
                        logger.Debug($"Confirmation e-mail sent successfully {custCode}");
                        return View("Confirmation");
                    }
                    else
                    {
                        var model = new ErrorModel { Title = BackendResources.ConfirmationSendingFailedTitle, Message = BackendResources.ConfirmationSendingFailedMsg };
                        logger.Error($"Failed to send confirmation e-mail to {custCode}");
                        return View("AccountError", model);
                    }
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]Register: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ConfirmEmail(string authToken)
        {
            try
            {
                logger.Debug("Loading e-mail confirmation");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefaultAsync();
                    if (cust == null)
                    {
                        logger.Debug($"Invalid e-mail confirmation token: {authToken}");
                        var model = new ErrorModel { Title = BackendResources.InvalidConfirmationLinkMsg, Message = BackendResources.InvalidConfirmationLinkTitle };
                        return View("AccountError", model);
                    }

                    cust.EmailConfirmed = true;
                    await db.SaveChangesAsync();
                    logger.Debug($"E-mail confirmed successfully for: {cust.CustCode}");
                    return RedirectToAction("Login", new { confirmed = 1 });
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]ConfirmEmail: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string custCode, string password, bool remember)
        {
            try
            {
                logger.Debug("Login requested");
                bool err = false;
                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                    {
                        AddModelError("password", BackendResources.CustCodeOrPwdInvalid, ref err);
                        logger.Debug($"Login failed. Invalid customer code or password for {custCode}: {password}");
                    }

                    if (err) return View("Login");
                    logger.Debug("Login validation sucessful");

                    SignIn(await db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefaultAsync(), remember);
                    logger.Debug("Logged in successfully");
                    return RedirectToAction("Index", "SurveyCreation");
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[POST]Login: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            try
            {
                logger.Debug("Logout requested");
                SignOut();
                logger.Debug("Logged out successfully");
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]Logout: Unexpected error");
                return View("AccountError", model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> ResetPassword(string authToken)
        {
            try
            {
                logger.Debug("Loading password reset page");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefaultAsync();
                    if (cust != null)
                    {
                        if (DateTime.Now - cust.LastTokenGenerated > TimeSpan.FromMinutes(15))
                        {
                            logger.Debug($"Loading password reset page failed. Expired password reset token: {authToken}");
                            var model = new ErrorModel { Title = BackendResources.ExpiredPasswordResetTitle , Message = BackendResources.ExpiredPasswordResetMsg };
                            return View("AccountError", model);
                        }
                        else
                        {
                            ViewBag.authToken = authToken;
                            logger.Debug($"Valid password reset token. Returning password reset page");
                            return View("ResetPassword");
                        }
                    }
                    else
                    {
                        logger.Debug($"Loading password reset page failed. Invalid password reset token: {authToken}");
                        var model = new ErrorModel { Title = BackendResources.InvalidPasswordResetTitle, Message = BackendResources.InvalidPasswordResetMsg };
                        return View("AccountError", model);
                    }
                }
            }
            catch (Exception ex)
            {
                var model = new ErrorModel { Title = BackendResources.Error, Message = BackendResources.DefaultErrorMsg };
                logger.Error(ex, "[GET]ResetPassword: Unexpected error");
                return View("AccountError", model);
            }
        }
        #endregion

        #region AJAX-Methods
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(string custCode)
        {
            try
            {
                logger.Debug("Requested password resetting email.");
                using (var db = new SimpleQDBEntities())
                {
                    if (await db.Customers.AnyAsync(c => c.CustCode == custCode))
                    {
                        var cust = await db.Customers.Where(c => c.CustCode == custCode).FirstOrDefaultAsync();
                        var authToken = await Task.Run(() => db.sp_GenerateAuthToken(cust.CustCode).First());
                        var email = cust.CustEmail;

                        var body = BackendResources.ForgotPasswordEmailMessage
                            .Replace("{Url}", Url.Action("ResetPassword", "Account", new { authToken }, Request.Url.Scheme));

                        if (await Email.Send("registration@simpleq.at", email, BackendResources.ForgotPasswordEmailSubject, body))
                        {
                            logger.Debug($"Password reset email sent successfully for: {custCode}");
                            return Http.Ok();
                        }
                        else
                        {
                            logger.Error($"Failed to send passsword reset e-mail to {custCode}");
                            return Http.ServiceUnavailable("Sending failed due to internal error(s).");
                        }
                    }
                    else
                    {
                        logger.Debug($"Invalid customer code: {custCode}");
                        return Http.NotFound("Customer not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[POST]ForgotPassword: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpPut]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(string authToken, string password)
        {
            try
            {
                logger.Debug("Changing password requested");
                using (var db = new SimpleQDBEntities())
                {
                    var cust = await db.Customers.Where(c => c.AuthToken == authToken).FirstOrDefaultAsync();
                    if (cust != null)
                    {
                        cust.CustPwdTmp = password;
                        await db.SaveChangesAsync();
                        logger.Debug("Password changed");

                        return Http.Ok();
                    }
                    else
                    {
                        logger.Debug($"Changing password failed. Invalid password reset token: {authToken}");
                        return Http.Forbidden("Invalid reset link");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[PUT]ChangePassword: Unexpected error");
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }

        [HttpDelete]
        [CAuthorize]
        public async Task<ActionResult> Unregister(string custCode, string password)
        {
            try
            {
                logger.Debug("Unregister requested.");
                if (custCode != AuthManager.User.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).FirstOrDefault()?.Value)
                {
                    logger.Debug($"Unregister failed. Invalid customer code {custCode}");
                    return Http.Forbidden("Invalid customer code");
                }

                using (var db = new SimpleQDBEntities())
                {
                    if (!await db.Customers.AnyAsync(c => c.CustCode == custCode))
                    {
                        logger.Debug($"Unregister failed. Invalid customer code {custCode}");
                        return Http.NotFound("Customer not found");
                    }
                    else if (!await db.Customers.AnyAsync(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                    {
                        logger.Debug($"Unregister failed. Invalid password for {custCode}: {password}");
                        return Http.Forbidden("Invalid password.");
                    }

                    var cust = await db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefaultAsync();
                    if (cust.CostBalance != 0)
                    {
                        logger.Debug($"Unregister failed. {custCode} has got bills to pay");
                        return Http.Conflict("There are still bills to pay");
                    }

                    var surveys = await db.Surveys.Where(s => s.CustCode == custCode).ToListAsync();
                    foreach (var s in surveys)
                    {
                        db.Votes.RemoveRange((await db.Votes.ToListAsync()).Where(v => s.AnswerOptions.Any(a => a.Votes.Any(vo => vo.VoteId == v.VoteId))));
                        db.AnswerOptions.RemoveRange(s.AnswerOptions);
                        s.Departments.Clear();
                        db.Surveys.Remove(s);
                    }
                    await db.SaveChangesAsync();

                    db.SurveyCategories.RemoveRange(db.SurveyCategories.Where(c => c.CustCode == custCode));
                    cust.AnswerTypes.Clear();
                    db.Bills.RemoveRange(db.Bills.Where(b => b.CustCode == custCode));
                    db.People.RemoveRange(cust.Departments.SelectMany(d => d.People));
                    db.Departments.RemoveRange(db.Departments.Where(d => d.CustCode == custCode));
                    db.Customers.Remove(cust);
                    await db.SaveChangesAsync();

                    Response.AppendHeader("msg", "cause nothin' lasts forever even cold november rain");
                    logger.Info($"Customer unregistered {custCode}");

                    SignOut();

                    return Http.Ok();
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex, "[DELETE]Unregister: Unexpected error"); ;
                return Http.InternalServerError("Something went wrong. Please try again later.");
            }
        }
        #endregion

        #region Helpers
        private void SignIn(Customer cust, bool remember)
        {
            try
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
            catch (Exception ex)
            {
                logger.Error(ex, "SignIn: Unexpected error");
                throw ex;
            }
        }

        private void SignOut()
        {
            try
            {
                Session.Abandon();
                AuthManager.SignOut("ApplicationCookie");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "SignOut: Unexpected error");
                throw ex;
            }
        }

        private IAuthenticationManager AuthManager
        {
            get
            {
                try
                {
                    return Request.GetOwinContext().Authentication;
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "AuthManager: Unexpected error");
                    throw ex;
                }
            }
        }

        protected override string CustCode => throw new NotSupportedException();
        #endregion
    }
}