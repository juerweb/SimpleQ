using SimpleQ.Models;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SimpleQ.PageModels.Services
{
    /// <summary>
    /// This is the QuestionService.
    /// </summary>
    public class QuestionService : IQuestionService, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public QuestionService(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// Without Parameter
        /// </summary>
        public QuestionService()
        {
            Questions = new ObservableCollection<QuestionModel>();

            AnsweredQuestions = new List<QuestionModel>();

            

            //only in DEBUG Modus => Demo Data
            this.AddQuestion(new YNQModel("Sind Sie männlich?", "YNQ Test", 0));
            this.AddQuestion(new TLQModel("Sind Sie anwesend?", "TLQ Test", 1));
            this.AddQuestion(new OWQModel("Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OWQ Test", 2));
            this.AddQuestion(new GAQModel("Was ist Ihre Lieblingsfarbe?", "GAQ Test", 1, new String[] { "Grün", "Rot", "Gelb", "Blau" }));
            //end of demo data

            this.PublicQuestions = Questions;

            this.IsPublicQuestionsEmpty = false;

        }
        #endregion

        #region Fields
        /// <summary>
        /// All questions
        /// </summary>
        private ObservableCollection<QuestionModel> questions;
        /// <summary>
        /// All questions, which the user answered and which aren't send to the server.
        /// </summary>
        private List<QuestionModel> answeredQuestions;
        /// <summary>
        /// All questions, which are actual shown on the front page.
        /// </summary>
        private ObservableCollection<QuestionModel> publicQuestions;

        private Boolean isPublicQuestionsEmpty;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        public ObservableCollection<QuestionModel> Questions { get => questions; private set => questions = value; }
        /// <summary>
        /// Gets or sets the answered questions.
        /// </summary>
        /// <value>
        /// The answered questions.
        /// </value>
        public List<QuestionModel> AnsweredQuestions { get => answeredQuestions; set => answeredQuestions = value; }
        /// <summary>
        /// Gets or sets the public questions.
        /// </summary>
        /// <value>
        /// The public questions.
        /// </value>
        public ObservableCollection<QuestionModel> PublicQuestions
        {
            get => publicQuestions;
            set
            {
                publicQuestions = value;
                OnPropertyChanged();
                this.PublicQuestions.CollectionChanged += PublicQuestions_CollectionChanged;
                PublicQuestions_CollectionChanged(null, null);
            }
            
        }

        public bool IsPublicQuestionsEmpty
        {
            get => isPublicQuestionsEmpty;
            set { isPublicQuestionsEmpty = value; OnPropertyChanged(); }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the user answered the question. The method calls a method in the questionService.
        /// </summary>
        /// <param name="question">The question.</param>
        public void QuestionAnswered(QuestionModel question)
        {
            Debug.WriteLine("Question Service with question from type: " + question.GetType(), "Info");

            MoveQuestion(question);

        }

        /// <summary>
        /// This method add a new question and checks if the categorie of the question already exists.
        /// </summary>
        /// <param name="question">The question.</param>
        public void AddQuestion(QuestionModel question)
        {
            this.questions.Add(question);
            if (!(App.MainMasterPageModel.MenuItems[0].Count(menuItem => menuItem.Title == question.Categorie) > 0))
            {
                //categorie does not exists

                App.MainMasterPageModel.AddCategorie(question.Categorie);
            }
        }

        /// <summary>
        /// This method sets the actual categorie filter and furthermore the new public Collection with the questions in it.
        /// </summary>
        /// <param name="categorie">The categorie.</param>
        public void SetCategorieFilter(String categorie)
        {
            if (categorie == AppResources.AllCategories)
            {
                this.PublicQuestions = Questions;
            }
            else
            {
                this.PublicQuestions = new ObservableCollection<QuestionModel>(this.questions.Where(question => question.Categorie == categorie).ToList());
            }
        }

        private void MoveQuestion(QuestionModel question)
        {
            if (questions.Contains(question))
            {
                this.questions.Remove(question);

                if (PublicQuestions.Contains(question))
                {
                    PublicQuestions.Remove(question);
                }

                this.answeredQuestions.Add(question);
            }
        }

        private void PublicQuestions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("PubliQuestions Changed... Actual count of elements: " + PublicQuestions.Count, "Info");
            IsPublicQuestionsEmpty = PublicQuestions.Count <= 0;
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
