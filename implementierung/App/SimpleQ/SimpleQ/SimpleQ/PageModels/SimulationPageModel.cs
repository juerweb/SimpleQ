using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the SimulationPageModel for the Page xy.
    /// </summary>
    public class SimulationPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public SimulationPageModel(String title): this()
        {
            Debug.WriteLine("Set title of SimulationPage to " + title + "...", "Info");
            this.title = title;
        }

        public SimulationPageModel()
        {

        }

        public override void Init(object initData)
        {
            base.Init(initData);

            Debug.WriteLine("Title: " + this.title);
        }
        #endregion

        #region Fields
        private string title;
        #endregion

        #region Properties + Getter/Setter Methods
        public string Title { get => title; set => title = value; }
        #endregion

        #region Commands
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
