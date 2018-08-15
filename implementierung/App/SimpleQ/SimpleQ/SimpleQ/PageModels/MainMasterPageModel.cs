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
            MenuItems = new ObservableCollection<Grouping<ItemType, MenuItemModel>>();

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
        private MenuItemModel selectedItem;

        private List<MenuItemModel> categories = new List<MenuItemModel>();
        private List<MenuItemModel> navigations = new List<MenuItemModel>();

        private Dictionary<String, Page> _pages = new Dictionary<string, Page>();
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

        public ObservableCollection<Grouping<ItemType, MenuItemModel>> MenuItems { get; set; }

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

        public Dictionary<string, Page> Pages { get => _pages;}
        #endregion

        #region Commands
        #endregion

        #region Methods
        protected override void CreateMenuPage(string menuPageTitle, string menuIcon = null)
        {
            Debug.WriteLine("Create Menu Page...", "Info");
            this.MenuItems.Add(new Grouping<ItemType, MenuItemModel>(ItemType.Categorie, categories));
            this.MenuItems.Add(new Grouping<ItemType, MenuItemModel>(ItemType.Navigation, navigations));

            MainMasterPage mainMasterPage = new MainMasterPage();
            mainMasterPage.BindingContext = this;
            Master = mainMasterPage;
        }

        public void AddPage(string title, ItemType itemType, FreshBasePageModel pageModel, String iconResourceName)
        {
            var page = FreshPageModelResolver.ResolvePageModel(pageModel.GetType(), null);
            page.GetModel().CurrentNavigationServiceName = NavigationServiceName;
            var navigationContainer = CreateContainerPage(page);
            if (this.categories.Count == 0 && this.navigations.Count == 0)
            {
                Detail = navigationContainer;
            }

            Debug.WriteLine("Add new Page");

            _pages.Add(title, navigationContainer);

            switch (itemType)
            {
                case ItemType.Categorie:
                    categories.Add(new MenuItemModel(title, pageModel));
                    break;
                case ItemType.Navigation:
                    navigations.Add(new MenuItemModel(title, pageModel, iconResourceName));
                    break;
            }
        }

        private void NavigateToNewPage()
        {

            if (selectedItem != null)
            {
                Debug.WriteLine("Navigate to new page: " + selectedItem.Title, "Info");

                IsPresented = false;

                Detail = _pages[SelectedItem.Title];
                Debug.WriteLine("TEST");
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
