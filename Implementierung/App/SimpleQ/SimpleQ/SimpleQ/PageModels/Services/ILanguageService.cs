using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface ILanguageService
    {
        CultureInfo GetCurrentLanguage();

        void SetCurrentLanguage(string languageISOCode = null);

        List<CultureInfo> SupportedLanguages
        {
            get;
        }
    }
}
