using SimpleQ.Models;
using SimpleQ.Shared;
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
            questions.Add(new SurveyModel(0, "Sind Sie männlich?", "YesNoQuestion Test", SurveyType.YesNoQuestion, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(3, "Sind Sie männlich?", "YesNoDontKnowQuestion Test", SurveyType.YesNoDontKnowQuestion, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(1, "Sind Sie anwesend?", "TrafficLightQuestion Test", SurveyType.TrafficLightQuestion, DateTime.Now, DateTime.Now));
            questions.Add(new SurveyModel(2, "Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OpenQuestion Test", SurveyType.OpenQuestion, DateTime.Now, DateTime.Now));
            List<AnswerOption> answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Grün", SvyId = 0 });
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Rot", SvyId = 1 });
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Gelb", SvyId = 2 });
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Blau", SvyId = 3 });

            questions.Add(new SurveyModel(3, "Was ist Ihre Lieblingsfarbe?", "GAQ Test", SurveyType.GAQ, DateTime.Now, DateTime.Now, answerTypes));
        }
        List<SurveyModel> questions = new List<SurveyModel>();
        public async Task<CodeValidationModel> CheckCode(int code)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return new CodeValidationModel(code == 1234567, "Tina's Factory", "Development", code);
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

        public void Logout(int registerCode)
        {
            Debug.WriteLine(String.Format("User with the registerCode: {0} logged out...", registerCode), "Info");
            return;
        }
    }
}
