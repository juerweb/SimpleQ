using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimpleQ.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : MasterDetailPage
    {
        public MainPage()
        {
            InitializeComponent();
            MenuItemsListView.ItemSelected += ListView_ItemSelected;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var item = e.SelectedItem as MainMenuItemModel;
            if (item == null)
                return;

            var page = (Page)Activator.CreateInstance(item.PageModelTyp);
            page.Title = item.Title;

            Detail = new NavigationPage(page);
            IsPresented = false;

            MenuItemsListView.SelectedItem = null;
        }
    }
}