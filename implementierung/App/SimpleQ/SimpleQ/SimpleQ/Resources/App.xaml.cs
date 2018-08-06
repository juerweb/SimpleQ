using SimpleQ.Pages;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SimpleQ.PageModels;
using FreshMvvm;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
		public App ()
		{
            InitializeComponent();

            var registerPageModel = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>();
            var navContainer = new FreshNavigationContainer(registerPageModel);
            MainPage = navContainer;
        }

		protected override void OnStart ()
		{
            // Handle when your app starts
            AppCenter.Start("android=d9823947-9138-4821-9dca-44c4750cb47e;" +
                  "uwp={Your UWP App secret here};" +
                  "ios=2f834068-f44d-4db0-8e56-8497f11d179a;",
                  typeof(Analytics), typeof(Crashes));
        }

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
