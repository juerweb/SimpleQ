using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
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
    /// This is the TLQPageModel for the Page xy.
    /// </summary>
    public class TLQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="TLQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public TLQPageModel(IQuestionService questionService) : this()
        {
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TLQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public TLQPageModel()
        {
            GreenCommand = new Command(() => QuestionAnswered(TLQAnswer.Green));
            RedCommand = new Command(() => QuestionAnswered(TLQAnswer.Red));
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.Question = (TLQModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        private TLQModel question;
        private IQuestionService questionService;
        #endregion

        #region Properties + Getter/Setter Methods
        public TLQModel Question { get => question; set => question = value; }
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }
        #endregion

        #region Commands
        public Command RedCommand { get; private set; }
        public Command GreenCommand { get; private set; }
        #endregion

        #region Methods
        private void QuestionAnswered(TLQAnswer answer)
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with {1}...", Question.QuestionId, answer), "Info");

            this.question.Answer = answer;

            this.questionService.QuestionAnswered(question);
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
