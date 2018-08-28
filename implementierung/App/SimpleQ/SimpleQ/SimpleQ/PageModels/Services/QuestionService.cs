using Akavache;
using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

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
        public QuestionService(ISimulationService simulationService): this()
        {
            this.simulationService = simulationService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// Without Parameter
        /// </summary>
        public QuestionService()
        {
            questions = new ObservableCollection<QuestionModel>();
            this.PublicQuestions = Questions;
            currentCategorie = AppResources.AllCategories;

            answeredQuestions = new List<QuestionModel>();
            this.IsPublicQuestionsEmpty = true;
        }

        public QuestionService(Boolean test)
        {
            questions = new ObservableCollection<QuestionModel>();

            answeredQuestions = new List<QuestionModel>();

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

        private String currentCategorie;

        private ISimulationService simulationService;
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

        public string CurrentCategorie { get => currentCategorie; set => currentCategorie = value; }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// This method is called, after the user answered the question. The method calls a method in the questionService.
        /// </summary>
        /// <param name="question">The question.</param>
        public async void QuestionAnswered(QuestionModel question)
        {
            Debug.WriteLine("Question Service with question from type: " + question.GetType(), "Info");

            MoveQuestion(question);

            await BlobCache.LocalMachine.InsertObject<List<QuestionModel>>("Questions", this.questions.ToList<QuestionModel>());

            simulationService.SetAnswerOfQuestion(question);
        }

        public async void RemoveQuestion(QuestionModel question)
        {
            Debug.WriteLine("Remove Question with the id: " + question.QuestionId, "Info");

            this.Questions.Remove(question);

            await BlobCache.LocalMachine.InsertObject<List<QuestionModel>>("Questions", this.questions.ToList<QuestionModel>());

            simulationService.SetAnswerOfQuestion(question);
        }

        /// <summary>
        /// This method add a new question and checks if the categorie of the question already exists.
        /// </summary>
        /// <param name="question">The question.</param>
        public void AddQuestion(QuestionModel question)
        {

            this.Questions.Add(question);

            if (!(App.MainMasterPageModel.MenuItems[0].Count(menuItem => menuItem.Title == question.Categorie) > 0))
            {
                //categorie does not exists
                Debug.WriteLine("Add new categorie from QuestionService", "Info");

                App.MainMasterPageModel.AddCategorie(question.Categorie);
            }
        }

        /// <summary>
        /// This method sets the actual categorie filter and furthermore the new public Collection with the questions in it.
        /// </summary>
        /// <param name="categorie">The categorie.</param>
        public void SetCategorieFilter(String categorie)
        {
            CurrentCategorie = categorie;
            if (categorie == AppResources.AllCategories)
            {
                this.PublicQuestions = Questions;
            }
            else
            {
                this.PublicQuestions = new ObservableCollection<QuestionModel>(this.questions.Where(question => question.Categorie == categorie).ToList());
            }
        }

        public void MoveQuestion(QuestionModel question)
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

        public void LoadData()
        {
            BlobCache.LocalMachine.GetAndFetchLatest<List<QuestionModel>>("Questions", async () => await simulationService.GetData(), null, null).Subscribe(qst=> {
                if (qst != null)
                {
                    Device.BeginInvokeOnMainThread(() => { Questions.Clear(); });
                    foreach (QuestionModel question in qst)
                    {
                        Device.BeginInvokeOnMainThread(() => { AddQuestion(question); });
                    }
                    this.SetCategorieFilter(this.currentCategorie);
                }

            });
        }

        public async Task RequestData()
        {
            List<QuestionModel> qst = await simulationService.GetData();
            Questions.Clear();
            foreach (QuestionModel question in qst)
            {
                Device.BeginInvokeOnMainThread(() => { AddQuestion(question); });
            }
            this.SetCategorieFilter(this.currentCategorie);
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
