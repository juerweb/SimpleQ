using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
            Debug.WriteLine("Error-Code: " + errorCode, "Error");
        }

        public void ShowDialog(String title, String body)
        {
            this.dialogService.Alert(body, title);
        }
    }
}
