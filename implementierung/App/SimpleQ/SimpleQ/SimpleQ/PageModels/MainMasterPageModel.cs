using FreshMvvm;
using MvvmHelpers;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Pages;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
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
            Debug.WriteLine("Constructor of MainMasterPageModel...", "Info");
            MenuItems = new ObservableCollection<MenuItemListModel>();
            this.MenuItems.Add(new MenuItemListModel(ItemType.Categorie.ToString()));
            this.MenuItems.Add(new MenuItemListModel(ItemType.Navigation.ToString()));

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

        public ObservableCollection<MenuItemListModel> MenuItems { get; set; }

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

            MainMasterPage mainMasterPage = new MainMasterPage();
            mainMasterPage.BindingContext = this;
            Master = mainMasterPage;
        }

        public void AddPage(string title, FreshBasePageModel pageModel, String iconResourceName)
        {
            var page = FreshPageModelResolver.ResolvePageModel(pageModel.GetType(), null);
            page.GetModel().CurrentNavigationServiceName = NavigationServiceName;
            var navigationContainer = CreateContainerPage(page);
            /*if (this.MenuItems[0].Count == 0 && this.MenuItems[1].Count == 0)
            {
                if (itemType == ItemType.CatName)
                {
                    Detail = _pages[AppResources.AllCategories];
                }
                else
                {
                    Detail = navigationContainer;
                }
                
            }*/
            Debug.WriteLine("Add new Page");

            _pages.Add(title, navigationContainer);

            this.MenuItems[1].Add(new MenuItemModel(title, pageModel, iconResourceName));
        }

        public void AddCategorie(string title)
        {
            if (title == AppResources.AllCategories)
            {
                FrontPageModel pageModel = new FrontPageModel();
                var page = FreshPageModelResolver.ResolvePageModel(pageModel.GetType(), null);
                page.GetModel().CurrentNavigationServiceName = NavigationServiceName;
                var navigationContainer = CreateContainerPage(page);

                Detail = navigationContainer;

                _pages.Add(title, navigationContainer);

                this.MenuItems[0].Add(new MenuItemModel(title, pageModel, null));
            }
            else
            {
                this.MenuItems[0].Add(new MenuItemModel(title, null, null));
            }

            
        }

        private void NavigateToNewPage()
        {

            if (selectedItem != null)
            {

                Debug.WriteLine("Navigate to new page: " + selectedItem.Title, "Info");

                IsPresented = false;

                if (this.MenuItems[0].Contains(SelectedItem))
                {
                    //the selected item is a catName
                    IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
                    questionService.SetCategorieFilter(SelectedItem.Title);

                    Detail = _pages[AppResources.AllCategories];
                    
                }
                else
                {
                    Detail = _pages[SelectedItem.Title];
                }
            }

        }

        public void SetNewCategorie(String title)
        {
            if (MenuItems[0].Count(mi => mi.Title == title) == 1)
            {
                Debug.WriteLine("Set new catName to: " + title, "Info");

                this.SelectedItem = MenuItems[0].Where(mi => mi.Title == title).ToList()[0];
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
