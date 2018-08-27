using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using SimpleQ.Resources;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the GAQPageModel for the GAQPage.
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
            this.question = (QuestionModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The question
        /// </summary>
        private QuestionModel question;
        /// <summary>
        /// The selected answer
        /// </summary>
        private String selectedAnswer;
        /// <summary>
        /// The is question answered
        /// </summary>
        private Boolean isQuestionAnswered;
        /// <summary>
        /// The question service
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
        public QuestionModel Question { get => question; set => question = value; }
        /// <summary>
        /// Gets or sets the selected answer.
        /// </summary>
        /// <value>
        /// The selected answer.
        /// </value>
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

        /// <summary>
        /// Gets or sets the question service.
        /// </summary>
        /// <value>
        /// The question service.
        /// </value>
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }

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
        /// Gets the send answer command.
        /// </summary>
        /// <value>
        /// The send answer command.
        /// </value>
        public Command SendAnswerCommand { get; private set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the user answered the question. The method calls a method in the questionService.
        /// </summary>
        private void QuestionAnswered()
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with the answer {1}...", Question.QuestionId, selectedAnswer), "Info");

            this.question.Answer = selectedAnswer;

            this.questionService.QuestionAnswered(question);

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
