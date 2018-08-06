using FreshMvvm;
using SimpleQ.Validations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace SimpleQ.PageModels.Commands
{
    public class ScanQRCodeCommand : FreshBasePageModel, ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Debug.WriteLine("Execute ScanQRCodeCommand", "Info");

            String code = parameter.ToString();
            //pageModel = (RegisterPageModel)parameter;

            Debug.WriteLine("QR Code found. Code is " + code, "Info");

            //Live-Check
            if (SixDigitCodeValidation.IsValid(code))
            {
                CoreMethods.PushPageModel<QRCodeReaderPageModel>();
                //pageModel.Model.RegisterCode = int.Parse(code);
                //pageModel.CheckingCode();
                Debug.WriteLine("Live-Check: QR-Code is valid.", "Info");
            }
            else
            {
                //pageModel.DialogService.ShowDialog(Services.DialogType.Error, 101);
            }
        }
    }
}
