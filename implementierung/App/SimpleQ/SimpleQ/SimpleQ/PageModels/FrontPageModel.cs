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
        public FrontPageModel(IQuestionService questionService): this()
        {
            this.questionService = questionService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public FrontPageModel()
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
        /// The question service
        /// </summary>
        private IQuestionService questionService;
        /// <summary>
        /// The selected question
        /// </summary>
        private QuestionModel selectedQuestion;
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
        public QuestionModel SelectedQuestion
        {
            get => selectedQuestion;
            set
            {
                selectedQuestion = value;
                OnPropertyChanged();

                if (selectedQuestion != null)
                {
                    NavigateToQuestion();
                }
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// This method opens the detailpage from the specific question type, with the selectedQuestion as parameter.
        /// </summary>
        private void NavigateToQuestion()
        {
            if (selectedQuestion.GetType() == typeof(YNQModel))
            {
                CoreMethods.PushPageModel<YNQPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.GetType() == typeof(TLQModel))
            {
                CoreMethods.PushPageModel<TLQPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.GetType() == typeof(OWQModel))
            {
                CoreMethods.PushPageModel<OWQPageModel>(selectedQuestion);
            }
            else if (selectedQuestion.GetType() == typeof(GAQModel))
            {
                CoreMethods.PushPageModel<GAQPageModel>(selectedQuestion);
            }
            
            selectedQuestion = null;
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
