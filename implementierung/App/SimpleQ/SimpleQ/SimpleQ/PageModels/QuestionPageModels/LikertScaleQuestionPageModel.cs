﻿using Akavache;
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
using System.Linq;

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
            this.CurrentValue = 1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public LikertScaleQuestionPageModel()
        {
            BeginText = "";
            EndText = "";
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
            switch (this.Question.TypeDesc)
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

            BeginText = this.Question.GivenAnswers.Where(ga => ga.FirstPosition == true).ToList()[0].AnsText;
            EndText = this.Question.GivenAnswers[this.Question.GivenAnswers.Count() - 1].AnsText;

        }
        #endregion

        #region Fields
        /// <summary>
        /// The ansDesc
        /// </summary>
        private int currentValue;
        private double gradation;
        private String beginText;
        private String endText;

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

        public string BeginText
        {
            get => beginText;
            set
            {
                beginText = value;
                OnPropertyChanged();
            }
        }

        public string EndText
        {
            get => endText;
            set
            {
                endText = value;
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
            AnswerOption current;
            if (currentValue == 1)
            {
                current = this.Question.GivenAnswers.Where(ga => ga.FirstPosition == true).ToList()[0];
            }
            else if (currentValue == this.Question.GivenAnswers.Count())
            {
                current = this.Question.GivenAnswers[this.Question.GivenAnswers.Count() - 1];
            }
            else
            {
                current = this.Question.GivenAnswers.Where(ga => ga.AnsText == currentValue.ToString()).ToList()[0];
            }
            //Debug.WriteLine(current.AnsText);
            base.QuestionAnswered(current);
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        //In the Base Class
        #endregion
    }
}
