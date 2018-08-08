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

        public async Task<Boolean> GetData()
        {
            await Task.Delay(TimeSpan.FromSeconds(5));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return true;
        }
    }
}
