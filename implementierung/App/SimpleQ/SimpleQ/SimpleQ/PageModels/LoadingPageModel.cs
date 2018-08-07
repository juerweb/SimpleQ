using Acr.UserDialogs;
using FreshMvvm;
using SimpleQ.PageModels.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the LoadingPageModel for the Page xy.
    /// </summary>
    public class LoadingPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public LoadingPageModel(ISimulationService simulationService, IUserDialogs dialogService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
        }
        public LoadingPageModel()
        {
            IsFirstStepTicked = false;
            IsSecondStepTicked = false;
            IsThirdStepTicked = false;
        }

        public override async void Init(object initData)
        {
            base.Init(initData);

            this.dialogService.ShowLoading();

            Debug.WriteLine("LoadingPageModel initalised with InitData: " + initData, "Info");

            Boolean isValid = await this.SimulationService.CheckCode((int)initData);

            this.dialogService.HideLoading();

            if (isValid)
            {
                this.dialogService.Alert("Gültig");
            }
            else
            {
                this.dialogService.Alert("Ungültig");
            }

        }


        #endregion

        #region Fields
        private Boolean isFirstStepTicked;
        private Boolean isSecondStepTicked;
        private Boolean isThirdStepTicked;
        private ISimulationService simulationService;
        private IUserDialogs dialogService;
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

        public bool IsThirdStepTicked
        {
            get => isThirdStepTicked;
            set
            {
                isThirdStepTicked = value;
                OnPropertyChanged();
            }
        }

        public ISimulationService SimulationService { get => simulationService; }
        public IUserDialogs DialogService { get => dialogService; }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// Ticks the step.
        /// </summary>
        /// <param name="stepNr">The step nr. from 1 to 3</param>
        /// <param name="status">if set to <c>true</c> [status].</param>
        public void TickStep(int stepNr, Boolean status)
        {
            switch (stepNr)
            {
                case 1:
                    IsFirstStepTicked = status;
                    break;
                case 2:
                    IsSecondStepTicked = status;
                    break;
                case 3:
                    IsThirdStepTicked = status;
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
