using SimpleQ.Webinterface.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the FAQModel for the FAQPage
    /// </summary>
    public class FAQModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="FAQModel"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="answer">The ansDesc.</param>
        public FAQModel(FaqEntry entry): this()
        {
            this.entry = entry;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAQModel"/> class.
        /// </summary>
        public FAQModel()
        {
            this.IsActive = (Device.RuntimePlatform == Device.iOS);
        }
        #endregion

        #region Fields
        private FaqEntry entry;
        /// <summary>
        /// Field, whichs shows the status of the faq.
        /// </summary>
        private bool isActive;

        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get => isActive;
            set
            {
                if (value)
                {
                    isActive = value;
                    OnPropertyChanged();
                }
                else if (Device.RuntimePlatform != Device.iOS)
                {
                    isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public FaqEntry Entry
        {
            get => entry;
            set
            {
                entry = value;
                OnPropertyChanged();
            }
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
