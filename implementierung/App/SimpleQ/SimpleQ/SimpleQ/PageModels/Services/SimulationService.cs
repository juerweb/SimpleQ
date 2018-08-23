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

            List<QuestionModel> questions = new List<QuestionModel>();
            questions.Add(new YNQModel("Sind Sie männlich?", "YNQ Test", 0));
            questions.Add(new TLQModel("Sind Sie anwesend?", "TLQ Test", 1));
            questions.Add(new OWQModel("Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OWQ Test", 2));
            questions.Add(new GAQModel("Was ist Ihre Lieblingsfarbe?", "GAQ Test", 1, new String[] { "Grün", "Rot", "Gelb", "Blau" }));

            return questions;
        }
    }
}
