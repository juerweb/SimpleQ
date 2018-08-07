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
        }

        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private IDialogService dialogService;
        #endregion

        #region Properties + Getter/Setter Methods
        public IDialogService DialogService { get => dialogService; set => dialogService = value; }
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

            //Live-Check
            if (SixDigitCodeValidation.IsValid(parameter.ToString()))
            {
                //pageModel.CheckingCode();
                Debug.WriteLine("Live-Check: QR-Code is valid.", "Info");
            }
            else
            {
                DialogService.ShowDialog(Services.DialogType.Error, 101);
            }
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
