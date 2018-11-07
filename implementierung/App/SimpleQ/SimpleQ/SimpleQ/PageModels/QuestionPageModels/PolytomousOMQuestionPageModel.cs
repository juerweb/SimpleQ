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
using System.Collections.ObjectModel;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the GAQPageModel for the GAQPage.
    /// </summary>
    public class PolytomousOMQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public PolytomousOMQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            IsQuestionAnswered = false;
            IsChecked = new ObservableCollection<IsCheckedModel>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public PolytomousOMQuestionPageModel()
        {
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
            foreach (AnswerOption option in this.Question.GivenAnswers)
            {
                this.isChecked.Add(new IsCheckedModel(option));
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The is question answered
        /// </summary>
        private Boolean isQuestionAnswered;

        private ObservableCollection<IsCheckedModel> isChecked;

        #endregion

        #region Properties + Getter/Setter Methods

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

        public ObservableCollection<IsCheckedModel> IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
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
            List<AnswerOption> result = new List<AnswerOption>();
            foreach (IsCheckedModel model in this.IsChecked)
            {
                if (model.IsChecked)
                {
                    result.Add(model.AnswerOption);
                }
            }

            base.QuestionAnswered(result);
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        #endregion
    }
}
