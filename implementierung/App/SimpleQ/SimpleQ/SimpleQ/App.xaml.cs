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
using System.IO;
using SimpleQ.Shared;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
        private static Boolean WasThereAlreadyANotification = false;
        public static String Key = "";

        public App(OSNotificationOpenedResult result): this()
        {
            HandleNotificationOpened(result);
        }

        public App ()
		{
            Debug.WriteLine("NO: App started...", "Info");
            //Application.Current.Properties.Remove("IsValidCodeAvailable");
            //Application.Current.Properties["Language"] = "en";

            SetupIOC();

            SetupBlobCache();

            //GetKeyFromFile();



            InitializeComponent();

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                SetupOneSignal();
            }

            GoToRightPage();

        }

        public static void GoToRightPage()
        {
            if (Application.Current.Properties.ContainsKey("registrations"))
            {
                Debug.WriteLine("Code is valid now...", "Info");
                NavigateToMainPageModel();
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

            var page = FreshPageModelResolver.ResolvePageModel<RegisterPageModel>(true);
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

            IWebAPIService webAPIService = FreshIOC.Container.Resolve<IWebAPIService>();

            //RegistrationData data = await webAPIService.Register("1234", "1234");
            //Debug.WriteLine("RegistrationData: " + data);
            //webAPIService.Unregister("1234", "123");

            SurveyModel sm = await webAPIService.GetSurveyData(1);
            Debug.WriteLine("New Survey: " + sm.SurveyDesc);

            SurveyVote vote = new SurveyVote();
            vote.CustCode = "17ad34fbcf43bd6";
            vote.ChosenAnswerOptions = new List<AnswerOption>();
            vote.ChosenAnswerOptions.Add(sm.GivenAnswers[0]);

            Boolean b = await webAPIService.AnswerSurvey(vote);
            Debug.WriteLine("Erfolgreich?: " + b);

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
                    case SurveyType.YesNoQuestion:

                        YesNoQuestionPage ynqPage = (YesNoQuestionPage)FreshPageModelResolver.ResolvePageModel<YesNoQuestionPageModel>(true);
                        YesNoQuestionPageModel ynqPageModel = (YesNoQuestionPageModel)ynqPage.BindingContext;
                        ynqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(ynqPage, ynqPageModel);
                        break;
                    case SurveyType.YesNoDontKnowQuestion:
                        YesNoDontKnowQuestionPage yndkqPage = (YesNoDontKnowQuestionPage)FreshPageModelResolver.ResolvePageModel<YesNoDontKnowQuestionPageModel>(true);
                        YesNoQuestionPageModel yndkqPageModel = (YesNoQuestionPageModel)yndkqPage.BindingContext;
                        yndkqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(yndkqPage, yndkqPageModel);
                        break;
                    case SurveyType.TrafficLightQuestion:
                        TrafficLightQuestionPage tlqPage = (TrafficLightQuestionPage)FreshPageModelResolver.ResolvePageModel<TrafficLightQuestionPageModel>(true);
                        TrafficLightQuestionPageModel tlqPageModel = (TrafficLightQuestionPageModel)tlqPage.BindingContext;
                        tlqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(tlqPage, tlqPageModel);
                        break;
                    case SurveyType.OpenQuestion:
                        OpenQuestionPage owqPage = (OpenQuestionPage)FreshPageModelResolver.ResolvePageModel<OpenQuestionPageModel>(true);
                        OpenQuestionPageModel owqPageModel = (OpenQuestionPageModel)owqPage.BindingContext;
                        owqPageModel.Question = WasQuestionOpened;
                        navService.PushPage(owqPage, owqPageModel);

                        break;
                    case SurveyType.PolytomousUSQuestion:
                        PolytomousUSQuestionPage polytomousUSPage = (PolytomousUSQuestionPage)FreshPageModelResolver.ResolvePageModel<PolytomousUSQuestionPageModel>(true);
                        PolytomousUSQuestionPageModel polytomousUSPageModel = (PolytomousUSQuestionPageModel)polytomousUSPage.BindingContext;
                        polytomousUSPageModel.Question = WasQuestionOpened;
                        navService.PushPage(polytomousUSPage, polytomousUSPageModel);
                        break;
                    case SurveyType.DichotomousQuestion:
                        DichotomousQuestionPage dichotomousQuestionPage = (DichotomousQuestionPage)FreshPageModelResolver.ResolvePageModel<DichotomousQuestionPageModel>(true);
                        DichotomousQuestionPageModel dichotomousQuestionPageModel = (DichotomousQuestionPageModel)dichotomousQuestionPage.BindingContext;
                        dichotomousQuestionPageModel.Question = WasQuestionOpened;
                        navService.PushPage(dichotomousQuestionPage, dichotomousQuestionPageModel);
                        break;
                    case SurveyType.PolytomousOMQuestion:
                        PolytomousOMQuestionPage polytomousOMQuestionPage = (PolytomousOMQuestionPage)FreshPageModelResolver.ResolvePageModel<DichotomousQuestionPageModel>(true);
                        DichotomousQuestionPageModel polytomousOMQuestionPageModel = (DichotomousQuestionPageModel)polytomousOMQuestionPage.BindingContext;
                        polytomousOMQuestionPageModel.Question = WasQuestionOpened;
                        navService.PushPage(polytomousOMQuestionPage, polytomousOMQuestionPageModel);
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
            FreshIOC.Container.Register<ISettingsService, SettingsService>();
            FreshIOC.Container.Register<IWebAPIService, WebAPIService>();
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

            OneSignal.Current.IdsAvailable(IdsAvailable);
        }

        private async void IdsAvailable(string userID, string pushToken)
        {
            Debug.WriteLine("Ids of OneSignal available...", "Info");
            Application.Current.Properties["userID"] = userID;
            await Application.Current.SavePropertiesAsync();
        }

        private void GetKeyFromFile()
        {
            var assembly = IntrospectionExtensions.GetTypeInfo(typeof(WebAPIService)).Assembly;
            Stream stream = assembly.GetManifestResourceStream("SimpleQ.Resources.private.key");

            StreamReader streamReader = new StreamReader(stream);

            Key = streamReader.ReadToEnd();
        }

        // Called when a notification is opened.
        // The name of the method can be anything as long as the signature matches.
        // Method must be static or this object should be marked as DontDestroyOnLoad
        private static async void HandleNotificationOpened(OSNotificationOpenedResult result)
        {
            Debug.WriteLine("NO: Notification Opened in App.xaml.cs ...", "Info");
            Dictionary<String, object> additionalData = result.notification.payload.additionalData;
            Debug.WriteLine(additionalData["svyId"]); 

            /*Console.WriteLine("1234: HandleNotificationOpened!23");

            Dictionary<String, object> additionalData = result.notification.payload.additionalData;

            IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
            Debug.WriteLine(additionalData["StartDate"].ToString());

            foreach (object o in additionalData)
            {
                Debug.WriteLine(o);
            }


            SurveyModel newSurveryModel = new SurveyModel(int.Parse(additionalData["SvyId"].ToString()), additionalData["SvyDesc"].ToString(), additionalData["CatName"].ToString(), int.Parse(additionalData["TypeId"].ToString()), DateTime.Now, new List<Shared.AnswerOption>());

            Debug.WriteLine("After try/catch");
            await BlobCache.LocalMachine.InsertObject<SurveyModel>("WasQuestionOpened", newSurveryModel);

            if (WasThereAlreadyANotification)
            {
                Console.WriteLine("1234: There was already a notification");
                OpenNotification();
                WasThereAlreadyANotification = false;
            }

            //questionService.AddQuestion(new SurveyModel(int.Parse(additionalData["SvyId"].ToString()), additionalData["SvyDesc"].ToString(), additionalData["CatName"].ToString(), int.Parse(additionalData["TypeId"].ToString()), DateTime.Now, DateTime.Now));
        */
        }
    }
}
