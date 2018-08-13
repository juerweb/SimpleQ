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
using SimpleQ.Models;
using System.Diagnostics;
using System.Threading.Tasks;
using SimpleQ.Extensions;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{

		public App ()
		{
            //Application.Current.Properties.Remove("IsValidCodeAvailable");

            InitializeComponent();

            SetupIOC();

            // To set MainPage for the Application  
            if (!Application.Current.Properties.Keys.Contains("IsValidCodeAvailable"))
            {
                //Code is not available => RegisterPage
                Debug.WriteLine("Property 'CodeValidationModel' is not available...", "Info");
                NavigateToRegisterPageModel();

            }
            else if ((bool)Application.Current.Properties["IsValidCodeAvailable"])
            {
                //Code is available => MainPage
                Debug.WriteLine("Code is valid now...", "Info");
                NavigateToMainPageModel();
            }
            else
            {
                //Code is not available => RegisterPage
                Debug.WriteLine("Code is not valid now...", "Info");
                NavigateToRegisterPageModel();
            }
        }

        private void NavigateToRegisterPageModel()
        {
            var page = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>();
            var basicNavContainer = new FreshNavigationContainer(page);
            MainPage = basicNavContainer;
        }

        public static void NavigateToMainPageModel()
        {
            var masterDetailNav = new MainMasterPageModel();
            
            masterDetailNav.AddPage("Test1", ItemType.Categorie, new Test1PageModel(), null);
            masterDetailNav.AddPage("Test2", ItemType.Navigation, new Test2PageModel(), "ic_extension_black_18dp.png");
            masterDetailNav.AddPage("Test3", ItemType.Navigation, new Test3PageModel(), "ic_extension_black_18dp.png");
            masterDetailNav.Init("Menu");
            Application.Current.MainPage = masterDetailNav;
        }

        private void SetupIOC()
        {
            FreshIOC.Container.Register<IUserDialogs>(UserDialogs.Instance);
            FreshIOC.Container.Register<IDialogService, DialogService>();
            FreshIOC.Container.Register<ISimulationService, SimulationService>();
        }

		protected override async void OnStart ()
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
