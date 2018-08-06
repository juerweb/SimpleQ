using Acr.UserDialogs;
using System;
using System.Collections.Generic;
using System.Text;

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
                    this.dialogs.Alert(App.Current.Resources[textCode].ToString(), App.Current.Resources["DialogErrorTitle"] + "\n" + App.Current.Resources["DialogErrorCode"] + ": " + errorCode);
                    break;
            }
        }
    }

    public enum DialogType
    {
        Error = 0
    }
}
