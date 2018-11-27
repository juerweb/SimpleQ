using System;
using System.Collections.Generic;
using System.Linq;
using Acr.UserDialogs;
using Com.OneSignal;
using Foundation;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace SimpleQ.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.Init();

            ZXing.Net.Mobile.Forms.iOS.Platform.Init();

            LoadApplication(new App());

            OneSignal.Current.StartInit("68b8996a-f664-4130-9854-9ed7f70d5540").EndInit();

            return base.FinishedLaunching(app, options);
        }
    }
}
