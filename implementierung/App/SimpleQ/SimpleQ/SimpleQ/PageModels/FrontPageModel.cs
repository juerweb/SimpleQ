using Akavache;
using FreshMvvm;
using MvvmHelpers;
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
                Debug.WriteLine(selectedQuestion);
                OnPropertyChanged();

                if (selectedQuestion != null)
                {
                    Debug.WriteLine(selectedQuestion.SurveyDesc);
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
            if (selectedQuestion.TypeDesc == SurveyType.YNQ)
            {
                CoreMethods.PushPageModel<YesNoQuestionPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.TypeDesc == SurveyType.TLQ)
            {
                CoreMethods.PushPageModel<TrafficLightQuestionPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.TypeDesc == SurveyType.OWQ)
            {
                CoreMethods.PushPageModel<OpenQuestionPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.TypeDesc == SurveyType.GAQ)
            {
                CoreMethods.PushPageModel<GAQPageModel>(selectedQuestion);
            }
            
            SelectedQuestion = null;
        }

        private void DeleteCommandExecuted(object question)
        {
            Debug.WriteLine("Delete Command Executed with object: " + question, "Info");
            questionService.RemoveQuestion((SurveyModel)question);
        }

        private async void RefreshCommandExecuted()
        {
            Debug.WriteLine("Refresh Command Executed...", "Info");
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
