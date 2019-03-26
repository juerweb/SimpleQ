using SimpleQ.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Xamarin.Forms;

[assembly: Dependency(typeof(SimpleQ.UWP.CloseApplication))]
namespace SimpleQ.UWP
{
    public class CloseApplication : ICloseApplication
    {
        void ICloseApplication.CloseApplication()
        {
            Debug.WriteLine("Close Application...", "Info");
            Windows.UI.Xaml.Application.Current.Exit();
        }
    }
}
