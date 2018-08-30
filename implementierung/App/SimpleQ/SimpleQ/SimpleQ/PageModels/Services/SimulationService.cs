using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public class SimulationService : ISimulationService
    {
        public SimulationService()
        {
            questions.Add(new QuestionModel("Sind Sie männlich?", "YNQ Test", 0, QuestionType.YNQ));
            questions.Add(new QuestionModel("Sind Sie anwesend?", "TLQ Test", 1, QuestionType.TLQ));
            questions.Add(new QuestionModel("Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OWQ Test", 2, QuestionType.OWQ));
            questions.Add(new QuestionModel("Was ist Ihre Lieblingsfarbe?", "GAQ Test", 3, QuestionType.GAQ, new List<String>(new String[] { "Grün", "Rot", "Gelb", "Blau" })));
        }
        List<QuestionModel> questions = new List<QuestionModel>();
        public async Task<CodeValidationModel> CheckCode(int code)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return new CodeValidationModel(code == 123456, "Tina's Factory", "Development", code);
        }

        public async Task<List<QuestionModel>> GetData()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            return questions;
        }

        public void SetAnswerOfQuestion(QuestionModel question)
        {
            foreach (QuestionModel q in questions)
            {
                if (q.QuestionId == question.QuestionId)
                {
                    questions.Remove(q);
                    return;
                }
            }
        }
    }
}
