using FreshMvvm;
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
    /// This is the LanguagePageModel for the Page xy.
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
            Debug.WriteLine("Constructor of LanguagePageModel", "Info");
            this.languageService = languageService;

            this.SelectedItem = languageService.GetCurrentLanguage();

            Debug.WriteLine("1", "Info");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LanguagePageModel"/> class.
        /// Without Parameter
        /// </summary>
        public LanguagePageModel()
        {
            Debug.WriteLine("2", "Info");
            ChangeLanguageCommand = new Command(ChangeLanguage);

            isSelected = false;
            Debug.WriteLine("3", "Info");
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            Debug.WriteLine("4", "Info");
            base.Init(initData);
            Debug.WriteLine("5", "Info");
        }
        #endregion

        #region Fields
        private ILanguageService languageService;
        private CultureInfo selectedItem;
        private Boolean isSelected;
        #endregion

        #region Properties + Getter/Setter Methods
        public ILanguageService LanguageService { get => languageService; set => languageService = value; }

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
        public Command ChangeLanguageCommand
        {
            get;
            set;
        }
        #endregion

        #region Methods
        private async void ChangeLanguage()
        {
            Debug.WriteLine("Change Language to: " + this.SelectedItem.TwoLetterISOLanguageName, "Info");

            LanguageService.SetCurrentLanguage(this.SelectedItem.TwoLetterISOLanguageName);
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
