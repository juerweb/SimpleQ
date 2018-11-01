using Acr.UserDialogs;
using Akavache;
using Com.OneSignal;
using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the LoadingPageModel for the Page xy.
    /// </summary>
    public class LoadingPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public LoadingPageModel(ISimulationService simulationService, IDialogService dialogService, IQuestionService questionService, IWebAPIService webAPIService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
            this.questionService = questionService;
            this.webAPIService = webAPIService;
        }
        public LoadingPageModel()
        {
            IsFirstStepTicked = false;
            IsSecondStepTicked = false;
        }

        public override async void Init(object initData)
        {
            base.Init(initData);
            Debug.WriteLine("LoadingPageModel initalised with InitData: " + initData, "Info");

            this.IsRunning = true;

            //Code Check
            int code = (int)initData;
            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                Debug.WriteLine("RuntimePlatform is in (iOS, Android)...", "Info");
                if (Application.Current.Properties["userID"] != null)
                {
                    RegistrationData data = null;
                    try
                    {
                        data = await this.webAPIService.Register("m4rku51", Application.Current.Properties["userID"].ToString());
                    }
                    catch (System.Net.Http.HttpRequestException e)
                    {
                        Application.Current.Properties["IsValidCodeAvailable"] = false;
                        Debug.WriteLine("WebException during the Validation", "Error");
                        this.IsRunning = false;

                        this.dialogService.ShowErrorDialog(202);
                        await CoreMethods.PopToRoot(false);
                        return;
                    }
                    if (data != null)
                    {
                        Application.Current.Properties["CompanyName"] = "";
                        Application.Current.Properties["DepartmentName"] = data.DepName;
                        Application.Current.Properties["PersId"] = data.PersId;
                        Application.Current.Properties["CustCode"] = data.CustCode;
                        Application.Current.Properties["RegisterCode"] = code;
                        Application.Current.Properties["IsValidCodeAvailable"] = true;
                        Debug.WriteLine("Code is valid...", "Info");
                        this.IsFirstStepTicked = true;
                    }
                    else
                    {
                        Application.Current.Properties["IsValidCodeAvailable"] = false;
                        Debug.WriteLine("Code is not valid...", "Info");
                        this.IsRunning = false;

                        this.dialogService.ShowErrorDialog(201);
                        await CoreMethods.PopToRoot(false);
                        return;
                    }
                }
                
            }
            else
            {
                //Code Check
                CodeValidationModel codeValidationModel = await this.SimulationService.CheckCode((int)initData);

                Application.Current.Properties["IsValidCodeAvailable"] = codeValidationModel.IsValid;
                Application.Current.Properties["CompanyName"] = codeValidationModel.CompanyName;

                Application.Current.Properties["DepartmentName"] = codeValidationModel.DepartmentName;
                Application.Current.Properties["RegisterCode"] = codeValidationModel.Code;

                if (codeValidationModel.IsValid)
                {
                    Debug.WriteLine("Code is valid...", "Info");
                    this.IsFirstStepTicked = true;
                }
                else
                {
                    Debug.WriteLine("Code is not valid...", "Info");
                    this.IsRunning = false;

                    this.dialogService.ShowErrorDialog(201);
                    await CoreMethods.PopToRoot(false);
                    return;
                }
            }
            //Debug.WriteLine("ID: " + Application.Current.Properties["userID"]);

            //Load Data
            await questionService.LoadData();
            Debug.WriteLine("Requested Data...", "Info");

            //Set MainPageModel as new Main Page
            App.GoToRightPage();
        }
        #endregion

        #region Fields
        private Boolean isFirstStepTicked;
        private Boolean isSecondStepTicked;
        private Boolean isRunning;
        private ISimulationService simulationService;
        private IDialogService dialogService;
        private IQuestionService questionService;
        private IWebAPIService webAPIService;
        #endregion

        #region Properties + Getter/Setter Methods
        public bool IsFirstStepTicked
        {
            get => isFirstStepTicked;
            set
            {
                isFirstStepTicked = value;
                OnPropertyChanged();
            }
        }

        public bool IsSecondStepTicked
        {
            get => isSecondStepTicked;
            set
            {
                isSecondStepTicked = value;
                OnPropertyChanged();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }

        public ISimulationService SimulationService { get => simulationService; }
        public IDialogService DialogService { get => dialogService; }
        public IWebAPIService WebAPIService { get => webAPIService; set => webAPIService = value; }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// Ticks the step.
        /// </summary>
        /// <param name="stepNr">The step nr. from 1 to 3</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        private void TickStep(int stepNr, Boolean status)
        {
            switch (stepNr)
            {
                case 1:
                    IsFirstStepTicked = status;
                    break;
                case 2:
                    IsSecondStepTicked = status;
                    break;
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
