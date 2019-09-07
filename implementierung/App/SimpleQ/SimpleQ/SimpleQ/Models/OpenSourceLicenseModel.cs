using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the Base Class for all Questions.
    /// </summary>
    public class OpenSourceLicenseModel : INotifyPropertyChanged
    {
        #region Constructor(s)

        public OpenSourceLicenseModel()
        {
        }

        public OpenSourceLicenseModel(string name, string description, string license, string version, string url) :this()
        {
            this.name = name;
            this.description = description;
            this.license = license;
            this.version = version;
            this.url = url;
        }


        #endregion

        #region Fields
        private String name;
        private String description;
        private String license;
        private String version;
        private String url;
        #endregion

        #region Properties + Getter/Setter Methods
        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public string License { get => license; set => license = value; }
        public string Version { get => version; set => version = value; }
        public string Url { get => url; set => url = value; }
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
