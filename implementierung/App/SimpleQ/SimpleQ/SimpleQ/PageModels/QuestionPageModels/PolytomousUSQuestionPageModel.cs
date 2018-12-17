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
    /// This is the GAQPageModel for the GAQPage.
    /// </summary>
    public class PolytomousUSQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public PolytomousUSQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            IsQuestionAnswered = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public PolytomousUSQuestionPageModel()
        {

        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The selected ansDesc
        /// </summary>
        private AnswerOption selectedAnswer;
        /// <summary>
        /// The is question answered
        /// </summary>
        private Boolean isQuestionAnswered;

        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the selected ansDesc.
        /// </summary>
        /// <value>
        /// The selected ansDesc.
        /// </value>
        public AnswerOption SelectedAnswer
        {
            get => selectedAnswer;
            set
            {
                selectedAnswer = value;
                IsQuestionAnswered = true;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is question answered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is question answered; otherwise, <c>false</c>.
        /// </value>
        public bool IsQuestionAnswered
        {
            get => isQuestionAnswered;
            set { isQuestionAnswered = value; OnPropertyChanged(); }
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
            base.QuestionAnswered(selectedAnswer);
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        #endregion
    }
}
