using Akavache;
using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Reactive.Linq;
using SimpleQ.Shared;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the OWQPageModel for the OWQPage.
    /// </summary>
    public class LikertScaleQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenQuestionPageModel"/> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public LikertScaleQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            this.CurrentValue = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public LikertScaleQuestionPageModel()
        {

        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            if (initData.GetType() == typeof(SurveyModel))
            {
                SurveyModel tmp = (SurveyModel)initData;
                switch (tmp.TypeDesc)
                {
                    case SurveyType.LikertScale3Question:
                        this.Gradation = 3;
                        break;
                    case SurveyType.LikertScale4Question:
                        this.Gradation = 4;
                        break;
                    case SurveyType.LikertScale5Question:
                        this.Gradation = 5;
                        break;
                    case SurveyType.LikertScale6Question:
                        this.Gradation = 6;
                        break;
                    case SurveyType.LikertScale7Question:
                        this.Gradation = 7;
                        break;
                    case SurveyType.LikertScale8Question:
                        this.Gradation = 8;
                        break;
                    case SurveyType.LikertScale9Question:
                        this.Gradation = 9;
                        break;
                }
            }
            base.Init(initData);

        }
        #endregion

        #region Fields
        /// <summary>
        /// The ansDesc
        /// </summary>
        private int currentValue;
        private double gradation;

        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the ansDesc.
        /// </summary>
        /// <value>
        /// The ansDesc.
        /// </value>
        public int CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = value;
                OnPropertyChanged();
            }
        }

        public double Gradation
        {
            get => gradation;
            set
            {
                gradation = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the send ansDesc command.
        /// </summary>
        /// <value>
        /// The send ansDesc command.
        /// </value>
        public Command SendAnswerCommand { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the user answered the question. The method calls a method in the questionService.
        /// </summary>
        private void QuestionAnswered()
        {
            base.QuestionAnswered(this.Question.GivenAnswers[currentValue]);
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
