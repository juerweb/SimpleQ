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
    /// This is the OWQPageModel for the Page xy.
    /// </summary>
    public class OWQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="OWQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
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
        private OWQModel question;
        private String answer;
        private IQuestionService questionService;

        #endregion

        #region Properties + Getter/Setter Methods
        public OWQModel Question { get => question; set => question = value; }
        public String Answer
        {
            get => answer;
            set
            {
                answer = value;
                OnPropertyChanged();
            }
        }
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }
        #endregion

        #region Commands
        public Command SendAnswerCommand { get; private set; }
        #endregion

        #region Methods
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
