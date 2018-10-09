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
using System.Text;
using System.Reactive.Linq;
using SimpleQ.Extensions;
using Xamarin.Forms;
using SimpleQ.Shared;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the BasicQuestionPageModel for the Page xy.
    /// </summary>
    public class BasicQuestionPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicQuestionPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public BasicQuestionPageModel(IQuestionService questionService) : this()
        {
            this.questionService = questionService;
            isItAStartQuestion = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BasicQuestionPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public BasicQuestionPageModel()
        {

        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            if (initData != null)
            {
                if (initData.GetType() == typeof(SurveyModel))
                {
                    this.Question = (SurveyModel)initData;
                }
                else if (initData.GetType() == typeof(Boolean))
                {
                    isItAStartQuestion = (Boolean)initData;
                }
            }

            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The actual question, with the ansDesc and the question
        /// </summary>
        private SurveyModel question;
        /// <summary>
        /// The question service to set the ansDesc of the actual question
        /// </summary>
        private IQuestionService questionService;

        protected Boolean isItAStartQuestion;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public SurveyModel Question
        {
            get => question;
            set
            {
                question = value;

                Debug.WriteLine("QuestionChanged: " + question, "Info");
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
        #endregion

        #region Methods
        public void QuestionAnswered(AnswerOption answer, string answerText)
        {
            this.question.GivenAnswers.Add(new AnswerOption() { AnsId = ansId, AnsText = answerText, SvyId = this.question.SurveyId });
            QuestionAnswered();
        }

        public void QuestionAnswered(AnswerOption answer)
        {
            this.question.GivenAnswers.Add(answer);
            QuestionAnswered();
        }

        public void QuestionAnswered(List<AnswerOption> answers)
        {
            this.question.GivenAnswers = answers;
            QuestionAnswered();
        }

        protected async void QuestionAnswered()
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0}...", Question.SurveyId), "Info");

            Debug.WriteLine("QS: " + this.questionService);
            if (this.questionService == null)
            {
                IQuestionService qs = FreshIOC.Container.Resolve<IQuestionService>();
                qs.QuestionAnswered(this.Question);
            }
            else
            {
                this.questionService.QuestionAnswered(this.Question);
            }
            try
            {
                Boolean CloseAppAfterNotification = await BlobCache.UserAccount.GetObject<Boolean>("CloseAppAfterNotification");
                if (CloseAppAfterNotification && isItAStartQuestion)
                {
                    Debug.WriteLine("Before Closer...", "Info");
                    ICloseApplication closer = DependencyService.Get<ICloseApplication>();
                    Debug.WriteLine("Closer: " + closer, "Info");
                    closer?.CloseApplication();
                }
                else
                {
                    CoreMethods.PopPageModel();
                }
            }
            catch (KeyNotFoundException ex)
            {
                CoreMethods.PopPageModel();
            }

            if (this.questionService.PublicQuestions.Count <= 0)
            {
                App.MainMasterPageModel.SetNewCategorie(AppResources.AllCategories);
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
