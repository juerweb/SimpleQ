﻿using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Extensions;
using SimpleQ.Logging;
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
            //Debug.WriteLine("Constructor of RegisterPageModel...", "Info");
            OpenScannerCommand = new Command(OnOpenScanner);
            ManualCodeEntryCommand = new Command(OnManualCodeEntry);
            this.Behavior = new SixDigitCodeBehavior();
        }

        public override async void Init(object initData)
        {
            if (initData != null)
            {
                this.isRegistration = (Boolean)initData;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The model field variable
        /// </summary>
        private string registerCode;

        /// <summary>
        /// The behavior
        /// </summary>
        private SixDigitCodeBehavior behavior;

        /// <summary>
        /// The dialog service
        /// </summary>
        private IUserDialogs dialogService;

        private Boolean isRegistration;

        private Boolean debugMode;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public string RegisterCode
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

        public bool IsRegistration { get => isRegistration; set => isRegistration = value; }

        public bool DebugMode { get => debugMode; set => debugMode = value; }

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
            //Debug.WriteLine("ManualCodeEntryCommand executed", "Info");
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("ManualCodeEntryCommand executed.");
            string code = this.RegisterCode;
            this.RegisterCode = "";
            CoreMethods.PushPageModel<LoadingPageModel>(new List<object> { code, this.isRegistration, this.DebugMode });
        }

        public void OnOpenScanner()
        {
            //Debug.WriteLine("OpenScannerCommand executed", "Info");
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("OpenScannerCommand executed.");
            CoreMethods.PushPageModel<QRCodeScannerPageModel>(this.isRegistration);
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
