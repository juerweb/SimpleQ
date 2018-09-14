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
        protected async void QuestionAnswered(String answer)
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with {1}...", Question.SurveyId, answer), "Info");

            this.question.AnsDesc = answer;


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
                if (CloseAppAfterNotification)
                {
                    App.Current.Quit();
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
