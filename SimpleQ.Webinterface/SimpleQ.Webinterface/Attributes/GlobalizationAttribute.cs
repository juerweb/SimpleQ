using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Attributes
{
    public class GlobalizationAttribute : ActionFilterAttribute
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                string language = (string)filterContext.RouteData.Values["language"] ?? "de";
                language = (!new[] { "de", "en" }.Contains(language)) ? "de" : language;
                language = (language == "en") ? "en-GB" : language;

                logger.Debug($"Language: {language}");

                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(language);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Error setting language");
            }
        }
    }
}