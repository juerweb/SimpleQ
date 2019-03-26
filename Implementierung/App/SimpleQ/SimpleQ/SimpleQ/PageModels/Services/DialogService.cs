using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Logging;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SimpleQ.PageModels.Services
{
    public class DialogService: IDialogService
    {
        private IUserDialogs dialogService;

        public DialogService()
        {
            this.dialogService = FreshIOC.Container.Resolve<IUserDialogs>();
        }

        public void ShowErrorDialog(int errorCode)
        {
            string textCode = "Error" + errorCode;

            this.dialogService.Alert(AppResources.ResourceManager.GetString(textCode) + "\n" + AppResources.DialogErrorCode + ": " + errorCode, AppResources.DialogErrorTitle);
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Error("Error has occured with the code " + errorCode);

            //Debug.WriteLine("Error-Code: " + errorCode, "Error");
        }

        public void ShowDialog(String title, String body)
        {
            this.dialogService.Alert(body, title);
        }

        public async Task<bool> ShowReallySureDialog()
        {
            ConfirmConfig cc = new ConfirmConfig();
            cc = cc.UseYesNo();
            cc.Message = AppResources.ReallySure;


            var result = await this.dialogService.ConfirmAsync(cc);

            return result;
        }
    }
}
