using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the RegisterPageModel for the RegisterPage.
    /// </summary>
    public class RegisterPageModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterPageModel"/> class.
        /// </summary>
        public RegisterPageModel()
        {
            this.Model = new RegisterModel();
        }
        #endregion

        #region Fields
        /// <summary>
        /// The model field variable
        /// </summary>
        private RegisterModel model;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the model.
        /// </summary>
        /// <value>
        /// The model.
        /// </value>
        public RegisterModel Model
        {
            get
            {
                return model;
            }
            set
            {
                model = value;
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
