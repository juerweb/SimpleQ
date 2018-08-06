using SimpleQ.Extensions;
using SimpleQ.Models;
using SimpleQ.PageModels.Commands;
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
            ScanCommand = new ScanQRCodeCommand();
            ManualCommand = new ManualCodeCommand();
            this.Behavior = new SixDigitCodeBehavior();
        }
        #endregion

        #region Fields
        /// <summary>
        /// The model field variable
        /// </summary>
        private RegisterModel model;

        /// <summary>
        /// The behavior
        /// </summary>
        private SixDigitCodeBehavior behavior;
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

        /// <summary>
        /// Gets or sets the behavior.
        /// </summary>
        /// <value>
        /// The behavior.
        /// </value>
        public SixDigitCodeBehavior Behavior { get => behavior; set => behavior = value; }
        #endregion

        #region Commands
        public ScanQRCodeCommand ScanCommand
        {
            get;
        }

        public ManualCodeCommand ManualCommand
        {
            get;
        }
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
