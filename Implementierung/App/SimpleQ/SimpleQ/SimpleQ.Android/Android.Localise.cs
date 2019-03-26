using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using Java.Util;
using SimpleQ.Droid;
using SimpleQ.Extensions;
using Xamarin.Forms;

[assembly: Dependency(typeof(Localize))]

namespace SimpleQ.Droid
{
    public class Localize : ILocalize
    {
        #region Public Methods

        public CultureInfo GetCurrentCultureInfo()
        {
            var androidLocale = Locale.Default;

            var language = androidLocale.ToString().Replace("_", "-");

            var cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            if (!cultures.Any(c => c.Name == language))
            {
                language = "en";
            }

            return new CultureInfo(language);
        }

        public void SetLocale()
        {
            var culture = GetCurrentCultureInfo();

            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        #endregion
    }
}