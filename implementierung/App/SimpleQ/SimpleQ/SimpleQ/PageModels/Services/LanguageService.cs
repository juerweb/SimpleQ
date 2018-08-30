using Plugin.Multilingual;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels.Services
{
    public class LanguageService: ILanguageService 
    {
        public LanguageService()
        {
            supportedLanguages.Add(new CultureInfo("de")); //<-- default Language
            supportedLanguages.Add(new CultureInfo("en"));
        }

        List<CultureInfo> supportedLanguages = new List<CultureInfo>();
        public List<CultureInfo> SupportedLanguages
        {
            get
            {
                return supportedLanguages;
            }
        }

        public CultureInfo GetCurrentLanguage()
        {
            if (!Application.Current.Properties.Keys.Contains("Language"))
            {
                return GetLanguageFromCode(CrossMultilingual.Current.DeviceCultureInfo.TwoLetterISOLanguageName);
            }
            else
            {
                return GetLanguageFromCode(Application.Current.Properties["Language"].ToString());
            }
        }

        public void SetCurrentLanguage(String languageISOCode = null)
        {
            if (languageISOCode == null)
            {
                CultureInfo current;
                if (!Application.Current.Properties.Keys.Contains("Language"))
                {
                    //Set Device Language to Default Language of App
                    current = CrossMultilingual.Current.DeviceCultureInfo;
                    if (!IsLanguageSupported(current.TwoLetterISOLanguageName))
                    {
                        current = SupportedLanguages[0];
                    }
                    else
                    {
                        current = GetLanguageFromCode(current.TwoLetterISOLanguageName);
                    }
                }
                else
                {
                    //Set Language from Properties to Default Language of App
                    current = GetLanguageFromCode(Application.Current.Properties["Language"].ToString());
                }

                CrossMultilingual.Current.CurrentCultureInfo = current;
                AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;
            }
            else
            {
                CultureInfo ci = GetLanguageFromCode(languageISOCode);
                Application.Current.Properties["Language"] = ci.TwoLetterISOLanguageName;
                CrossMultilingual.Current.CurrentCultureInfo = ci;
                AppResources.Culture = CrossMultilingual.Current.CurrentCultureInfo;

                App.NavigateToMainPageModel(true);
            }

        }

        public Boolean IsLanguageSupported(String isoLanguageCode)
        {
            foreach (CultureInfo ci in SupportedLanguages)
            {
                if (ci.TwoLetterISOLanguageName == isoLanguageCode)
                {
                    return true;
                }
            }

            return false;
        }

        public CultureInfo GetLanguageFromCode(String isoLanguageCode)
        {
            foreach (CultureInfo ci in SupportedLanguages)
            {
                if (ci.TwoLetterISOLanguageName == isoLanguageCode)
                {
                    return ci;
                }
            }

            return null;
        }
    }
}
