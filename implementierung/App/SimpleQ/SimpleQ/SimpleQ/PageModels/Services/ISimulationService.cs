using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface ISimulationService
    {
        Task<CodeValidationModel> CheckCode(int code);
        Task<Boolean> GetData();
    }
}
