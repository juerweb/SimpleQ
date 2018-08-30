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
using Com.OneSignal;
using Com.OneSignal.Abstractions;

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

            SetupOneSignal();

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
                NavigateToMainPageModel(true);
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

        public static void NavigateToMainPageModel(Boolean LoadData)
        {
            //Localization Details
            ILanguageService languageService = FreshIOC.Container.Resolve<ILanguageService>();

            languageService.SetCurrentLanguage();
            Debug.WriteLine("Current Device Culture Info: " + CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName, "Info");

            //Set new Navigation Container
            MainMasterPageModel = new MainMasterPageModel();


            //questionService.LoadDataFromCache();
            if (LoadData)
            {
                IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
                questionService.LoadData();
            }



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

        private void SetupOneSignal()
        {
            OneSignal.Current.StartInit("68b8996a-f664-4130-9854-9ed7f70d5540")
                .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                .HandleNotificationOpened(HandleNotificationOpened)
                .HandleNotificationReceived(HandleNotificationReceived)
                .EndInit();
        }

        // Called when a notification is opened.
        // The name of the method can be anything as long as the signature matches.
        // Method must be static or this object should be marked as DontDestroyOnLoad
        private void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            Debug.WriteLine("HandleNotificationOpened...", "Info");
        }

        void HandleNotificationReceived(OSNotification result)
        {
            Debug.WriteLine("HandleNotificationReceived...", "Info");
            BlobCache.LocalMachine.InsertObject<String>("Test1002", "Hoho");
        }
    }
}
