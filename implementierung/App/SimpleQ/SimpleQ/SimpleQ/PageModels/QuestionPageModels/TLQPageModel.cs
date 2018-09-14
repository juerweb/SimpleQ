using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using System.Reactive.Linq;
using Akavache;
using System.Collections.Generic;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the TLQPageModel for the Page TLQPage.
    /// </summary>
    public class TLQPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="TLQPageModel"/> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public TLQPageModel(IQuestionService questionService) : base(questionService)
        {
            GreenCommand = new Command(() => QuestionAnswered(TLQAnswer.Green));
            RedCommand = new Command(() => QuestionAnswered(TLQAnswer.Red));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TLQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public TLQPageModel()
        {

        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.Question = (SurveyModel) initData;

            base.Init(initData);
        }
        #endregion

        #region Fields
        #endregion

        #region Properties + Getter/Setter Methods
        #endregion

        #region Commands
        /// <summary>
        /// Gets the red command.
        /// </summary>
        /// <value>
        /// The red command.
        /// </value>
        public Command RedCommand { get; private set; }
        /// <summary>
        /// Gets the green command.
        /// </summary>
        /// <value>
        /// The green command.
        /// </value>
        public Command GreenCommand { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the user answered the question. The method calls a method in the questionService.
        /// </summary>
        /// <param name="answer">The ansDesc.</param>
        private void QuestionAnswered(TLQAnswer answer)
        {
            base.QuestionAnswered(answer.ToString());
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
