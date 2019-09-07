using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    public class IsCheckedModel<T> : INotifyPropertyChanged
    {
        #region Constructor(s)
        public IsCheckedModel(T option) : this()
        {
            this.answerOption = option;
        }
        public IsCheckedModel()
        {
            isChecked = false;
        }
        #endregion

        #region Fields
        private bool isChecked;
        private T answerOption;
        #endregion

        #region Properties + Getter/Setter Methods
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                this.isChecked = value;
                OnPropertyChanged();
            }
        }
        public T AnswerOption { get => answerOption; set => answerOption = value; }
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
