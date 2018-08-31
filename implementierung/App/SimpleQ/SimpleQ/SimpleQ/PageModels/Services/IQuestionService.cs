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

        void QuestionAnswered(SurveyModel question);
        void AddQuestion(SurveyModel question);
        void SetCategorieFilter(String categorie);
        void MoveQuestion(SurveyModel question);
        void LoadData();
        Task RequestData();
        void RemoveQuestion(SurveyModel questionModel);

    }
}
