using Microsoft.Owin.Security;
using SimpleQ.Webinterface.Extensions;
using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public class AccountController : Controller
    {
        #region MVC-Methods
        [HttpGet]
        public ActionResult Index()
        {
            return View("Account");
        }

        [HttpPost]
        public ActionResult Login(string custCode, string password, bool remember)
        {
            bool err = false;
            using (var db = new SimpleQDBEntities())
            {
                if (!db.Customers.Any(c => c.CustCode == custCode))
                {
                    AddModelError("custCode", "Invalid customer code.", ref err);
                }
                else if (!db.Customers.Any(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)))
                {
                    AddModelError("password", "Invalid password.", ref err);
                }

                if (err)
                {
                    return Index();
                }
                else
                {
                    SignIn(db.Customers.Where(c => c.CustCode == custCode && c.CustPwdHash == db.fn_GetHash(password)).FirstOrDefault(), remember);
                    return RedirectToAction("Index", "SurveyCreation");
                }
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            SignOut();
            return View("Account");
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