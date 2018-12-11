using Acr.UserDialogs;
using Akavache;
using Com.OneSignal;
using FreshMvvm;
using Newtonsoft.Json;
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
using System.Linq;

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

            //isRegister and not joinDepartment
            Boolean isRegister;

            //Code Check
            List<object> objects = (List<object>)initData;
            string code = objects[0].ToString();

            Debug.WriteLine("DebugMode: " + (Boolean)objects[1]);

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                Debug.WriteLine("RuntimePlatform is in (iOS, Android)...", "Info");
                if (Application.Current.Properties["userID"] != null)
                {
                    RegistrationDataModel data = new RegistrationDataModel();
                    try
                    {
                        if (!Application.Current.Properties.ContainsKey("registrations"))
                        {
                            Debug.WriteLine("New Registration...", "Info");
                            data.RegistrationData = await this.webAPIService.Register(code, Application.Current.Properties["userID"].ToString());
                            data.IsRegister = true;
                        }
                        else
                        {
                            Debug.WriteLine("New Join Department...", "Info");
                            List<RegistrationDataModel> tmp = JsonConvert.DeserializeObject<List<RegistrationDataModel>>(Application.Current.Properties["registrations"].ToString());
                            if (tmp.Count(registration => registration.RegistrationData.CustCode + registration.RegistrationData.DepId == code) <= 0)
                            {
                                data.RegistrationData = await this.webAPIService.JoinDepartment(code, tmp[0].RegistrationData.PersId);
                                data.IsRegister = false;
                            }
                            else
                            {
                                Debug.WriteLine("Code is not valid...", "Info");
                                this.IsRunning = false;

                                this.dialogService.ShowErrorDialog(205);
                                await CoreMethods.PopToRoot(false);
                                return;
                            }
                        }
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
                    if (data.RegistrationData != null)
                    {
                        if (Application.Current.Properties.ContainsKey("registrations"))
                        {
                            List<RegistrationDataModel> tmp = JsonConvert.DeserializeObject<List<RegistrationDataModel>>(Application.Current.Properties["registrations"].ToString());
                            tmp.Add(data);
                            Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(tmp);
                        }
                        else
                        {
                            Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(new List <RegistrationDataModel> { data });
                        }
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
                
            }
            else
            {
                //Code Check
                CodeValidationModel codeValidationModel = await this.SimulationService.CheckCode(code);

                if (codeValidationModel.IsValid)
                {
                    Debug.WriteLine("Code is valid...", "Info");
                    this.IsFirstStepTicked = true;

                    RegistrationDataModel data = new RegistrationDataModel() { RegistrationData=new RegistrationData() { CustCode = "1", DepId = 1, DepName = "Development", PersId = 1, CustName="SimpleQ Company" } };
                    //Code Check
                    if (Application.Current.Properties.ContainsKey("registrations"))
                    {
                        List<RegistrationDataModel> tmp = JsonConvert.DeserializeObject<List<RegistrationDataModel>>(Application.Current.Properties["registrations"].ToString());
                        tmp.Add(data);
                        Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(tmp);
                    }
                    else
                    {
                        Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(new List<RegistrationDataModel> { data });
                    }
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

            await Application.Current.SavePropertiesAsync();

            //Debug.WriteLine("ID: " + Application.Current.Properties["userID"]);

            //Load Data
            if ((Boolean)objects[1])
            {
                if (objects.Count > 2 && (Boolean)objects[2])
                {
                    await questionService.LoadData();
                    Debug.WriteLine("Requested Data in Debug Mode...", "Info");
                }
                else
                {
                    Debug.WriteLine("No Data Request because not in Debug Mode...", "Info");
                }
            }

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
