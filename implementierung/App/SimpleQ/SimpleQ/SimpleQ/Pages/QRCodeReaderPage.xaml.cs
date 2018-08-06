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
	public partial class QRCodeReaderPage : ContentPage
	{
		public QRCodeReaderPage ()
		{
			InitializeComponent ();

            this.logoImage.Source = ImageSource.FromResource(AppResources.LogoResourcename);
        }
	}
}