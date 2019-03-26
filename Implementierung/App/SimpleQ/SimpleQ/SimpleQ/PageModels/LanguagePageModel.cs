using FreshMvvm;
using SimpleQ.Logging;
using SimpleQ.PageModels.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the LanguagePageModel for the LanguagePage.
    /// </summary>
    public class LanguagePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguagePageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public LanguagePageModel(ILanguageService languageService): this()
        {
            //Debug.WriteLine("Constructor of LanguagePageModel", "Info");
            this.languageService = languageService;

            this.SelectedItem = languageService.GetCurrentLanguage();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguagePageModel"/> class.
        /// Without Parameter
        /// </summary>
        public LanguagePageModel()
        {
            ChangeLanguageCommand = new Command(ChangeLanguage);

            isSelected = false;
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
        /// <summary>
        /// The language service
        /// </summary>
        private ILanguageService languageService;
        /// <summary>
        /// The selected item of the Listview
        /// </summary>
        private CultureInfo selectedItem;
        /// <summary>
        /// Checks if something in the Listview is selected.
        /// </summary>
        private Boolean isSelected;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the language service.
        /// </summary>
        /// <value>
        /// The language service.
        /// </value>
        public ILanguageService LanguageService { get => languageService; set => languageService = value; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
        public CultureInfo SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                if (SelectedItem != null)
                {
                    IsSelected = true;
                }
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is selected.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is selected; otherwise, <c>false</c>.
        /// </value>
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets or sets the change language command.
        /// </summary>
        /// <value>
        /// The change language command.
        /// </value>
        public Command ChangeLanguageCommand
        {
            get;
            set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Method, which was called after the button was clicked. This Method change the current Language to the selected one.
        /// </summary>
        private void ChangeLanguage()
        {
            if (this.SelectedItem.TwoLetterISOLanguageName != Application.Current.Properties["Language"].ToString())
            {
                //Debug.WriteLine("Change Language to: " + this.SelectedItem.TwoLetterISOLanguageName, "Info");

                Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
                logger.Info("Change Language to " + this.SelectedItem.TwoLetterISOLanguageName);

                LanguageService.SetCurrentLanguage(this.SelectedItem.TwoLetterISOLanguageName);
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
