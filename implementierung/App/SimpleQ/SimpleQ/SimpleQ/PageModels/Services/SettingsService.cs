using Akavache;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Xamarin.Forms;
using System.Diagnostics;
using SimpleQ.Logging;

namespace SimpleQ.PageModels.Services
{
    public class SettingsService : ISettingsService
    {
        public async Task<T> GetSetting<T>(string name)
        {
            try
            {
                //Debug.WriteLine("KeyFound...");
                T test = await BlobCache.UserAccount.GetObject<T>(name);
                //Debug.WriteLine("KeyFound...");
                return test;
            }
            catch (KeyNotFoundException ex)
            {
                Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
                logger.Error("Settings Key with the name " + name + " not found.");
                return default(T);
            }

        }

        public bool SetSetting<T>(string name, T value)
        {
            try
            {
                BlobCache.UserAccount.InsertObject<T>(name, value);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
