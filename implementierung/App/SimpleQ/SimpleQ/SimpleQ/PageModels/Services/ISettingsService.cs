using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface ISettingsService
    {
        bool SetSetting<T>(string name, T value);
        Task<T> GetSetting<T>(string name);
    }
}
