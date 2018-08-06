using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the LoadingPageModel for the Page xy.
    /// </summary>
    public class LoadingPageModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public LoadingPageModel()
        {
            IsFirstStepTicked = false;
            IsSecondStepTicked = false;
            IsThirdStepTicked = false;
        }
        #endregion

        #region Fields
        private Boolean isFirstStepTicked;
        private Boolean isSecondStepTicked;
        private Boolean isThirdStepTicked;
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
