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

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the OWQPageModel for the OWQPage.
    /// </summary>
    public class OpenQuestionPageModel : BasicQuestionPageModel
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="OpenQuestionPageModel"/> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public OpenQuestionPageModel(IQuestionService questionService) : base(questionService)
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            this.Answer = "";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public OpenQuestionPageModel()
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
        /// The ansDesc
        /// </summary>
        private String answer;

        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the ansDesc.
        /// </summary>
        /// <value>
        /// The ansDesc.
        /// </value>
        public String Answer
        {
            get => answer;
            set
            {
                answer = value;
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
            base.QuestionAnswered(1, answer);
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
