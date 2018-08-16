using FreshMvvm;
using SimpleQ.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the AboutPageModel for the Page xy.
    /// </summary>
    public class AboutPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public AboutPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AboutPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public AboutPageModel()
        {

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
        #endregion

        #region Properties + Getter/Setter Methods
        #endregion

        #region Commands
        #endregion

        #region Methods
        public string VersionNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber(); }
        public string BuildNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber(); }
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
