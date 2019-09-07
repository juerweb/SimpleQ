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
using System.Linq;
using System.Threading.Tasks;
using SimpleQ.Logging;

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
                List<object> objects = (List<object>)initData;
                this.Question = (SurveyModel)objects[0];
                if (this.Question.TypeDesc == SurveyType.PolytomousOMQuestion || this.Question.TypeDesc == SurveyType.PolytomousOSQuestion)
                {
                    this.Question.GivenAnswers = this.Question.GivenAnswers.OrderBy(ga => ga.AnsText).ToList();
                }
                this.isItAStartQuestion = (Boolean)objects[1];
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
        private Boolean isRunning;
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
                //Debug.WriteLine("QuestionChanged: " + question, "Info");
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

        public void QuestionAnswered(string answerText)
        {
            this.question.SurveyVote.ChosenAnswerOptions = this.question.GivenAnswers;
            this.question.SurveyVote.VoteText = answerText;
            QuestionAnswered();
        }

        public void QuestionAnswered(AnswerOption answer)
        {
            this.question.SurveyVote.ChosenAnswerOptions.Add(answer);
            QuestionAnswered();
        }

        public void QuestionAnswered(List<AnswerOption> answers)
        {
            this.question.SurveyVote.ChosenAnswerOptions = answers;
            QuestionAnswered();
        }

        private async void QuestionAnswered()
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0}...", Question.SurveyId), "Info");
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("User answered the question with the id " + Question.SurveyId);

            //Debug.WriteLine("QS: " + this.questionService);
            Boolean success;
            if (this.questionService == null)
            {
                IQuestionService qs = FreshIOC.Container.Resolve<IQuestionService>();
                success = await qs.QuestionAnswered(this.Question);
            }
            else
            {
                success = await this.questionService.QuestionAnswered(this.Question);
            }

            Boolean ShowMessageAfterAnswering = false;
            try
            {
                ShowMessageAfterAnswering = await BlobCache.UserAccount.GetObject<Boolean>("ShowMessageAfterAnswering");
                if (ShowMessageAfterAnswering && success)
                {
                    if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
                    {
                        IToastService toastService = FreshIOC.Container.Resolve<IToastService>();
                        toastService.LongMessage(AppResources.SuccessfulAnsweringQuestion);
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("Exception thrown " + e.StackTrace);
                //Debug.WriteLine("Exception thrown " + e.StackTrace, "Exception");
            }

            try
            {
                Boolean CloseAppAfterNotification = await BlobCache.UserAccount.GetObject<Boolean>("CloseAppAfterNotification");
                //Debug.WriteLine("isItAStartQuestion: " + isItAStartQuestion, "Info");
                logger.Info("Is it a start question? " + isItAStartQuestion);
                //Debug.WriteLine("CloseAppAfterNotification: " + CloseAppAfterNotification, "Info");
                if (CloseAppAfterNotification && isItAStartQuestion)
                {
                    if (ShowMessageAfterAnswering)
                    {
                        await Task.Delay(int.Parse(AppResources.CloseInterval));
                    }

                    //Debug.WriteLine("Before Closer...", "Info");
                    ICloseApplication closer = DependencyService.Get<ICloseApplication>();
                    //Debug.WriteLine("Closer: " + closer, "Info");
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
