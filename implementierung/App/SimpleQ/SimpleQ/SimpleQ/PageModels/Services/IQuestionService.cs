using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface IQuestionService
    {
        ObservableCollection<QuestionModel> Questions { get; }
        List<QuestionModel> AnsweredQuestions { get; set; }

        void QuestionAnswered(QuestionModel question);

    }
}
