using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        public override void Init(object initData)
        {
            base.Init(initData);

            Random random = new Random();
            this.Title = "Test " + random.Next(0, 100).ToString();
        }
        #endregion

        #region Fields
        private string title;
        #endregion

        #region Properties + Getter/Setter Methods
        public string Title { get => title; set { title = value; OnPropertyChanged(); } }
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
