using FreshMvvm;
using SimpleQ.Extensions;
using SimpleQ.Resources;
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
            VisitUsCommand = new Command(VisitUs);
            EmailUsCommand = new Command(EmailUs);
            PhoneUsCommand = new Command(PhoneUs);
            ContactUsCommand = new Command(ContactUs);
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
        public string VersionNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber(); }
        public string BuildNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber(); }
        public string RuntimePlatform { get => Device.RuntimePlatform; }
        #endregion

        #region Commands
        public Command VisitUsCommand { get; private set; }
        public Command EmailUsCommand { get; private set; }
        public Command PhoneUsCommand { get; private set; }
        public Command ContactUsCommand { get; private set; }
        #endregion

        #region Methods
        private void VisitUs(object obj)
        {
            Device.OpenUri(new Uri(AppResources.LinkToWebsite));
        }

        private void EmailUs(object obj)
        {
            String emailString = String.Format("mailto:{0}?body=//Plattform%20Information,%20DO%20NOT%20REMOVE!//%0ARuntime%20Plattform:%20{1}%0AVersion%20Number:%20{2}%0A//Plattform%20Information,%20DO%20NOT%20REMOVE!//", AppResources.EmailAdress, RuntimePlatform, VersionNumber);
            Device.OpenUri(new Uri(emailString));
        }

        private void PhoneUs(object obj)
        {
            Device.OpenUri(new Uri("tel:" + AppResources.PhoneNumber));
        }

        private void ContactUs(object obj)
        {
            Device.OpenUri(new Uri(AppResources.LinkToContactForm));
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
