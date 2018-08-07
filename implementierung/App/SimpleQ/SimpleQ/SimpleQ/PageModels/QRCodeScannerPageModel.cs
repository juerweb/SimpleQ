using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.PageModels.Commands;
using SimpleQ.PageModels.Services;
using SimpleQ.Validations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the QRCodeScannerPageModel for the Page xy.
    /// </summary>
    public class QRCodeScannerPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public QRCodeScannerPageModel(IDialogService dialogService) : this()
        {
            this.DialogService = dialogService;
        }

        public QRCodeScannerPageModel()
        {
            ScanningResultCommand = new Command(OnScanningResult);
            IsScanning = true;
        }

        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private IDialogService dialogService;
        private Boolean isScanning;
        #endregion

        #region Properties + Getter/Setter Methods
        public IDialogService DialogService { get => dialogService; set => dialogService = value; }

        public bool IsScanning { get => isScanning; set { isScanning = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        public Command ScanningResultCommand
        {
            get;
        }

        #endregion

        #region Methods
        private void OnScanningResult(object parameter)
        {
            Debug.WriteLine("QR Code found. Code is " + parameter.ToString(), "Info");
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsScanning = false;
                //Live-Check
                if (SixDigitCodeValidation.IsValid(parameter.ToString()))
                {
                    Debug.WriteLine("Live-Check: QR-Code is valid.", "Info");
                    //Check if Code is in DB
                    await CoreMethods.PushPageModel<LoadingPageModel>(int.Parse(parameter.ToString()));
                }
                else
                {
                    Debug.WriteLine("Live-Check: QR-Code is not valid.", "Info");
                    DialogService.ShowDialog(Services.DialogType.Error, 101);
                    await CoreMethods.PopPageModel();
                }
            });
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
