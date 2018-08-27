using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the Base Class for all Questions.
    /// </summary>
    public class QuestionModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public QuestionModel(String questionDesc, String categorie, int questionId, QuestionType questionType, List<String> givenAnswers = null): this()
        {
            this.questionDesc = questionDesc;
            this.categorie = categorie;
            this.questionId = questionId;
            this.questionType = questionType;
            this.givenAnswers = givenAnswers;

            this.answer = "";
            IsAnswerAllowed = false;
        }

        public QuestionModel()
        {

        }
        #endregion

        #region Fields
        private int questionId;
        private String questionDesc;
        private String categorie;
        private QuestionType questionType;
        private String answer;
        private Boolean isAnswerAllowed;
        private List<string> givenAnswers;
        #endregion

        #region Properties + Getter/Setter Methods
        public int QuestionId
        {
            get => questionId;
            set { questionId = value; OnPropertyChanged(); }

        }
        public string QuestionDesc
        {
            get => questionDesc;
            set { questionDesc = value; OnPropertyChanged(); }
        }
        public string Categorie
        {
            get => categorie;
            set { categorie = value; OnPropertyChanged(); }
        }

        public bool IsAnswerAllowed
        {
            get => isAnswerAllowed;
            set { isAnswerAllowed = value; OnPropertyChanged(); }
        }

        public QuestionType QuestionType { get => questionType; set => questionType = value; }

        public string Answer
        {
            get => answer;
            set
            {
                answer = value;
                if (questionType == QuestionType.GAQ)
                {
                    CheckIfAnswerIsAllowed();
                }
            } 
        }

        public List<string> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }

        //public List<string> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }
        #endregion

        #region Methods
        private void CheckIfAnswerIsAllowed()
        {
            //IsAnswerAllowed = givenAnswers.Contains(answer);
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

    public enum QuestionType
    {
        YNQ = 0,
        TLQ = 1,
        OWQ = 2,
        GAQ = 3
    }

    public enum YNQAnswer
    {
        Yes = 0,
        No = 1
    }

    public enum TLQAnswer
    {
        Green = 0,
        Red = 1
    }
}
