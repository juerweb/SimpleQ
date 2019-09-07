using FreshMvvm;
using SimpleQ.Models;
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
    /// This is the StandardMenuPageModel for all other PageModel, whichs needs a Menu Structure.
    /// </summary>
    public class StandardMenuPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMenuPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public StandardMenuPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardMenuPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public StandardMenuPageModel()
        {
            menuItems = new ObservableCollection<MenuItemModel>();
            //Debug.WriteLine("Constructor of StandrdMenuPageModel", "Info");
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
        /// The menu items
        /// </summary>
        private ObservableCollection<MenuItemModel> menuItems;
        /// <summary>
        /// The selected item
        /// </summary>
        private MenuItemModel selectedItem;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the menu items.
        /// </summary>
        /// <value>
        /// The menu items.
        /// </value>
        public ObservableCollection<MenuItemModel> MenuItems { get => menuItems; set => menuItems = value; }
        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        /// <value>
        /// The selected item.
        /// </value>
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
        /// <summary>
        /// Navigates to new page.
        /// </summary>
        private async void NavigateToNewPage()
        {
            await CoreMethods.PushPageModel(this.SelectedItem.PageModelTyp.GetType(), false);
            SelectedItem = null;
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
