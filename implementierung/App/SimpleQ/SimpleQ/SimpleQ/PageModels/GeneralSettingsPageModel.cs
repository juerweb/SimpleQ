using Akavache;
using FreshMvvm;
using SimpleQ.PageModels.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Reactive.Linq;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the GeneralSettingsPageModel for the GeneralSettingsPage.
    /// </summary>
    public class GeneralSettingsPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettingsPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public GeneralSettingsPageModel(object param): this()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralSettingsPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public GeneralSettingsPageModel()
        {
            SetValuesFromMemory();
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private Boolean closeAppAfterNotification;


        #endregion

        #region Properties + Getter/Setter Methods
        public Boolean CloseAppAfterNotification
        {
            get => closeAppAfterNotification;
            set
            {
                closeAppAfterNotification = value;
                OnPropertyChanged();
                BlobCache.UserAccount.InvalidateObject<Boolean>("CloseAppAfterNotification");
                BlobCache.UserAccount.InsertObject<Boolean>("CloseAppAfterNotification", closeAppAfterNotification);
                Debug.WriteLine("ShowFrontPageAfterNotification changed...", "Info");
            } 
        }

        #endregion

        #region Commands
        #endregion

        #region Methods
        private void SetValuesFromMemory()
        {
            try
            {
                BlobCache.UserAccount.GetObject<Boolean>("CloseAppAfterNotification").Subscribe(obj => { CloseAppAfterNotification = obj; });
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error: " + e.StackTrace, "Error");
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
