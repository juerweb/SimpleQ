using Akavache;
using FreshMvvm;
using Newtonsoft.Json;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;
using System.Reactive.Linq;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the UnregisterPageModel for the UnregisterPage.
    /// </summary>
    public class UnregisterPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisterPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public UnregisterPageModel(ISimulationService simulationService, IDialogService dialogService, IWebAPIService webAPIService, IQuestionService questionService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
            this.webAPIService = webAPIService;
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisterPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public UnregisterPageModel()
        {
            UnregisterCommand = new Command(UnregisterCommandExecuted);
            List<RegistrationDataModel> registrations = JsonConvert.DeserializeObject<List<RegistrationDataModel>>(Application.Current.Properties["registrations"].ToString());
            isChecked = new ObservableCollection<IsCheckedModel<RegistrationDataModel>>();
            foreach (RegistrationDataModel data in registrations)
            {
                isChecked.Add(new IsCheckedModel<RegistrationDataModel>(data));
            }
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private ObservableCollection<IsCheckedModel<RegistrationDataModel>> isChecked;
        private ISimulationService simulationService;
        private IDialogService dialogService;
        private IWebAPIService webAPIService;
        private IQuestionService questionService;
        private Boolean isOneChecked;

        #endregion

        #region Properties + Getter/Setter Methods

        public ObservableCollection<IsCheckedModel<RegistrationDataModel>> IsChecked { get => isChecked; set => isChecked = value; }

        public bool IsOneChecked
        {
            get => isOneChecked;
            set
            {
                isOneChecked = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public Command UnregisterCommand
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        private async void UnregisterCommandExecuted()
        {
            Debug.WriteLine("Unregister Command Executed...", "Info");
            if (await dialogService.ShowReallySureDialog())
            {
                ObservableCollection<IsCheckedModel<RegistrationDataModel>> saveCollection = new ObservableCollection<IsCheckedModel<RegistrationDataModel>>(IsChecked);
                foreach (IsCheckedModel<RegistrationDataModel> isCheckedModel in saveCollection)
                {
                    if (isCheckedModel.IsChecked)
                    {
                        try
                        {
                            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                            {
                                //Android or iOS App
                                Debug.WriteLine("Unregister Command executed on iOS or Android...", "Info");
                                Boolean success;

                                success = await this.webAPIService.LeaveDepartment(isCheckedModel.AnswerOption.RegistrationData.PersId, isCheckedModel.AnswerOption.RegistrationData.DepId, isCheckedModel.AnswerOption.RegistrationData.CustCode);

                                if (success)
                                {
                                    IsChecked.Remove(isCheckedModel);
                                    if (IsChecked.Count == 0)
                                    {

                                        success = await this.webAPIService.Unregister(isCheckedModel.AnswerOption.RegistrationData.PersId.ToString(), isCheckedModel.AnswerOption.RegistrationData.CustCode);

                                        if (success)
                                        {
                                            Application.Current.Properties.Remove("registrations");
                                            try
                                            {
                                                await BlobCache.LocalMachine.InvalidateObject<List<SurveyModel>>("Questions");
                                                await BlobCache.LocalMachine.Flush();
                                                this.questionService.RemoveQuestions();
                                            }
                                            catch (Exception e)
                                            {
                                                Debug.WriteLine("Exception was occured: " + e.InnerException, "Exception");
                                            }
                                        }
                                        else
                                        {
                                            Debug.WriteLine("Problem during the Unregister", "Error");
                                            this.dialogService.ShowErrorDialog(203);
                                        }

                                    }
                                    else
                                    {
                                        List<RegistrationDataModel> tmpList = new List<RegistrationDataModel>();
                                        foreach (IsCheckedModel<RegistrationDataModel> tmp in IsChecked)
                                        {
                                            tmpList.Add(tmp.AnswerOption);
                                        }
                                        Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(tmpList);
                                    }
                                }
                                else
                                {
                                    Debug.WriteLine("Problem during the Unregister", "Error");
                                    this.dialogService.ShowErrorDialog(203);
                                }
                            }
                            else
                            {
                                //UWP App
                                Debug.WriteLine("Unregister Command executed on UWP", "Info");
                                IsChecked.Remove(isCheckedModel);
                                if (IsChecked.Count == 0)
                                {
                                    Application.Current.Properties.Remove("registrations");
                                }
                                else
                                {
                                    List<RegistrationDataModel> tmpList = new List<RegistrationDataModel>();
                                    foreach (IsCheckedModel<RegistrationDataModel> tmp in IsChecked)
                                    {
                                        tmpList.Add(tmp.AnswerOption);
                                    }
                                    Application.Current.Properties["registrations"] = JsonConvert.SerializeObject(tmpList);
                                }
                            }
                        }
                        catch (System.Net.Http.HttpRequestException e)
                        {
                            Debug.WriteLine("WebException during the Unregister", "Error");
                            this.dialogService.ShowErrorDialog(202);
                            return;
                        }
                        await Application.Current.SavePropertiesAsync();
                    }
                }
                Debug.WriteLine("Go to right Page in UnregisterPageModel...", "Info");
                App.GoToRightPage();
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
