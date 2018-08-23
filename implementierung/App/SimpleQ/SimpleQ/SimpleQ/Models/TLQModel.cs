using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the TLQModel for the xyPage
    /// </summary>
    public class TLQModel : QuestionModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public TLQModel(string questionDesc, string categorie, int questionId) : base(questionDesc, categorie, questionId, QuestionType.TLQ)
        {

        }

        public TLQModel()
        {

        }
        #endregion

        #region Fields
        private TLQAnswer answer;
        #endregion

        #region Properties + Getter/Setter Methods
        public TLQAnswer Answer { get => answer; set => answer = value; }
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

    public enum TLQAnswer
    {
        Green = 0,
        Red = 1
    }
}
