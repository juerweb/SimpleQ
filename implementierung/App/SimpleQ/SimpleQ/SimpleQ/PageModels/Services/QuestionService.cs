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
using Newtonsoft.Json;

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
        public QuestionService(ISimulationService simulationService, IWebAPIService webAPIService, IDialogService dialogService): this()
        {
            this.simulationService = simulationService;
            this.webAPIService = webAPIService;
            this.dialogService = dialogService;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// Without Parameter
        /// </summary>
        public QuestionService()
        {
            Debug.WriteLine("Constructor of QuestionService...", "Info");
            questions = new ObservableCollection<SurveyModel>();
            this.PublicQuestions = Questions;

            currentCategorie = AppResources.AllCategories;

            answeredQuestions = new List<SurveyModel>();
            this.IsPublicQuestionsEmpty = true;
        }

        public QuestionService(Boolean test)
        {
            questions = new ObservableCollection<SurveyModel>();

            answeredQuestions = new List<SurveyModel>();

            this.PublicQuestions = Questions;

            this.IsPublicQuestionsEmpty = false;
        }
        #endregion

        #region Fields
        /// <summary>
        /// All questions
        /// </summary>
        private ObservableCollection<SurveyModel> questions;
        /// <summary>
        /// All questions, which the user answered and which aren't send to the server.
        /// </summary>
        private List<SurveyModel> answeredQuestions;
        /// <summary>
        /// All questions, which are actual shown on the front page.
        /// </summary>
        private ObservableCollection<SurveyModel> publicQuestions;

        private Boolean isPublicQuestionsEmpty;

        private String currentCategorie;

        private ISimulationService simulationService;
        private IWebAPIService webAPIService;
        private IDialogService dialogService;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets the questions.
        /// </summary>
        /// <value>
        /// The questions.
        /// </value>
        public ObservableCollection<SurveyModel> Questions { get => questions; private set => questions = value; }
        /// <summary>
        /// Gets or sets the public questions.
        /// </summary>
        /// <value>
        /// The public questions.
        /// </value>
        public ObservableCollection<SurveyModel> PublicQuestions
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
        public async Task<Boolean> QuestionAnswered(SurveyModel question)
        {
            Debug.WriteLine("Question Service with question from type: " + question.GetType(), "Info");

            if (Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.iOS || Xamarin.Forms.Device.RuntimePlatform == Xamarin.Forms.Device.Android)
            {
                //Android or iOS App
                //this.webAPIService.AnswerSurvey(new Shared.SurveyVote { ChosenAnswerOptions=question.})
                List<RegistrationDataModel> tmp = JsonConvert.DeserializeObject<List<RegistrationDataModel>>(Application.Current.Properties["registrations"].ToString());
                question.SurveyVote.CustCode = tmp[0].RegistrationData.CustCode;

                Debug.WriteLine("Answer Question with CustCode: " + question.SurveyVote.CustCode);
                Debug.WriteLine("Answer Question with VoteText: " + question.SurveyVote.VoteText);
                Debug.WriteLine("Answer Question with a Count of ChosenAnswerOptions of: " + question.SurveyVote.ChosenAnswerOptions.Count);

                Boolean success = false;
                try
                {
                    success = await this.webAPIService.AnswerSurvey(question.SurveyVote);
                }
                catch (System.Net.Http.HttpRequestException e)
                {
                    Application.Current.Properties["IsValidCodeAvailable"] = false;
                    Debug.WriteLine("WebException during the Validation", "Error");

                    this.dialogService.ShowErrorDialog(202);
                }
                
                if (!success)
                {
                    this.dialogService.ShowErrorDialog(204);
                }
                else
                {
                    MoveQuestion(question);
                    await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", this.questions.ToList<SurveyModel>());
                }
                return success;
            }
            else
            {
                //UWP App
                simulationService.SetAnswerOfQuestion(question);
                MoveQuestion(question);
                await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", this.questions.ToList<SurveyModel>());
                return true;
            }
        }

        public async void RemoveQuestion(SurveyModel question)
        {
            Debug.WriteLine("Remove Question with the id: " + question.SurveyId, "Info");

            this.Questions.Remove(question);

            await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", this.questions.ToList<SurveyModel>());

            //simulationService.SetAnswerOfQuestion(question);
        }

        /// <summary>
        /// This method add a new question and checks if the catName of the question already exists.
        /// </summary>
        /// <param name="question">The question.</param>
        public async void AddQuestion(SurveyModel question)
        {
            Debug.WriteLine("Add new Question...", "Info");
            List<SurveyModel> list;
            try
            {
                list = await BlobCache.LocalMachine.GetObject<List<SurveyModel>>("Questions");
            }
            catch (KeyNotFoundException e)
            {
                list = new List<SurveyModel>();
            }
            
            if (list.Count(sm => sm.SurveyId == question.SurveyId) == 0)
            {
                list.Add(question);
                await BlobCache.LocalMachine.InsertObject<List<SurveyModel>>("Questions", list);
            }

            this.Questions.Add(question);

            if (!(App.MainMasterPageModel.MenuItems[0].Count(menuItem => menuItem.Title == question.CatName) > 0))
            {
                //catName does not exists
                Debug.WriteLine("Add new catName from QuestionService", "Info");

                App.MainMasterPageModel.AddCategorie(question.CatName);
            }

            await BlobCache.LocalMachine.Flush();
        }

        /// <summary>
        /// This method sets the actual catName filter and furthermore the new public Collection with the questions in it.
        /// </summary>
        /// <param name="categorie">The catName.</param>
        public void SetCategorieFilter(String categorie)
        {
            CurrentCategorie = categorie;
            if (categorie == AppResources.AllCategories)
            {
                this.PublicQuestions = Questions;
            }
            else
            {
                this.PublicQuestions = new ObservableCollection<SurveyModel>(this.questions.Where(question => question.CatName == categorie).ToList());
            }
        }

        public void MoveQuestion(SurveyModel question)
        {
            Debug.WriteLine("Test0");
            if (questions.Contains(question))
            {
                Debug.WriteLine("Test1");
                this.questions.Remove(question);

                if (PublicQuestions.Contains(question))
                {
                    Debug.WriteLine("Test2");
                    PublicQuestions.Remove(question);
                }

                Debug.WriteLine("Test3");
                this.answeredQuestions.Add(question);
            }
        }

        private void PublicQuestions_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Debug.WriteLine("PubliQuestions Changed... Actual count of elements: " + PublicQuestions.Count, "Info");
            IsPublicQuestionsEmpty = PublicQuestions.Count <= 0;
        }

        public async Task LoadData()
        {
            BlobCache.LocalMachine.GetAndFetchLatest<List<SurveyModel>>("Questions", async () => await simulationService.GetData(), null, null).Subscribe(qst=> {
                if (qst != null)
                {
                    Device.BeginInvokeOnMainThread(() => { Questions.Clear(); });
                    foreach (SurveyModel question in qst)
                    {
                        Device.BeginInvokeOnMainThread(() => { AddQuestion(question); });
                    }
                    this.SetCategorieFilter(this.currentCategorie);
                }

            });
        }

        public async Task LoadDataFromCache()
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    List<SurveyModel> list = await BlobCache.LocalMachine.GetObject<List<SurveyModel>>("Questions");

                    this.Questions = new ObservableCollection<SurveyModel>();
                    this.PublicQuestions = this.Questions;

                    foreach (SurveyModel sm in list)
                    {
                        this.AddQuestion(sm);
                    }
                    this.SetCategorieFilter(this.currentCategorie);
                }
                catch
                {

                }

            });
        }

        public void RemoveQuestions()
        {
            this.Questions = new ObservableCollection<SurveyModel>();
            this.PublicQuestions = this.Questions;
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
