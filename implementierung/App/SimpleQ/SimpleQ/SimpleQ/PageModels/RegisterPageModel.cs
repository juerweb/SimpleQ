using Acr.UserDialogs;
using SimpleQ.Extensions;
using SimpleQ.Models;
using SimpleQ.PageModels.Commands;
using SimpleQ.PageModels.Services;
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
    /// This is the RegisterPageModel for the RegisterPage.
    /// </summary>
    public class RegisterPageModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterPageModel"/> class.
        /// </summary>
        public RegisterPageModel(INavigation navigation, IUserDialogs dialogs)
        {
            this.Model = new RegisterModel();
            ScanCommand = new ScanQRCodeCommand();
            ManualCommand = new ManualCodeCommand();
            this.Behavior = new SixDigitCodeBehavior();

            this.navigationService = navigation;
            this.dialogService = new DialogService(dialogs);
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
        private INavigation navigationService;

        /// <summary>
        /// The dialog service
        /// </summary>
        private DialogService dialogService;
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
        public INavigation NavigationService { get => navigationService; }

        /// <summary>
        /// Gets or sets the dialog service.
        /// </summary>
        /// <value>
        /// The dialog service.
        /// </value>
        public DialogService DialogService { get => dialogService; }
        #endregion

        #region Commands
        public ScanQRCodeCommand ScanCommand
        {
            get;
        }

        public ManualCodeCommand ManualCommand
        {
            get;
        }
        #endregion

        #region Methods
        public void CheckingCode()
        {
            Debug.WriteLine("Checking Code....", "Info");
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
