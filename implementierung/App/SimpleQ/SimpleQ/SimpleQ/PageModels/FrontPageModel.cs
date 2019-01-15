using Akavache;
using FreshMvvm;
using MvvmHelpers;
using SimpleQ.Logging;
using SimpleQ.Models;
using SimpleQ.PageModels.QuestionPageModels;
using SimpleQ.PageModels.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the FrontPageModel for the FrontPage.
    /// </summary>
    public class FrontPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="FrontPageModel"/> class.
        /// </summary>
        /// <param name="questionService">The question service.</param>
        public FrontPageModel(IQuestionService questionService, ISimulationService simulationService): this()
        {
            this.questionService = questionService;
            this.simulationService = simulationService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public FrontPageModel()
        {
            DeleteCommand = new Command(DeleteCommandExecuted);
            RefreshCommand = new Command(RefreshCommandExecuted);

            IsRefreshing = false;
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            //Debug.WriteLine("Count of Questions in PublicQuestions: " + this.QuestionService.PublicQuestions);
            base.Init(initData);
        }
        #endregion

        #region Fields
        /// <summary>
        /// The question service
        /// </summary>
        private IQuestionService questionService;
        /// <summary>
        /// The selected question
        /// </summary>
        private SurveyModel selectedQuestion;

        private ISimulationService simulationService;

        private Boolean isRefreshing;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the question service.
        /// </summary>
        /// <value>
        /// The question service.
        /// </value>
        public IQuestionService QuestionService { get => questionService; set => questionService = value; }
        /// <summary>
        /// Gets or sets the selected question.
        /// </summary>
        /// <value>
        /// The selected question.
        /// </value>
        public SurveyModel SelectedQuestion
        {
            get => selectedQuestion;
            set
            {
                selectedQuestion = value;
                //Debug.WriteLine(selectedQuestion);
                OnPropertyChanged();

                if (selectedQuestion != null)
                {
                    //Debug.WriteLine(selectedQuestion.SurveyDesc);
                    NavigateToQuestion();
                }
            }
        }

        public bool IsRefreshing
        {
            get => isRefreshing;
            set { isRefreshing = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        public Command DeleteCommand { get; set; }
        public Command RefreshCommand { get; set; }
        #endregion

        #region Methods
        /// <summary>
        /// This method opens the detailpage from the specific question type, with the selectedQuestion as parameter.
        /// </summary>
        private void NavigateToQuestion()
        {
            if (selectedQuestion.TypeDesc == SurveyType.YesNoQuestion)
            {
                CoreMethods.PushPageModel<YesNoQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.YesNoDontKnowQuestion)
            {
                CoreMethods.PushPageModel<YesNoDontKnowQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.TrafficLightQuestion)
            {
                CoreMethods.PushPageModel<TrafficLightQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.OpenQuestion)
            {
                CoreMethods.PushPageModel<OpenQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.PolytomousUSQuestion)
            {
                CoreMethods.PushPageModel<PolytomousUSQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.DichotomousQuestion)
            {
                CoreMethods.PushPageModel<DichotomousQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.PolytomousOSQuestion)
            {
                CoreMethods.PushPageModel<PolytomousOSQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.PolytomousOMQuestion)
            {
                CoreMethods.PushPageModel<PolytomousOMQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.PolytomousUMQuestion)
            {
                CoreMethods.PushPageModel<PolytomousUMQuestionPageModel>(new List<object> { selectedQuestion, false });
            }
            else if (selectedQuestion.TypeDesc == SurveyType.LikertScale3Question 
                || selectedQuestion.TypeDesc == SurveyType.LikertScale4Question
                || selectedQuestion.TypeDesc == SurveyType.LikertScale5Question
                || selectedQuestion.TypeDesc == SurveyType.LikertScale6Question
                || selectedQuestion.TypeDesc == SurveyType.LikertScale7Question
                || selectedQuestion.TypeDesc == SurveyType.LikertScale8Question
                || selectedQuestion.TypeDesc == SurveyType.LikertScale9Question)
            {
                CoreMethods.PushPageModel<LikertScaleQuestionPageModel>(new List<object> { selectedQuestion, false });
            }

            SelectedQuestion = null;
        }

        private void DeleteCommandExecuted(object question)
        {
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("Delete Command Executed with object: " + question);
            //Debug.WriteLine("Delete Command Executed with object: " + question, "Info");
            questionService.RemoveQuestion((SurveyModel)question);
        }

        private async void RefreshCommandExecuted()
        {
            //Debug.WriteLine("Refresh Command Executed...", "Info");
            Logging.ILogger logger = DependencyService.Get<ILogManager>().GetLog();
            logger.Info("Refresh Command Executed.");
            await this.questionService.LoadDataFromCache();
            this.IsRefreshing = false;
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
