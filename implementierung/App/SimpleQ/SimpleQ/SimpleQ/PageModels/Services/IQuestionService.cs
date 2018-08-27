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
        ObservableCollection<QuestionModel> Questions { get; }
        ObservableCollection<QuestionModel> PublicQuestions { get; set; }
        Boolean IsPublicQuestionsEmpty { get; set; }

        void QuestionAnswered(QuestionModel question);
        void AddQuestion(QuestionModel question);
        void SetCategorieFilter(String categorie);
        void MoveQuestion(QuestionModel question);
        Task RequestData();
        void LoadDataFromCache();
        void RemoveQuestion(QuestionModel questionModel);
        void CheckIfRequestIsNeeded();

    }
}
