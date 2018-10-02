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

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the YNQPageModel for the Page YNQPage.
    /// </summary>
    public class YesNoQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestionPageModel" /> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public YesNoQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            YesCommand = new Command(() => QuestionAnswered(YNQAnswer.Yes));
            NoCommand = new Command(() => QuestionAnswered(YNQAnswer.No));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YesNoQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public YesNoQuestionPageModel()
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

        #endregion

        #region Properties + Getter/Setter Methods

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
            base.QuestionAnswered(answer.ToString());
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        #endregion
    }
}
