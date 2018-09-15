using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using SimpleQ.Extensions;
using Xamarin.Forms;

[assembly: Dependency(typeof(SimpleQ.Droid.CloseApplication))]
namespace SimpleQ.Droid
{
    public class CloseApplication : ICloseApplication
    {
        void ICloseApplication.CloseApplication()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());

            //var activity = (Activity)Forms.Context;
            //activity.FinishAffinity();
        }
    }
}