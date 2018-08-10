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
    /// This is the MainPageModel for the Page xy.
    /// </summary>
    public class MainPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public MainPageModel()
        {
            MenuItems = new ObservableCollection<Grouping<ItemType, MainMenuItemModel>>();

            List<MainMenuItemModel> categoriesItems = new List<MainMenuItemModel>();
            categoriesItems.Add(new MainMenuItemModel(1, "Test Categorie 1", typeof(SimulationPageModel)));

            List<MainMenuItemModel> navigationItems = new List<MainMenuItemModel>();
            navigationItems.Add(new MainMenuItemModel(2, "Test Navigation 1", typeof(SimulationPageModel)));
            navigationItems.Add(new MainMenuItemModel(3, "Test Navigation 2", typeof(SimulationPageModel)));

            this.MenuItems.Add(new Grouping<ItemType, MainMenuItemModel>(ItemType.Categorie, categoriesItems));
            this.MenuItems.Add(new Grouping<ItemType, MainMenuItemModel>(ItemType.Navigation, navigationItems));
        }

        public override void Init(object initData)
        {
            base.Init(initData);

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
        #endregion

        #region Commands
        #endregion

        #region Methods
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
