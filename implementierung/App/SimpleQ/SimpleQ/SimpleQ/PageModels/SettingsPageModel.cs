using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the SettingsPageModel for the Page Settings.
    /// </summary>
    public class SettingsPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public SettingsPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public SettingsPageModel()
        {
            menuItems = new ObservableCollection<MenuItemModel>();
            menuItems.Add(new MenuItemModel(AppResources.Language, new LanguagePageModel(), "ic_language_black_18.png"));
            Debug.WriteLine("Constructor of SettingsPageModel", "Info");
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override async void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private ObservableCollection<MenuItemModel> menuItems;
        private MenuItemModel selectedItem;
        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<MenuItemModel> MenuItems { get => menuItems; set => menuItems = value; }
        public MenuItemModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                OnPropertyChanged();
                if (selectedItem != null)
                {
                    NavigateToNewPage();
                }
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        private async void NavigateToNewPage()
        {
            await CoreMethods.PushPageModel <LanguagePageModel> ();
            SelectedItem = null;
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            Debug.WriteLine("PropertyChanged: " + propertyName, "Info");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
