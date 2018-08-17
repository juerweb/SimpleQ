using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the FAQModel for the FAQPage
    /// </summary>
    public class FAQModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public FAQModel(string question, string answer): this()
        {
            this.question = question;
            this.answer = answer;
        }

        public FAQModel()
        {
            this.IsActive = false;
        }
        #endregion

        #region Fields
        private String question;
        private String answer;
        private Boolean isActive;

        #endregion

        #region Properties + Getter/Setter Methods
        public string Question { get => question; set => question = value; }
        public string Answer { get => answer; set => answer = value; }
        public Boolean IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
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
