using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the GAQModel for the xyPage
    /// </summary>
    public class GAQModel : QuestionModel, INotifyPropertyChanged
    {

        #region Constructor(s)
        public GAQModel(string questionDesc, string categorie, int questionId, ICollection<String> givenAnswers) : base(questionDesc, categorie, questionId)
        {
            this.givenAnswers = givenAnswers;
            IsAnswerAllowed = false;
        }
        #endregion

        #region Fields
        private ICollection<String> givenAnswers;
        private Boolean isAnswerAllowed;
        private String answer;
        #endregion

        #region Properties + Getter/Setter Methods
        public ICollection<string> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }
        public bool IsAnswerAllowed
        {
            get => isAnswerAllowed;
            set { isAnswerAllowed = value; OnPropertyChanged(); }
        }
        public string Answer
        {
            get => answer;
            set { answer = value; CheckIfAnswerIsAllowed(); }
        }
        #endregion

        #region Methods
        private void CheckIfAnswerIsAllowed()
        {
            IsAnswerAllowed = givenAnswers.Contains(answer);
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
