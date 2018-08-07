using SimpleQ.PageModels;
using SimpleQ.Resources;
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
	public partial class LoadingPage : ContentPage
	{
		public LoadingPage ()
		{
			InitializeComponent ();

            //this.BindingContext = new LoadingPageModel();

            this.logoImage.Source = ImageSource.FromResource(AppResources.LogoResourcename);
        }
	}
}