using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface ISimulationService
    {
        Task<CodeValidationModel> CheckCode(string code);
        Task<List<SurveyModel>> GetData();
        void SetAnswerOfQuestion(SurveyModel question);
        void Logout(int registerCode);
    }
}
