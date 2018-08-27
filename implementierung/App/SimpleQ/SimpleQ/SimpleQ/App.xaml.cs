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
using Plugin.Multilingual;
using SimpleQ.Resources;
using System.Globalization;
using System.Collections.Generic;
using Akavache;
using System.Reactive.Linq;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{

		public App ()
		{
            //Application.Current.Properties.Remove("IsValidCodeAvailable");
            //Application.Current.Properties["Language"] = "en";

            SetupIOC();

            SetupBlobCache();

            InitializeComponent();

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
            //Localization Details
            ILanguageService languageService = FreshIOC.Container.Resolve<ILanguageService>();

            languageService.SetCurrentLanguage();
            Debug.WriteLine("Current Device Culture Info: " + CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName, "Info");

            var page = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>();
            var basicNavContainer = new FreshNavigationContainer(page);
            MainPage = basicNavContainer;
        }

        public async static void NavigateToMainPageModel()
        {
            //Localization Details
            ILanguageService languageService = FreshIOC.Container.Resolve<ILanguageService>();

            languageService.SetCurrentLanguage();
            Debug.WriteLine("Current Device Culture Info: " + CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName, "Info");

            //Set new Navigation Container
            MainMasterPageModel = new MainMasterPageModel();

            IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
            questionService.LoadDataFromCache();
            questionService.CheckIfRequestIsNeeded();


            MainMasterPageModel.AddCategorie(AppResources.AllCategories);
            MainMasterPageModel.AddPage(AppResources.Settings, new SettingsPageModel(), "ic_settings_black_18.png");
            MainMasterPageModel.AddPage(AppResources.Help, new HelpPageModel(), "ic_help_black_18.png");
            MainMasterPageModel.Init("Menu");


            Application.Current.MainPage = MainMasterPageModel;
        }

        private void SetupIOC()
        {
            FreshIOC.Container.Register<IUserDialogs>(UserDialogs.Instance);
            FreshIOC.Container.Register<IDialogService, DialogService>();
            FreshIOC.Container.Register<ISimulationService, SimulationService>();
            FreshIOC.Container.Register<ILanguageService, LanguageService>();
            FreshIOC.Container.Register<IQuestionService, QuestionService>();
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

        public static MainMasterPageModel MainMasterPageModel
        {
            get;
            set;
        }

        private void SetupBlobCache()
        {
            BlobCache.ApplicationName = "SimpleQ";
        }
    }
}
