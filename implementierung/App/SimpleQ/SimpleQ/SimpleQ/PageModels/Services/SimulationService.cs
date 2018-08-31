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
            questions.Add(new SurveyModel(0, "Sind Sie männlich?", "YNQ Test", SurveyType.YNQ, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(1, "Sind Sie anwesend?", "TLQ Test", SurveyType.TLQ, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(2, "Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OWQ Test", SurveyType.OWQ, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(3, "Was ist Ihre Lieblingsfarbe?", "GAQ Test", SurveyType.GAQ, DateTime.Now, DateTime.Now, new List<String>(new String[] { "Grün", "Rot", "Gelb", "Blau" })));
        }
        List<SurveyModel> questions = new List<SurveyModel>();
        public async Task<CodeValidationModel> CheckCode(int code)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return new CodeValidationModel(code == 123456, "Tina's Factory", "Development", code);
        }

        public async Task<List<SurveyModel>> GetData()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            return questions;
        }

        public void SetAnswerOfQuestion(SurveyModel question)
        {
            foreach (SurveyModel q in questions)
            {
                if (q.SurveyId == question.SurveyId)
                {
                    questions.Remove(q);
                    return;
                }
            }
        }
    }
}
