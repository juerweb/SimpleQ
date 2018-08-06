using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Extensions;
using SimpleQ.Models;
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
        public RegisterPageModel()
        {
            this.Model = new RegisterModel();

            ManualCommand = new ManualCodeCommand();

            OpenScanPageCommand = new Command(() =>
            {
                CoreMethods.PushPageModel<QRCodeReaderPageModel>();
            });

            this.Behavior = new SixDigitCodeBehavior();
            IsIndicatorRunning = false;

            //this.navigationService = navigation;
            //this.dialogService = new DialogService(dialogs);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The model field variable
        /// </summary>
        private RegisterModel model;

        /// <summary>
        /// The behavior
        /// </summary>
        private SixDigitCodeBehavior behavior;

        /// <summary>
        /// The navigation service
        /// </summary>
        //private INavigation navigationService;

        /// <summary>
        /// The dialog service
        /// </summary>
        private DialogService dialogService;

        private Boolean isIndicatorRunning;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public RegisterModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
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
        /// Gets or sets the navigation service.
        /// </summary>
        /// <value>
        /// The navigation service.
        /// </value>
        //public INavigation NavigationService { get => navigationService; }

        /// <summary>
        /// Gets or sets the dialog service.
        /// </summary>
        /// <value>
        /// The dialog service.
        /// </value>
        public DialogService DialogService { get => dialogService; }

        public bool IsIndicatorRunning
        {
            get
            {
                return isIndicatorRunning;
            }
            set
            {
                isIndicatorRunning = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands


        public ManualCodeCommand ManualCommand
        {
            get;
        }

        public Command OpenScanPageCommand
        {
            get;
        }
        #endregion

        #region Methods
        public void CheckingCode()
        {
            LoadingPage loadingPage = new LoadingPage();
            Debug.WriteLine("Checking Code....", "Info");
            //navigationService.PushModalAsync(loadingPage);
            Debug.WriteLine("Loading Data...", "Info");

            new Thread(() =>
            {
                Thread.Sleep(2000);
                ((LoadingPageModel)loadingPage.BindingContext).IsFirstStepTicked = true;
                Thread.Sleep(1000);
                ((LoadingPageModel)loadingPage.BindingContext).IsSecondStepTicked = true;
                Thread.Sleep(4000);
                ((LoadingPageModel)loadingPage.BindingContext).IsThirdStepTicked = true;

                //navigationService.PopModalAsync();
                //navigationService.PushModalAsync(new Main());
            }).Start();


            

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
