using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the YNQModel for the xyPage
    /// </summary>
    public class YNQModel : QuestionModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public YNQModel(string questionDesc, string categorie, int questionId) : base(questionDesc, categorie, questionId, QuestionType.YNQ)
        {

        }

        public YNQModel()
        {

        }
        #endregion

        #region Fields
        private YNQAnswer answer;
        #endregion

        #region Properties + Getter/Setter Methods
        public YNQAnswer Answer { get => answer; set => answer = value; }
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

    public enum YNQAnswer
    {
        Yes = 0,
        No = 1
    }
}
