using IntelliAbb.Xamarin.Controls;
using SimpleQ.Models;
using SimpleQ.PageModels;
using SimpleQ.Shared;
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
	public partial class UnregisterPage : ContentPage
	{
		public UnregisterPage ()
		{
			InitializeComponent ();
		}

        private void Checkbox_IsCheckedChanged(object sender, TappedEventArgs e)
        {
            if (this.BindingContext != null)
            {
                UnregisterPageModel pageModel = (UnregisterPageModel)(this.BindingContext);
                foreach (IsCheckedModel<RegistrationDataModel> model in pageModel.IsChecked)
                {
                    if (model.IsChecked)
                    {
                        pageModel.IsOneChecked = true;
                        return;
                    }
                }
                pageModel.IsOneChecked = false;
            }

        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                IsCheckedModel<RegistrationDataModel> model = (IsCheckedModel<RegistrationDataModel>)e.SelectedItem;
                UnregisterPageModel pageModel = (UnregisterPageModel)(this.BindingContext);
                model.IsChecked = !model.IsChecked;
                listView.SelectedItem = null;
            }
        }
    }
}