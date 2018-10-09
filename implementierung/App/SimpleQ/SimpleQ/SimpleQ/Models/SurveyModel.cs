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
        public SurveyModel(int surveyId, string surveyDesc, string catName, SurveyType surveyType, DateTime endDate, List<AnswerOption> givenAnswers) : this()
        {
            this.surveyId = surveyId;
            this.surveyDesc = surveyDesc;
            this.catName = catName;
            this.typeDesc = surveyType;
            this.givenAnswers = givenAnswers;
            this.endDate = endDate;
        }

        public SurveyModel(int surveyId, string surveyDesc, string catName, int typeId, DateTime endDate, List<AnswerOption> givenAnswers) : this()
        {
            this.surveyId = surveyId;
            this.surveyDesc = surveyDesc;
            this.catName = catName;
            this.typeDesc = (SurveyType)typeId;
            this.givenAnswers = givenAnswers;
            this.endDate = endDate;
        }

        public SurveyModel(SurveyNotification sn): this()
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
            IsAnswerAllowed = false;
            this.choosenAnswers = new List<AnswerOption>();
        }


        #endregion

        #region Fields
        private int surveyId;
        private String surveyDesc;
        private String catName;
        private SurveyType typeDesc;
        private Boolean isAnswerAllowed;
        private List<AnswerOption> givenAnswers;
        private List<AnswerOption> choosenAnswers;
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

        public List<AnswerOption> GivenAnswers { get => givenAnswers; set => givenAnswers = value; }
        public DateTime EndDate { get => endDate; set => endDate = value; }
        public List<AnswerOption> ChoosenAnswers { get => choosenAnswers; set => choosenAnswers = value; }
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
        YesNoQuestion = 1,
        YesNoDontKnowQuestion = 2,
        TrafficLightQuestion = 3,
        OpenQuestion = 4,
        DichotomousQuestion = 5,
        PolytomousUSQuestion = 6,
        PolytomousUMQuestion = 7,
        PolytomousOSQuestion = 8,
        PolytomousOMQuestion = 9,
        LikertScale3Question = 10,
        LikertScale4Question = 11,
        LikertScale5Question = 12,
        LikertScale6Question = 13,
        LikertScale7Question = 14,
        LikertScale8Question = 15,
        LikertScale9Question = 16,
        //GAQ is only tmp Type
        GAQ = 17,

    }

    public enum YNQAnswer
    {
        Yes = 1,
        No = 2
    }

    public enum YNDKQAnswer
    {
        Yes = 1,
        No = 2,
        DontKnow = 3
    }

    public enum TLQAnswer
    {
        Green = 1,
        Yellow = 2,
        Red = 3
    }
}
