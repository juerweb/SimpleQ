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
    public class ScanQRCodeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private RegisterPageModel pageModel;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Debug.WriteLine("Execute ScanQRCodeCommand", "Info");
            pageModel = (RegisterPageModel)parameter;

            //Start QR Code Reader
            ScanQRCodeAsync();
        }

        private async System.Threading.Tasks.Task ScanQRCodeAsync()
        {
            ZXingScannerPage scanPage = new ZXingScannerPage();
            await pageModel.NavigationService.PushModalAsync(scanPage);

            scanPage.OnScanResult += (result) =>
            {
                // Stop scanning
                scanPage.IsScanning = false;

                // Pop the page and show the result
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await pageModel.NavigationService.PopModalAsync();
                    Debug.WriteLine("QR Code found. Code is " + result.Text, "Info");

                    //Live-Check
                    if (SixDigitCodeValidation.IsValid(result.Text))
                    {
                        
                    }
                    else
                    {
                        pageModel.DialogService.ShowDialog(Services.DialogType.Error, 101);
                    }
                });
            };
        }
    }
}
