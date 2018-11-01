using FreshMvvm;
using Newtonsoft.Json;
using SimpleQ.PageModels.Services;
using SimpleQ.Shared;
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
    /// This is the UnregisterPageModel for the UnregisterPage.
    /// </summary>
    public class UnregisterPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisterPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public UnregisterPageModel(ISimulationService simulationService, IDialogService dialogService, IWebAPIService webAPIService) : this()
        {
            this.simulationService = simulationService;
            this.dialogService = dialogService;
            this.webAPIService = webAPIService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UnregisterPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public UnregisterPageModel()
        {
            UnregisterCommand = new Command(UnregisterCommandExecuted);
            registrations = new ObservableCollection<RegistrationData>(JsonConvert.DeserializeObject<List<RegistrationData>>(Application.Current.Properties["registrations"].ToString()));
            isChecked = new Dictionary<RegistrationData, bool>();
            foreach (RegistrationData data in registrations)
            {
                isChecked.Add(data, false);
            }
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
        private ObservableCollection<RegistrationData> registrations;
        private Dictionary<RegistrationData, bool> isChecked;
        private ISimulationService simulationService;
        private IDialogService dialogService;
        private IWebAPIService webAPIService;
        private Boolean isOneChecked;

        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<RegistrationData> Registrations
        {
            get => registrations;
            set
            {
                registrations = value;
                OnPropertyChanged();
            }
        }

        public Dictionary<RegistrationData, bool> IsChecked { get => isChecked; set => isChecked = value; }

        public bool IsOneChecked
        {
            get => isOneChecked;
            set
            {
                isOneChecked = value;
                OnPropertyChanged();
            }
        }
        #endregion

            #region Commands
        public Command UnregisterCommand
        {
            get;
            private set;
        }
        #endregion

        #region Methods
        private async void UnregisterCommandExecuted()
        {
            Debug.WriteLine("Unregister Command Executed...", "Info");
            if (await dialogService.ShowReallySureDialog())
            {
                try
                {
                    if (Application.Current.Properties.ContainsKey("PersId") && Application.Current.Properties.ContainsKey("CustCode"))
                    {
                        Debug.WriteLine("Unregister Command executed on iOS or Android with PersId: " + Application.Current.Properties["PersId"] + " and CustCode: " + Application.Current.Properties["CustCode"], "Info");
                        Boolean success = await this.webAPIService.Unregister(Application.Current.Properties["PersId"].ToString(), Application.Current.Properties["CustCode"].ToString());
                        if (success)
                        {
                            Application.Current.Properties.Remove("IsValidCodeAvailable");
                            Application.Current.Properties.Remove("CompanyName");
                            Application.Current.Properties.Remove("DepartmentName");
                            Application.Current.Properties.Remove("RegisterCode");
                            Application.Current.Properties.Remove("PersId");
                            Application.Current.Properties.Remove("CustCode");
                        }
                        else
                        {
                            Debug.WriteLine("Problem during the Unregister", "Error");
                            this.dialogService.ShowErrorDialog(203);
                        }
                    }
                    else
                    {
                        //UWP App
                        Debug.WriteLine("Unregister Command executed on UWP", "Info");
                        Application.Current.Properties.Remove("IsValidCodeAvailable");
                        Application.Current.Properties.Remove("CompanyName");
                        Application.Current.Properties.Remove("DepartmentName");
                        Application.Current.Properties.Remove("RegisterCode");
                    }
                }
                catch (System.Net.Http.HttpRequestException e)
                {
                    Debug.WriteLine("WebException during the Unregister", "Error");
                    this.dialogService.ShowErrorDialog(202);
                    return;
                }
                App.GoToRightPage();
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
