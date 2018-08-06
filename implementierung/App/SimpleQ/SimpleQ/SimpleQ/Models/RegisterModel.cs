using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the RegisterModel for the ModelPage
    /// </summary>
    public class RegisterModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="RegisterModel"/> class.
        /// </summary>
        public RegisterModel()
        {
            this.ImageSource = Resources.AppResources.LogoResourcename;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The register code, which should the app scan from the qr-code
        /// </summary>
        private int? registerCode;
        /// <summary>
        /// The image source, for the logo
        /// </summary>
        private String imageSource;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the register code.
        /// </summary>
        /// <value>
        /// The register code.
        /// </value>
        public int? RegisterCode
        {
            get
            {
                return registerCode;
            }
            set
            {
                registerCode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the image source.
        /// </summary>
        /// <value>
        /// The image source.
        /// </value>
        public String ImageSource
        {
            get
            {
                return imageSource;
            }
            set
            {
                imageSource = value;
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
