using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Controllers
{
    public abstract class BaseController : Controller
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        #region Helpers
        protected virtual string CustCode
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

        protected virtual void AddModelError(string key, string errorMessage, ref bool error)
        {
            try
            {
                logger.Debug($"Model error: {key}: {errorMessage}");
                ModelState.AddModelError(key, errorMessage);
                error = true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "AddModelError: Unexpected error");
                throw ex;
            }
        }
        #endregion
    }
}