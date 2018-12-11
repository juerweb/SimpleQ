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
using System.Net.Http;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
        private static Boolean WasThereAlreadyANotification = false;
        public static String Key = "";
        private SurveyModel currentQuestion;

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

            SetDefaultProperties();

            //GetKeyFromFile();
            InitializeComponent();


            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                SetupOneSignal();
            }

            GoToRightPage();
            Debug.WriteLine(Xamarin.Forms.Font.Default);

            IFaqService faqService = FreshIOC.Container.Resolve<IFaqService>();
            faqService.LoadDataFromCache();
        }

        private async void SetDefaultProperties()
        {
            try
            {
                Boolean closeAppAfterNotification = await BlobCache.UserAccount.GetObject<Boolean>("CloseAppAfterNotification");
                Debug.WriteLine("CloseAppAfterNotification is set to " + closeAppAfterNotification, "Info");
            }
            catch (KeyNotFoundException e)
            {
                Debug.WriteLine("CloseAppAfterNotification is not set... ", "Info");
                BlobCache.UserAccount.InsertObject<Boolean>("CloseAppAfterNotification", true);
            }

            try
            {
                Boolean showMessageAfterAnswering = await BlobCache.UserAccount.GetObject<Boolean>("ShowMessageAfterAnswering");
                Debug.WriteLine("CloseAppAfterNotification is set to " + showMessageAfterAnswering, "Info");
            }
            catch (KeyNotFoundException e)
            {
                Debug.WriteLine("ShowMessageAfterAnswering is not set... ", "Info");
                BlobCache.UserAccount.InsertObject<Boolean>("ShowMessageAfterAnswering", true);
            }
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

            /*SurveyModel sm = await webAPIService.GetSurveyData(1);
            Debug.WriteLine("New Survey: " + sm.SurveyDesc);

            SurveyVote vote = new SurveyVote();
            vote.CustCode = "17ad34fbcf43bd6";
            vote.ChosenAnswerOptions = new List<AnswerOption>();
            vote.ChosenAnswerOptions.Add(sm.GivenAnswers[0]);

            Boolean b = await webAPIService.AnswerSurvey(vote);
            Debug.WriteLine("Erfolgreich?: " + b);*/
        }

        public static async void OpenQuestionPage(SurveyModel surveyModel)
        {
            IFreshNavigationService navService = FreshIOC.Container.Resolve<IFreshNavigationService>(MainMasterPageModel.NavigationServiceName);

            switch (surveyModel.TypeDesc)
            {
                case SurveyType.YesNoQuestion:
                    YesNoQuestionPage ynqPage = (YesNoQuestionPage)FreshPageModelResolver.ResolvePageModel<YesNoQuestionPageModel>(new List<object> { surveyModel, true });
                    YesNoQuestionPageModel ynqPageModel = (YesNoQuestionPageModel)ynqPage.BindingContext;
                    //ynqPageModel.Question = surveyModel;
                    navService.PushPage(ynqPage, ynqPageModel);
                    return;
                case SurveyType.YesNoDontKnowQuestion:
                    YesNoDontKnowQuestionPage yndkqPage = (YesNoDontKnowQuestionPage)FreshPageModelResolver.ResolvePageModel<YesNoDontKnowQuestionPageModel>(new List<object> { surveyModel, true });
                    YesNoDontKnowQuestionPageModel yndkqPageModel = (YesNoDontKnowQuestionPageModel)yndkqPage.BindingContext;
                    //yndkqPageModel.Question = surveyModel;
                    navService.PushPage(yndkqPage, yndkqPageModel);
                    return;
                case SurveyType.TrafficLightQuestion:
                    TrafficLightQuestionPage tlqPage = (TrafficLightQuestionPage)FreshPageModelResolver.ResolvePageModel<TrafficLightQuestionPageModel>(new List<object> { surveyModel, true });
                    TrafficLightQuestionPageModel tlqPageModel = (TrafficLightQuestionPageModel)tlqPage.BindingContext;
                    //tlqPageModel.Question = surveyModel;
                    navService.PushPage(tlqPage, tlqPageModel);
                    return;
                case SurveyType.OpenQuestion:
                    OpenQuestionPage owqPage = (OpenQuestionPage)FreshPageModelResolver.ResolvePageModel<OpenQuestionPageModel>(new List<object> { surveyModel, true });
                    OpenQuestionPageModel owqPageModel = (OpenQuestionPageModel)owqPage.BindingContext;
                    //owqPageModel.Question = surveyModel;
                    navService.PushPage(owqPage, owqPageModel);
                    return;
                case SurveyType.PolytomousUSQuestion:
                    PolytomousUSQuestionPage polytomousUSPage = (PolytomousUSQuestionPage)FreshPageModelResolver.ResolvePageModel<PolytomousUSQuestionPageModel>(new List<object> { surveyModel, true });
                    PolytomousUSQuestionPageModel polytomousUSPageModel = (PolytomousUSQuestionPageModel)polytomousUSPage.BindingContext;
                    //polytomousUSPageModel.Question = surveyModel;
                    navService.PushPage(polytomousUSPage, polytomousUSPageModel);
                    return;
                case SurveyType.PolytomousOSQuestion:
                    PolytomousOSQuestionPage polytomousOSPage = (PolytomousOSQuestionPage)FreshPageModelResolver.ResolvePageModel<PolytomousOSQuestionPageModel>(new List<object> { surveyModel, true });
                    PolytomousUSQuestionPageModel polytomousOSPageModel = (PolytomousUSQuestionPageModel)polytomousOSPage.BindingContext;
                    //polytomousOSPageModel.Question = surveyModel;
                    navService.PushPage(polytomousOSPage, polytomousOSPageModel);
                    return;
                case SurveyType.DichotomousQuestion:
                    DichotomousQuestionPage dichotomousQuestionPage = (DichotomousQuestionPage)FreshPageModelResolver.ResolvePageModel<DichotomousQuestionPageModel>(new List<object> { surveyModel, true });
                    DichotomousQuestionPageModel dichotomousQuestionPageModel = (DichotomousQuestionPageModel)dichotomousQuestionPage.BindingContext;
                    //dichotomousQuestionPageModel.Question = surveyModel;
                    navService.PushPage(dichotomousQuestionPage, dichotomousQuestionPageModel);
                    return;
                case SurveyType.PolytomousOMQuestion:
                    PolytomousOMQuestionPage polytomousOMQuestionPage = (PolytomousOMQuestionPage)FreshPageModelResolver.ResolvePageModel<PolytomousOMQuestionPageModel>(new List<object> { surveyModel, true });
                    PolytomousOMQuestionPageModel polytomousOMQuestionPageModel = (PolytomousOMQuestionPageModel)polytomousOMQuestionPage.BindingContext;
                    //polytomousOMQuestionPageModel.Question = surveyModel;
                    navService.PushPage(polytomousOMQuestionPage, polytomousOMQuestionPageModel);
                    return;
                case SurveyType.PolytomousUMQuestion:
                    PolytomousUMQuestionPage polytomousUMQuestionPage = (PolytomousUMQuestionPage)FreshPageModelResolver.ResolvePageModel<PolytomousUMQuestionPageModel>(new List<object> { surveyModel, true });
                    PolytomousUMQuestionPageModel polytomousUMQuestionPageModel = (PolytomousUMQuestionPageModel)polytomousUMQuestionPage.BindingContext;
                    //polytomousUMQuestionPageModel.Question = surveyModel;
                    navService.PushPage(polytomousUMQuestionPage, polytomousUMQuestionPageModel);
                    return;
            }

            List<SurveyType> types = new List<SurveyType>(new SurveyType[]{ SurveyType.LikertScale3Question, SurveyType.LikertScale4Question, SurveyType.LikertScale5Question , SurveyType.LikertScale6Question, SurveyType.LikertScale7Question, SurveyType.LikertScale8Question, SurveyType.LikertScale9Question });
            if (types.Contains(surveyModel.TypeDesc))
            {
                LikertScaleQuestionPage likertScaleQuestionPage = (LikertScaleQuestionPage)FreshPageModelResolver.ResolvePageModel<LikertScaleQuestionPageModel>(new List<object> { surveyModel, true });
                LikertScaleQuestionPageModel likertScaleQuestionPageModel = (LikertScaleQuestionPageModel)likertScaleQuestionPage.BindingContext;
                //likertScaleQuestionPageModel.Question = surveyModel;
                navService.PushPage(likertScaleQuestionPage, likertScaleQuestionPageModel);
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
            FreshIOC.Container.Register<IToastService, ToastService>();
            FreshIOC.Container.Register<IFaqService, FaqService>();
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
                .HandleNotificationReceived(HandleNotificationReceived)
                .EndInit();

            OneSignal.Current.IdsAvailable(IdsAvailable);
        }

        private void HandleNotificationReceived(OSNotification notification)
        {
            Debug.WriteLine("NO: Notification Received...", "Info");
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
            Debug.WriteLine(additionalData["SvyId"]);

            IWebAPIService webAPIService = FreshIOC.Container.Resolve<IWebAPIService>();
            Debug.WriteLine(int.Parse(additionalData["SvyId"].ToString()));
            try
            {
                IQuestionService questionService = FreshIOC.Container.Resolve<IQuestionService>();
                if (additionalData.ContainsKey("Cancel") && Convert.ToBoolean(additionalData["Cancel"].ToString()))
                {
                    //Cancel Survey
                    Debug.WriteLine("Cancel Survey with id: " + additionalData["SvyId"].ToString(), "Info");
                    questionService.RemoveQuestion(int.Parse(additionalData["SvyId"].ToString()));
                }
                else
                {
                    IFreshNavigationService navService = FreshIOC.Container.Resolve<IFreshNavigationService>(MainMasterPageModel.NavigationServiceName);
                    LoadingQuestionPage page = (LoadingQuestionPage)FreshPageModelResolver.ResolvePageModel<LoadingQuestionPageModel>(int.Parse(additionalData["SvyId"].ToString()));
                    LoadingQuestionPageModel pageModel = (LoadingQuestionPageModel)page.BindingContext;
                    navService.PushPage(page, pageModel);
                }

            }
            catch (HttpRequestException e)
            {
                Debug.WriteLine("WebException during the GetSurveyData", "Error");
                IDialogService dialogService = FreshIOC.Container.Resolve<IDialogService>();

                dialogService.ShowErrorDialog(202);
            }



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
