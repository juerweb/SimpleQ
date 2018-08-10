using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
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
        public LoadingPageModel(ISimulationService simulationService, IDialogService dialogService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
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

                this.dialogService.ShowDialog(DialogType.Error, 201);
                await CoreMethods.PopPageModel();
                return;
            }

            //Load Data
            Boolean success = await this.SimulationService.GetData();
            if (success)
            {
                Debug.WriteLine("Data successfully loaded", "Info");
            }
            else
            {
                Debug.WriteLine("Data not successfully loaded", "Info");
            }

            //Set MainPageModel as new Main Page
            App.NavigateToMainPageModel();
        }
        #endregion

        #region Fields
        private Boolean isFirstStepTicked;
        private Boolean isSecondStepTicked;
        private Boolean isRunning;
        private ISimulationService simulationService;
        private IDialogService dialogService;
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
