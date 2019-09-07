using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    public class RegistrationDataModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public RegistrationDataModel()
        {
        }
        #endregion

        #region Fields
        private bool isRegister;
        private RegistrationData registrationData;
        #endregion

        #region Properties + Getter/Setter Methods
        public bool IsRegister { get => isRegister; set => isRegister = value; }
        public RegistrationData RegistrationData { get => registrationData; set => registrationData = value; }
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
