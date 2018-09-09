﻿using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the YNQPageModel for the Page YNQPage.
    /// </summary>
    public class YNQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="YNQPageModel" /> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public YNQPageModel(IQuestionService questionService) : this()
        {
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YNQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public YNQPageModel()
        {
            YesCommand = new Command(()=>QuestionAnswered(YNQAnswer.Yes));
            NoCommand = new Command(() => QuestionAnswered(YNQAnswer.No));
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.question = (SurveyModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual question, with the ansDesc and the question
        /// </summary>
        private SurveyModel question;
        /// <summary>
        /// The question service to set the ansDesc of the actual question
        /// </summary>
        private IQuestionService questionService;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public SurveyModel Question
        {
            get => question;
            set
            {
                question = value;

                Debug.WriteLine("QuestionChanged: " + question, "Info");
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the question service.
        /// </summary>
        /// <value>
        /// The question service.
        /// </value>
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the yes command.
        /// </summary>
        /// <value>
        /// The yes command.
        /// </value>
        public Command YesCommand { get; private set; }
        /// <summary>
        /// Gets the no command.
        /// </summary>
        /// <value>
        /// The no command.
        /// </value>
        public Command NoCommand { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the question was answered.
        /// </summary>
        /// <param name="answer">The ansDesc.</param>
        private void QuestionAnswered(YNQAnswer answer)
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with {1}...", Question.SurveyId, answer), "Info");

            this.question.AnsDesc = answer.ToString();


            Debug.WriteLine("QS: " + this.questionService);
            if (this.questionService == null)
            {
                IQuestionService qs = FreshIOC.Container.Resolve<IQuestionService>();
                qs.QuestionAnswered(this.Question);
            }
            else
            {
                this.questionService.QuestionAnswered(this.Question);
            }

            CoreMethods.PopPageModel();
            if (this.questionService.PublicQuestions.Count <= 0)
            {
                App.MainMasterPageModel.SetNewCategorie(AppResources.AllCategories);
            }
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
