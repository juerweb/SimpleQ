using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface IQuestionService
    {
        ObservableCollection<SurveyModel> Questions { get; }
        ObservableCollection<SurveyModel> PublicQuestions { get; set; }
        Boolean IsPublicQuestionsEmpty { get; set; }

        Task<Boolean> QuestionAnswered(SurveyModel question);
        void AddQuestion(SurveyModel question);
        void SetCategorieFilter(String categorie);
        void MoveQuestion(SurveyModel question);
        Task LoadData();
        Task LoadDataFromCache();
        void RemoveQuestion(SurveyModel questionModel);
        void RemoveQuestions();

    }
}
