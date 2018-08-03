using SimpleQ.PageModels;
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
	public partial class RegisterPage : ContentPage
	{
		public RegisterPage ()
		{
            InitializeComponent ();

            RegisterPageModel pageModel = new RegisterPageModel();
            this.BindingContext = pageModel;

            this.logoImage.Source = ImageSource.FromResource(pageModel.Model.ImageSource);
            this.sixDigitCodeEntry.Behaviors.Add(pageModel.Behavior);

        }
	}
}