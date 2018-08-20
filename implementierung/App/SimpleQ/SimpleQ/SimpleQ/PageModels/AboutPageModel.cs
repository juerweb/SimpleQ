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
    /// This is the AboutPageModel for the AboutPage.
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
        /// <summary>
        /// Gets the version number of the app.
        /// </summary>
        /// <value>
        /// The version number.
        /// </value>
        public string VersionNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetVersionNumber(); }
        /// <summary>
        /// Gets the build number of the app.
        /// </summary>
        /// <value>
        /// The build number.
        /// </value>
        public string BuildNumber { get => DependencyService.Get<IAppVersionAndBuild>().GetBuildNumber(); }
        /// <summary>
        /// Gets the runtime platform.
        /// </summary>
        /// <value>
        /// The runtime platform.
        /// </value>
        public string RuntimePlatform { get => Device.RuntimePlatform; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the visit us command.
        /// </summary>
        /// <value>
        /// The visit us command.
        /// </value>
        public Command VisitUsCommand { get; private set; }
        /// <summary>
        /// Gets the email us command.
        /// </summary>
        /// <value>
        /// The email us command.
        /// </value>
        public Command EmailUsCommand { get; private set; }
        /// <summary>
        /// Gets the phone us command.
        /// </summary>
        /// <value>
        /// The phone us command.
        /// </value>
        public Command PhoneUsCommand { get; private set; }
        /// <summary>
        /// Gets the contact us command.
        /// </summary>
        /// <value>
        /// The contact us command.
        /// </value>
        public Command ContactUsCommand { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// Method for the VisitUsCommand, whichs open the website of SimpleQ in the default browser of the Device.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void VisitUs(object obj)
        {
            Device.OpenUri(new Uri(AppResources.LinkToWebsite));
        }

        /// <summary>
        /// Method for the EmailUsCommand, whichs opens the default E-Mail App, with some extra Informations about the App.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void EmailUs(object obj)
        {
            String emailString = String.Format("mailto:{0}?body=//Plattform%20Information,%20DO%20NOT%20REMOVE!//%0ARuntime%20Plattform:%20{1}%0AVersion%20Number:%20{2}%0A//Plattform%20Information,%20DO%20NOT%20REMOVE!//", AppResources.EmailAdress, RuntimePlatform, VersionNumber);
            Device.OpenUri(new Uri(emailString));
        }

        /// <summary>
        /// Method for the PhoneUsCommand, whichs opens the default Phone App on the device.
        /// </summary>
        /// <param name="obj">The object.</param>
        private void PhoneUs(object obj)
        {
            Device.OpenUri(new Uri("tel:" + AppResources.PhoneNumber));
        }

        /// <summary>
        /// Method for the ContactUsCommand, whichs opens the contact form in the internet
        /// </summary>
        /// <param name="obj">The object.</param>
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
