using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.PageModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public QRCodeScannerPageModel(IUserDialogs dialogs) : this()
        {
            this.DialogService = dialogs;
        }

        public QRCodeScannerPageModel()
        {
            //this.DialogService.Alert("Test", "Tewst");
            ResultCommand = new Command(OnScanningResult);
        }

        public override void Init(object initData)
        {
            base.Init(initData);
            this.DialogService.ShowLoading(initData.ToString());
        }
        #endregion

        #region Fields
        private IUserDialogs dialogService;
        #endregion

        #region Properties + Getter/Setter Methods
        public IUserDialogs DialogService { get => dialogService; set => dialogService = value; }
        #endregion

        #region Commands
        public Command ResultCommand
        {
            get;
        }
        #endregion

        #region Methods
        private void OnScanningResult(object parameter)
        {
            this.DialogService.Alert(parameter.ToString());
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
