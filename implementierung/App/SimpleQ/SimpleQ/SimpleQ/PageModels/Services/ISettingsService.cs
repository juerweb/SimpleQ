using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface ISettingsService
    {
        Boolean SetSetting(String name, String value);
        String GetSetting(String name);
    }
}
