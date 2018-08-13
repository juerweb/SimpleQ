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

            //LanguageService.SetCurrentLanguage(this.SelectedItem.TwoLetterISOLanguageName);

            await CoreMethods.PopPageModel();
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
