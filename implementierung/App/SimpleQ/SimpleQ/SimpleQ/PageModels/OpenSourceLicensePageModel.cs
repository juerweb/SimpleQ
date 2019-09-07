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
    public class OpenSourceLicensePageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSourceLicensePageModel"/> class.
        /// Without Parameter
        /// </summary>
        public OpenSourceLicensePageModel(): base()
        {
            openSourceLicenses = new ObservableCollection<OpenSourceLicenseModel>();
            openSourceLicenses.Add(new OpenSourceLicenseModel("Acr.UserDialogs", "A cross platform library that allows you to call for standard user dialogs from a core .net standard library, Actionsheets, alerts, confirmations, loading, login, progress, prompt, toast...", "MIT", "7.0.1", "https://github.com/aritchie/userdialogs"));
            openSourceLicenses.Add(new OpenSourceLicenseModel("Akavache", "An asynchronous, persistent key-value store", "MIT", "5.0.0", "https://github.com/reactiveui/Akavache"));
            openSourceLicenses.Add(new OpenSourceLicenseModel("OneSignal-Xamarin-SDK", "OneSignal is a free push notification service for mobile apps. This plugin makes it easy to integrate your Xamarin app with OneSignal.", "Modified MIT License", "3.2.0", "https://github.com/OneSignal/OneSignal-Xamarin-SDK"));
            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "FreshMvvm",
                "FreshMvvm is a super light Mvvm Framework designed specifically for Xamarin.Forms. It's designed to be Easy, Simple and Flexible.",
                "Apache License 2.0",
                "2.2.4",
                "https://github.com/rid00z/FreshMvvm"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "IntelliAbb.Xamarin.Controls",
                "Cross-platform controls for Xamarin and Xamarin.Forms.",
                "Apache License 2.0",
                "1.0.115-pre",
                "https://github.com/Intelliabb/XamarinControls"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Microsoft.AppCenter",
                "Development repository for the App Center SDK for .NET platforms, including Xamarin",
                "MIT",
                "1.8.0",
                "https://github.com/Microsoft/AppCenter-SDK-DotNet"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "NLog",
                "Advanced and Structured Logging for Various .NET Platforms",
                "BSD 3-Clause",
                "4.5.11",
                "https://github.com/NLog/NLog"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Plugin.Multilingual",
                "Multilingual Plugin for Xamarin and Windows",
                "MIT",
                "1.0.2",
                "https://github.com/CrossGeeks/MultilingualPlugin"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Refractored.MvvmHelpers",
                "Collection of MVVM helper classes for any application",
                "MIT",
                "1.3.0",
                "https://github.com/jamesmontemagno/mvvm-helpers"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Splat",
                "A library to make things cross-platform that should be",
                "MIT",
                "5.0.2",
                "https://github.com/reactiveui/splat"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Xamarin.Forms",
                "A plattform to develop cross-platform-apps.",
                "MIT",
                "3.3.0.912540",
                "https://github.com/xamarin/Xamarin.Forms"
            ));

            openSourceLicenses.Add(new OpenSourceLicenseModel(
                "Zxing.Net.Mobile",
                "Zxing Barcode Scanning Library for MonoTouch, Mono for Android, and Windows Phone",
                "Apache-2.0",
                "2.4.1",
                "https://github.com/Redth/ZXing.Net.Mobile"
            ));
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
        private ObservableCollection<OpenSourceLicenseModel> openSourceLicenses;
        private OpenSourceLicenseModel selectedItem;
        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<OpenSourceLicenseModel> OpenSourceLicenses
        {
            get => openSourceLicenses;
            set
            {
                 openSourceLicenses = value;
                OnPropertyChanged();
            }
        }

        public OpenSourceLicenseModel SelectedItem
        {
            get => selectedItem;
            set
            {
                if (value != null)
                {
                    selectedItem = value;
                    OnPropertyChanged();
                    //Debug.WriteLine(selectedItem.Url);
                    OpenLink(selectedItem.Url);
                    selectedItem = null;
                    OnPropertyChanged();
                }

            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        private void OpenLink(String url)
        {
            //Debug.WriteLine(url);
            Device.OpenUri(new Uri(url));
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
