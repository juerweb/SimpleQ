using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the SettingsPageModel for the Page Settings.
    /// </summary>
    public class SettingsPageModel : StandardMenuPageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public SettingsPageModel(ISimulationService simulationService, IDialogService dialogService): this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingsPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public SettingsPageModel(): base()
        {
            MenuItems.Add(new MenuItemModel(AppResources.Language, new LanguagePageModel(), "ic_language_black_18.png"));

            LogOutCommand = new Command(LogOutCommandExecuted);
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override async void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        private ISimulationService simulationService;
        private IDialogService dialogService;
        #endregion

        #region Properties + Getter/Setter Methods
        #endregion

        #region Commands
        public Command LogOutCommand
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        private async void LogOutCommandExecuted()
        {
            if (await dialogService.ShowReallySureDialog())
            {
                simulationService.Logout((int)Application.Current.Properties["RegisterCode"]);
                Application.Current.Properties.Remove("IsValidCodeAvailable");
                Application.Current.Properties.Remove("CompanyName");
                Application.Current.Properties.Remove("DepartmentName");
                Application.Current.Properties.Remove("RegisterCode");

                App.GoToRightPage();
            }

        }
        #endregion

        #region INotifyPropertyChanged Implementation
        //The Implemenation of the INotifyPropertyChanged is in the Base Class
        #endregion
    }
}
