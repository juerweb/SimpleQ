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
    /// This is the GAQPageModel for the Page xy.
    /// </summary>
    public class GAQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public GAQPageModel(IQuestionService questionService) : this()
        {
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GAQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public GAQPageModel()
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            IsQuestionAnswered = false;
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.question = (GAQModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        private GAQModel question;
        private String selectedAnswer;
        private Boolean isQuestionAnswered;
        private IQuestionService questionService;

        #endregion

        #region Properties + Getter/Setter Methods
        public GAQModel Question { get => question; set => question = value; }
        public string SelectedAnswer
        {
            get => selectedAnswer;
            set
            {
                selectedAnswer = value;
                IsQuestionAnswered = true;
                OnPropertyChanged();
            }
        }

        public IQuestionService QuestionService { get => questionService; set => questionService = value; }

        public bool IsQuestionAnswered
        {
            get => isQuestionAnswered;
            set { isQuestionAnswered = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public Command SendAnswerCommand { get; private set; }
        #endregion

        #region Methods
        private void QuestionAnswered()
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with the answer {1}...", Question.QuestionId, selectedAnswer), "Info");

            this.question.Answer = selectedAnswer;

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
