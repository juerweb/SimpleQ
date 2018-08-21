using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the OWQPageModel for the OWQPage.
    /// </summary>
    public class OWQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="OWQPageModel"/> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public OWQPageModel(IQuestionService questionService) : this()
        {
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OWQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public OWQPageModel()
        {
            SendAnswerCommand = new Command(QuestionAnswered);
            this.Answer = "";
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.question = (OWQModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The question
        /// </summary>
        private OWQModel question;
        /// <summary>
        /// The answer
        /// </summary>
        private String answer;
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
        public OWQModel Question { get => question; set => question = value; }
        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer.
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
        /// <summary>
        /// Gets or sets the question service.
        /// </summary>
        /// <value>
        /// The question service.
        /// </value>
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }
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
            Debug.WriteLine(String.Format("User answered the question with the id {0} with the answertext '{1}'...", Question.QuestionId, answer), "Info");

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
