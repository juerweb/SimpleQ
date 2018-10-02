using FreshMvvm;
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
using System.Reactive.Linq;
using Akavache;
using SimpleQ.Shared;
using System.Collections.ObjectModel;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the YNQPageModel for the Page YNQPage.
    /// </summary>
    public class DichotomousQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestionPageModel" /> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public DichotomousQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            Option1Command = new Command(() => QuestionAnswered(YNQAnswer.Yes));
            Option2Command = new Command(() => QuestionAnswered(YNQAnswer.No));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public DichotomousQuestionPageModel()
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
        private ObservableCollection<AnswerOption> answerOptions;
        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<AnswerOption> AnswerOptions { get => answerOptions; set => answerOptions = value; }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the yes command.
        /// </summary>
        /// <value>
        /// The yes command.
        /// </value>
        public Command Option1Command { get; private set; }
        /// <summary>
        /// Gets the no command.
        /// </summary>
        /// <value>
        /// The no command.
        /// </value>
        public Command Option2Command { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the question was answered.
        /// </summary>
        /// <param name="answer">The ansDesc.</param>
        private void QuestionAnswered(YNQAnswer answer)
        {
            base.QuestionAnswered(answer.ToString());
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        #endregion
    }
}
