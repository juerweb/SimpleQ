using FreshMvvm;
using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public FrontPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FrontPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public FrontPageModel()
        {
            Questions = new ObservableCollection<QuestionModel>();
            Questions.Add(new YNQModel("Sind Sie männlich?", "Allgemein", 0));
            Questions.Add(new YNQModel("Sind Sie anwesend?", "Allgemein", 1));
            Questions.Add(new OWQModel("Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "Allgemein", 2));
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
        private ObservableCollection<QuestionModel> questions;
        private QuestionModel selectedQuestion;
        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<QuestionModel> Questions { get => questions; set => questions = value; }
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
        private void NavigateToQuestion()
        {
            CoreMethods.PushPageModel<QuestionPageModel>(selectedQuestion);
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
