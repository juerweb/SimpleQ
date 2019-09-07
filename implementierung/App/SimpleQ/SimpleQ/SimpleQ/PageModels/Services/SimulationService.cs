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
            List<AnswerOption> answerTypes = new List<AnswerOption>();

            answerTypes.Add(new AnswerOption() { AnsId = 0, SvyId = 0, AnsText = "Yes" });
            answerTypes.Add(new AnswerOption() { AnsId = 1, SvyId = 0, AnsText = "No" });
            questions.Add(new SurveyModel(0, "Sind Sie männlich?", "YesNoQuestion Test", SurveyType.YesNoQuestion, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, SvyId = 1, AnsText = "Yes" });
            answerTypes.Add(new AnswerOption() { AnsId = 1, SvyId = 1, AnsText = "No" });
            answerTypes.Add(new AnswerOption() { AnsId = 2, SvyId = 1, AnsText = "DontKnow" });
            questions.Add(new SurveyModel(3, "Sind Sie männlich?", "YesNoDontKnowQuestion Test", SurveyType.YesNoDontKnowQuestion, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, SvyId = 2, AnsText = "Green" });
            answerTypes.Add(new AnswerOption() { AnsId = 1, SvyId = 2, AnsText = "Yellow" });
            answerTypes.Add(new AnswerOption() { AnsId = 2, SvyId = 2, AnsText = "Red" });
            questions.Add(new SurveyModel(1, "Sind Sie anwesend?", "TrafficLightQuestion Test", SurveyType.TrafficLightQuestion, DateTime.Now.AddYears(2), answerTypes));

            questions.Add(new SurveyModel(2, "Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OpenQuestion Test", SurveyType.OpenQuestion, DateTime.Now.AddYears(2), new List<AnswerOption>()));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Grün", SvyId = 3 });
            answerTypes.Add(new AnswerOption() { AnsId = 1, AnsText = "Rot", SvyId = 3 });
            answerTypes.Add(new AnswerOption() { AnsId = 2, AnsText = "Gelb", SvyId = 3 });
            answerTypes.Add(new AnswerOption() { AnsId = 3, AnsText = "Blau", SvyId = 3 });

            questions.Add(new SurveyModel(3, "Was ist Ihre Lieblingsfarbe?", "Polytomous US Test", SurveyType.PolytomousUSQuestion, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Männlich", SvyId = 4 });
            answerTypes.Add(new AnswerOption() { AnsId = 1, AnsText = "Weiblich", SvyId = 4 });

            questions.Add(new SurveyModel(4, "Wählen Sie aus!", "Dichotomous Test", SurveyType.DichotomousQuestion, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Grün", SvyId = 5 });
            answerTypes.Add(new AnswerOption() { AnsId = 1, AnsText = "Rot", SvyId = 5 });
            answerTypes.Add(new AnswerOption() { AnsId = 2, AnsText = "Gelb", SvyId = 5 });
            answerTypes.Add(new AnswerOption() { AnsId = 3, AnsText = "Blau", SvyId = 5 });

            questions.Add(new SurveyModel(5, "Was ist Ihre Lieblingsfarbe?", "Polytomous OS Test", SurveyType.PolytomousOSQuestion, DateTime.Now.AddYears(2), answerTypes));
            questions.Add(new SurveyModel(6, "Was ist Ihre Lieblingsfarbe?", "Polytomous OM Test", SurveyType.PolytomousOMQuestion, DateTime.Now.AddYears(2), answerTypes));
            questions.Add(new SurveyModel(7, "Was ist Ihre Lieblingsfarbe?", "Polytomous UM Test", SurveyType.PolytomousUMQuestion, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Gut", SvyId = 8, FirstPosition=true });
            answerTypes.Add(new AnswerOption() { AnsId = 1, AnsText = "Mittel", SvyId = 8, FirstPosition = false });
            answerTypes.Add(new AnswerOption() { AnsId = 2, AnsText = "Schlecht", SvyId = 8, FirstPosition=null });

            questions.Add(new SurveyModel(8, "Wie fühlen Sie sich?", "Likert Scale 3 Test", SurveyType.LikertScale3Question, DateTime.Now.AddYears(2), answerTypes));

            answerTypes = new List<AnswerOption>();
            answerTypes.Add(new AnswerOption() { AnsId = 0, AnsText = "Sehr Gut", SvyId = 9, FirstPosition=true});
            answerTypes.Add(new AnswerOption() { AnsId = 1, AnsText = "Gut", SvyId = 9, FirstPosition = false });
            answerTypes.Add(new AnswerOption() { AnsId = 2, AnsText = "Ausreichend", SvyId = 9, FirstPosition = false });
            answerTypes.Add(new AnswerOption() { AnsId = 3, AnsText = "Schlecht", SvyId = 9, FirstPosition = false });
            answerTypes.Add(new AnswerOption() { AnsId = 4, AnsText = "Sehr Schlecht", SvyId = 9, FirstPosition = false });
            answerTypes.Add(new AnswerOption() { AnsId = 5, AnsText = "Sehr Sehr Schlecht", SvyId = 9, FirstPosition = null });

            questions.Add(new SurveyModel(9, "Wie fühlen Sie sich?", "Likert Scale 6 Test", SurveyType.LikertScale6Question, DateTime.Now.AddYears(2), answerTypes));
        }

        List<SurveyModel> questions = new List<SurveyModel>();
        public async Task<CodeValidationModel> CheckCode(string code)
        {
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return new CodeValidationModel(code == "1234567", "Tina's Factory", "Development", code);
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
