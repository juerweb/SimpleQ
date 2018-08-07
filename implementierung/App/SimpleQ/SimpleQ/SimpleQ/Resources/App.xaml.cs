using SimpleQ.Pages;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using FreshMvvm;
using SimpleQ.PageModels;
using Acr.UserDialogs;
using SimpleQ.PageModels.Services;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
		public App ()
		{
            InitializeComponent();

            SetupIOC();

            // To set MainPage for the Application  
            var page = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>();
            var basicNavContainer = new FreshNavigationContainer(page);
            MainPage = basicNavContainer;
        }

        private void SetupIOC()
        {
            FreshIOC.Container.Register<IUserDialogs>(UserDialogs.Instance);
            FreshIOC.Container.Register<ISimulationService, SimulationService>();
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
