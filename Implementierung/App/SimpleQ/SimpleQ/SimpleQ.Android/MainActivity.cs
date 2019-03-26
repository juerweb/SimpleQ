using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Acr.UserDialogs;
using Akavache;
using Com.OneSignal;
using Xamarin.Forms;
using Com.OneSignal.Abstractions;
using System.Diagnostics;

namespace SimpleQ.Droid
{
    [Activity(Label = "SimpleQ", Icon = "@mipmap/ic_launcher_2", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private OSNotificationOpenedResult result = null;
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            PowerSaverManager.StartPowerSaverIntent(this);

            OneSignal.Current.StartInit("68b8996a-f664-4130-9854-9ed7f70d5540").HandleNotificationOpened(HandleNotificationOpened).EndInit();

            #if PRESENTATION
            this.Window.AddFlags(WindowManagerFlags.Fullscreen);
            this.Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            #endif

            ZXing.Net.Mobile.Forms.Android.Platform.Init();
            UserDialogs.Init(this);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            if (this.result != null)
            {
                LoadApplication(new App(this.result));
            }
            else
            {
                LoadApplication(new App());
            }
            
        }

        private void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            this.result = result;
            System.Diagnostics.Debug.WriteLine("NO: Notification Opened in MainActivity (Android)...", "Info");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

