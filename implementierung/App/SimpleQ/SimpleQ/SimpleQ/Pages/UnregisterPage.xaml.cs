using IntelliAbb.Xamarin.Controls;
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
            Checkbox box = (Checkbox)sender;
            RegistrationData option = (RegistrationData)(box.BindingContext);

            UnregisterPageModel pageModel = (UnregisterPageModel)(this.BindingContext);
            pageModel.IsChecked[option] = box.IsChecked;

            if (box.IsChecked)
            {
                pageModel.IsOneChecked = true;
            }
            else
            {
                foreach (Boolean b in pageModel.IsChecked.Values)
                {
                    if (b)
                    {
                        pageModel.IsOneChecked = true;
                        return;
                    }
                }
                pageModel.IsOneChecked = false;
            }
        }
    }
}