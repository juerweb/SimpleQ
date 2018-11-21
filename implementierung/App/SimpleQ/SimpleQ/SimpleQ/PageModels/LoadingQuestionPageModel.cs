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

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the LoadingQuestionPageModel for the Page xy.
    /// </summary>
    public class LoadingQuestionPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public LoadingQuestionPageModel(ISimulationService simulationService, IDialogService dialogService, IQuestionService questionService, IWebAPIService webAPIService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
            this.questionService = questionService;
            this.webAPIService = webAPIService;
        }
        public LoadingQuestionPageModel()
        {
        }

        public override async void Init(object initData)
        {
            base.Init(initData);
            Debug.WriteLine("LoadingPageModel initalised with InitData: " + initData, "Info");

            this.IsRunning = true;
        }
        #endregion

        #region Fields
        private Boolean isRunning;
        private ISimulationService simulationService;
        private IDialogService dialogService;
        private IQuestionService questionService;
        private IWebAPIService webAPIService;
        #endregion

        #region Properties + Getter/Setter Methods

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
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
