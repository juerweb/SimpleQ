using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace SimpleQ.Extensions
{
    public interface ILocalize
    {
        CultureInfo GetCurrentCultureInfo();

        void SetLocale();
    }
}
