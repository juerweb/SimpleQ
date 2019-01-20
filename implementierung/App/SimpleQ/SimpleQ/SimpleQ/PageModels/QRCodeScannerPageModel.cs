using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Logging;
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
            isRegistration = true;
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            if (initData != null)
            {
                isRegistration = (Boolean)initData;
            }
        }
        #endregion

        #region Fields
        private IDialogService dialogService;
        private Boolean isScanning;
        private Boolean isRegistration;
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
            //Debug.WriteLine("QR Code found. Code is " + parameter.ToString(), "Info");
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("QR Code found. Code is " + parameter.ToString());
            Device.BeginInvokeOnMainThread(async () =>
            {
                IsScanning = false;
                //Live-Check
                if (SixDigitCodeValidation.IsValid(parameter.ToString()))
                {
                    //Debug.WriteLine("Live-Check: QR-Code is valid.", "Info");
                    //Check if Code is in DB
                    try
                    {
                        await CoreMethods.PushPageModel<LoadingPageModel>(new List<object> { int.Parse(parameter.ToString()), isRegistration });
                    }
                    catch (Exception e)
                    {
                        //Debug.WriteLine("QR Code has not the right format...", "Info");
                        logger.Warn("QR Code has not the right format.");

                        this.dialogService.ShowErrorDialog(101);
                        await CoreMethods.PopToRoot(false);
                        return;
                    }
                }
                else
                {
                    //Debug.WriteLine("Live-Check: QR-Code is not valid.", "Info");
                    DialogService.ShowErrorDialog(101);
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
