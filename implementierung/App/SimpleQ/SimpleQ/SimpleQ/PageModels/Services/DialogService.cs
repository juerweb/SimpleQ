using Acr.UserDialogs;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels.Services
{
    public class DialogService
    {
        private IUserDialogs dialogs;

        public DialogService(IUserDialogs dialogs)
        {
            this.dialogs = dialogs;
        }

        public void ShowDialog(DialogType type, int errorCode)
        {
            switch (type)
            {
                case DialogType.Error:
                    string textCode = "Error" + errorCode;

                    this.dialogs.Alert(AppResources.ResourceManager.GetString(textCode) + "\n" + AppResources.DialogErrorCode + ": " + errorCode, AppResources.DialogErrorTitle);
                    Debug.WriteLine("Error-Code: " + errorCode, "Error");
                    break;
            }
        }
    }

    public enum DialogType
    {
        Error = 0
    }
}
