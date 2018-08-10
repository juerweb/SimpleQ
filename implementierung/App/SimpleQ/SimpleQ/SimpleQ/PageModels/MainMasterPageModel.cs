using FreshMvvm;
using MvvmHelpers;
using SimpleQ.Models;
using SimpleQ.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the MainMasterPageModel for the Page xy.
    /// </summary>
    public class MainMasterPageModel : FreshMasterDetailNavigationContainer, INotifyPropertyChanged
    {
        #region Constructor(s)
        public MainMasterPageModel(): base()
        {
            MenuItems = new ObservableCollection<Grouping<ItemType, MainMenuItemModel>>();

            //Generate CodeValidationModel from Application Properties
            Boolean isValidCodeAvailable = (bool)Application.Current.Properties["IsValidCodeAvailable"];
            String companyName = (string)Application.Current.Properties["CompanyName"];
            String departmentName = (string)Application.Current.Properties["DepartmentName"];
            int registerCode = (int)Application.Current.Properties["RegisterCode"];

            this.CodeValidationModel = new CodeValidationModel(isValidCodeAvailable, companyName, departmentName, registerCode);
        }
        #endregion

        #region Fields
        private CodeValidationModel codeValidationModel;
        private MainMenuItemModel selectedItem;

        private List<MainMenuItemModel> categories = new List<MainMenuItemModel>();
        private List<MainMenuItemModel> navigations = new List<MainMenuItemModel>();

        private Dictionary<String, Page> pages = new Dictionary<string, Page>();
        #endregion

        #region Properties + Getter/Setter Methods
        public CodeValidationModel CodeValidationModel
        {
            get => codeValidationModel;
            set
            {
                codeValidationModel = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Grouping<ItemType, MainMenuItemModel>> MenuItems { get; set; }

        public MainMenuItemModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                NavigateToNewPage();
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        protected override void CreateMenuPage(string menuPageTitle, string menuIcon = null)
        {
            Debug.WriteLine("Create Menu Page...", "Info");
            this.MenuItems.Add(new Grouping<ItemType, MainMenuItemModel>(ItemType.Categorie, categories));
            this.MenuItems.Add(new Grouping<ItemType, MainMenuItemModel>(ItemType.Navigation, navigations));

            MainMasterPage mainMasterPage = new MainMasterPage();
            mainMasterPage.BindingContext = this;
            Master = mainMasterPage;
        }

        public void AddPage(string title, ItemType itemType, FreshBasePageModel pageModel)
        {
            var page = FreshPageModelResolver.ResolvePageModel(pageModel.GetType(), null);
            page.GetModel().CurrentNavigationServiceName = NavigationServiceName;
            var navigationContainer = CreateContainerPage(page);
            if (this.categories.Count == 0 && this.navigations.Count == 0)
            {
                Detail = navigationContainer;
            }

            Debug.WriteLine("Add new Page");

            pages.Add(title, navigationContainer);

            switch (itemType)
            {
                case ItemType.Categorie:
                    categories.Add(new MainMenuItemModel(0, title, pageModel));
                    break;
                case ItemType.Navigation:
                    navigations.Add(new MainMenuItemModel(0, title, pageModel));
                    break;
            }
        }

        private void NavigateToNewPage()
        {

            if (selectedItem != null)
            {
                Debug.WriteLine("Navigate to new page: " + selectedItem.Title, "Info");

                IsPresented = false;

                Detail = pages[SelectedItem.Title];
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
