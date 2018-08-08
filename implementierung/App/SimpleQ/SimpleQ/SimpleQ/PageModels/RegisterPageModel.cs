using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Extensions;
using SimpleQ.PageModels.Commands;
using SimpleQ.PageModels.Services;
using SimpleQ.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the RegisterPageModel for the RegisterPage.
    /// </summary>
    public class RegisterPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterPageModel"/> class.
        /// </summary>
        public RegisterPageModel(IUserDialogs dialogs): this()
        {
            this.dialogService = dialogs;
        }

        public RegisterPageModel()
        {
            OpenScannerCommand = new Command(OnOpenScanner);
            ManualCodeEntryCommand = new Command(OnManualCodeEntry);
            this.Behavior = new SixDigitCodeBehavior();
        }
        #endregion

        #region Fields
        /// <summary>
        /// The model field variable
        /// </summary>
        private int registerCode;

        /// <summary>
        /// The behavior
        /// </summary>
        private SixDigitCodeBehavior behavior;

        /// <summary>
        /// The dialog service
        /// </summary>
        private IUserDialogs dialogService;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public int RegisterCode
        {
            get
            {
                return registerCode;
            }
            set
            {
                registerCode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the behavior.
        /// </summary>
        /// <value>
        /// The behavior.
        /// </value>
        public SixDigitCodeBehavior Behavior { get => behavior; set => behavior = value; }

        /// <summary>
        /// Gets or sets the dialog service.
        /// </summary>
        /// <value>
        /// The dialog service.
        /// </value>
        public IUserDialogs DialogService { get => dialogService; }
        #endregion

        #region Commands
        public Command OpenScannerCommand
        {
            get;
        }

        public Command ManualCodeEntryCommand
        {
            get;
        }
        #endregion

        #region Methods

        public void OnManualCodeEntry()
        {
            Debug.WriteLine("ManualCodeEntryCommand executed", "Info");
            CoreMethods.PushPageModel<LoadingPageModel>(this.RegisterCode);
        }

        public void OnOpenScanner()
        {
            Debug.WriteLine("OpenScannerCommand executed", "Info");
            CoreMethods.PushPageModel<QRCodeScannerPageModel>();
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
