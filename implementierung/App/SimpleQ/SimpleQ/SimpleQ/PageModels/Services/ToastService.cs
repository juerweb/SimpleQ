using SimpleQ.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels.Services
{
    public class ToastService : IToastService
    {
        public void ShortMessage(string message)
        {
            DependencyService.Get<IMessage>().ShortAlert(message);
        }

        public void LongMessage(string message)
        {
            DependencyService.Get<IMessage>().LongAlert(message);
        }
    }
}
