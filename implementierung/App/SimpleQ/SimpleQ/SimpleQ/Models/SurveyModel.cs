using SimpleQ.Shared;
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
    public class SurveyModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public SurveyModel(int surveyId, string surveyDesc, string catName, SurveyType surveyType, DateTime startDate, DateTime endDate, List<AnswerOption> givenAnswers = null) : this()
        {
            this.surveyId = surveyId;
            this.surveyDesc = surveyDesc;
            this.catName = catName;
            this.typeDesc = surveyType;
            this.givenAnswers = givenAnswers;
            this.endDate = endDate;
        }
        public SurveyModel(int surveyId, string surveyDesc, string catName, int typeId, DateTime startDate, DateTime endDate, List<AnswerOption> givenAnswers = null) : this()
        {
            this.surveyId = surveyId;
            this.surveyDesc = surveyDesc;
            this.catName = catName;
            this.typeDesc = (SurveyType)typeId;
            this.givenAnswers = givenAnswers;
            this.endDate = endDate;
        }

        public SurveyModel(SurveyNotification sn)
        {
            this.surveyId = sn.SvyId;
            this.surveyDesc = sn.SvyText;
            this.catName = sn.CatName;
            this.typeDesc = (SurveyType)sn.TypeId;
            this.givenAnswers = sn.AnswerOptions;
            this.endDate = sn.EndDate;
        }

        public SurveyModel()
        {
            this.ansDesc = "";
            IsAnswerAllowed = false;
        }


        #endregion

        #region Fields
        private int surveyId;
        private String surveyDesc;
        private String catName;
        private SurveyType typeDesc;
        private String ansDesc;
        private Boolean isAnswerAllowed;
        private List<AnswerOption> givenAnswers;
        private DateTime endDate;
        #endregion

        #region Properties + Getter/Setter Methods
        public int SurveyId
        {
            get => surveyId;
            set { surveyId = value; OnPropertyChanged(); }

        }
        public string SurveyDesc
        {
            get => surveyDesc;
            set { surveyDesc = value; OnPropertyChanged(); }
        }
        public string CatName
        {
            get => catName;
            set { catName = value; OnPropertyChanged(); }
        }

        public bool IsAnswerAllowed
        {
            get => isAnswerAllowed;
            set { isAnswerAllowed = value; OnPropertyChanged(); }
        }

        public SurveyType TypeDesc { get => typeDesc; set => typeDesc = value; }

        public string AnsDesc
        {
            get => ansDesc;
            set
            {
                ansDesc = value;
                if (typeDesc == SurveyType.GAQ)
                {
                    CheckIfAnswerIsAllowed();
                }
            } 
        }

        public List<AnswerOption> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }
        public DateTime EndDate { get => endDate; set => endDate = value; }

        //public List<string> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }
        #endregion

        #region Methods
        private void CheckIfAnswerIsAllowed()
        {
            //IsAnswerAllowed = givenAnswers.Contains(ansDesc);
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

    public enum SurveyType
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
