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
	public partial class QRCodeScannerPage : ContentPage
	{
		public QRCodeScannerPage ()
		{
			InitializeComponent ();

            TimeSpan ts = new TimeSpan(0, 0, 0, 3, 0);
            Device.StartTimer(ts, () => {
                if (scannerView.IsScanning)
                {
                    scannerView.AutoFocus();
                }

                return true;
            });
        }
	}
}