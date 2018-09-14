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
using SimpleQ.PageModels.QuestionPageModels;
using SimpleQ.Pages.QuestionPages;
using Xamarin.Forms.Internals;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
        private static Boolean WasThereAlreadyANotification = false;
        public App ()
		{
            //Application.Current.Properties.Remove("IsValidCodeAvailable");
            //Application.Current.Properties["Language"] = "en";

            SetupIOC();

            SetupBlobCache();



            InitializeComponent();

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                SetupOneSignal();
            }

            GoToRightPage();

        }

        public static void GoToRightPage()
        {
                Debug.WriteLine("T1");
                if (Application.Current.Properties.ContainsKey("IsValidCodeAvailable"))
                {
                    if ((bool)Application.Current.Properties["IsValidCodeAvailable"])
                    {
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
                else
                {
                    //Code is not available => RegisterPage
                    Debug.WriteLine("Property 'CodeValidationModel' is not available...", "Info");
                    NavigateToRegisterPageModel();
                }
        }

        private static void NavigateToRegisterPageModel()
        {
            //Localization Details
            ILanguageService languageService = FreshIOC.Container.Resolve<ILanguageService>();

            languageService.SetCurrentLanguage();
            Debug.WriteLine("Current Device Culture Info: " + CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName, "Info");

            var page = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>();
            var basicNavContainer = new FreshNavigationContainer(page);
            App.Current.MainPage = basicNavContainer;
        }

        private static async void NavigateToMainPageModel()
        {
            //Localization Details
            ILanguageService languageService = FreshIOC.Container.Resolve<ILanguageService>();

            languageService.SetCurrentLanguage();
            Debug.WriteLine("Current Device Culture Info: " + CrossMultilingual.Current.CurrentCultureInfo.TwoLetterISOLanguageName, "Info");

            //Set new Navigation Container
            MainMasterPageModel = new MainMasterPageModel();

            //questionService.LoadDataFromCache();
            /*if (LoadData)
            {
                IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
                questionService.LoadData();
            }*/


            MainMasterPageModel.AddCategorie(AppResources.AllCategories);
            MainMasterPageModel.AddPage(AppResources.Settings, new SettingsPageModel(), "ic_settings_black_18.png");
            MainMasterPageModel.AddPage(AppResources.Help, new HelpPageModel(), "ic_help_black_18.png");
            MainMasterPageModel.Init("Menu");

            Console.WriteLine("1234: Load Data from Cache");
            IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
            await questionService.LoadDataFromCache();

            Application.Current.MainPage = MainMasterPageModel;

            OpenNotification();
        }

        private static async void OpenNotification()
        {
            try
            {
                IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
                IFreshNavigationService navService = FreshIOC.Container.Resolve<IFreshNavigationService>(MainMasterPageModel.NavigationServiceName);

                SurveyModel WasQuestionOpened = await BlobCache.LocalMachine.GetObject<SurveyModel>("WasQuestionOpened");
                await BlobCache.LocalMachine.InvalidateObject<SurveyModel>("WasQuestionOpened");

                Console.WriteLine("1234: GoTo Question");
                Debug.WriteLine("WasQuestionOpened: " + WasQuestionOpened.SurveyDesc, "Info");

                questionService.AddQuestion(WasQuestionOpened);

                switch (WasQuestionOpened.TypeDesc)
                {
                    case SurveyType.YNQ:

                        YNQPage ynqPage = (YNQPage)FreshPageModelResolver.ResolvePageModel<YNQPageModel>();
                        YNQPageModel ynqPageModel = (YNQPageModel)ynqPage.BindingContext;
                        ynqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(ynqPage, ynqPageModel);
                        break;
                    case SurveyType.TLQ:
                        TLQPage tlqPage = (TLQPage)FreshPageModelResolver.ResolvePageModel<TLQPageModel>();
                        TLQPageModel tlqPageModel = (TLQPageModel)tlqPage.BindingContext;
                        tlqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(tlqPage, tlqPageModel);
                        break;
                    case SurveyType.OWQ:
                        OWQPage owqPage = (OWQPage)FreshPageModelResolver.ResolvePageModel<OWQPageModel>();
                        OWQPageModel owqPageModel = (OWQPageModel)owqPage.BindingContext;
                        owqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(owqPage, owqPageModel);

                        break;
                    case SurveyType.GAQ:
                        GAQPage gaqPage = (GAQPage)FreshPageModelResolver.ResolvePageModel<GAQPageModel>();
                        GAQPageModel gaqPageModel = (GAQPageModel)gaqPage.BindingContext;
                        gaqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(gaqPage, gaqPageModel);
                        break;
                }

                WasThereAlreadyANotification = true;

                try
                {
                    Debug.WriteLine("start of try");
                    List<SurveyModel> list = await BlobCache.LocalMachine.GetObject<List<SurveyModel>>("Questions");
                    list.Add(WasQuestionOpened);

                    await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", list);
                    Debug.WriteLine("end of try");
                }
                catch (KeyNotFoundException ex)
                {
                    Debug.WriteLine("in catch");
                    await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", new List<SurveyModel>(new SurveyModel[] { WasQuestionOpened }));
                }
            }
            catch
            {
                Console.WriteLine("1234: No Notification found...");
            }
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
            BlobCache.ApplicationName = "com.simpleQ.SimpleQ";
        }

        private void SetupOneSignal()
        {
            OneSignal.Current.StartInit("68b8996a-f664-4130-9854-9ed7f70d5540")
                .InFocusDisplaying(OSInFocusDisplayOption.Notification)
                .HandleNotificationOpened(HandleNotificationOpened)
                .EndInit();
        }

        // Called when a notification is opened.
        // The name of the method can be anything as long as the signature matches.
        // Method must be static or this object should be marked as DontDestroyOnLoad
        private async void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            Console.WriteLine("1234: HandleNotificationOpened!23");

            Dictionary<String, object> additionalData = result.notification.payload.additionalData;

            IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
            Debug.WriteLine(additionalData["StartDate"].ToString());

            foreach (object o in additionalData)
            {
                Debug.WriteLine(o);
            }


            SurveyModel newSurveryModel = new SurveyModel(int.Parse(additionalData["SvyId"].ToString()), additionalData["SvyDesc"].ToString(), additionalData["CatName"].ToString(), int.Parse(additionalData["TypeId"].ToString()), DateTime.Now, DateTime.Now);

            Debug.WriteLine("After try/catch");
            await BlobCache.LocalMachine.InsertObject<SurveyModel>("WasQuestionOpened", newSurveryModel);

            if (WasThereAlreadyANotification)
            {
                Console.WriteLine("1234: There was already a notification");
                OpenNotification();
                WasThereAlreadyANotification = false;
            }

            //questionService.AddQuestion(new SurveyModel(int.Parse(additionalData["SvyId"].ToString()), additionalData["SvyDesc"].ToString(), additionalData["CatName"].ToString(), int.Parse(additionalData["TypeId"].ToString()), DateTime.Now, DateTime.Now));
        }
    }
}
