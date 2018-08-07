using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public class SimulationService : ISimulationService
    {
        public async Task<bool> CheckCode(int code)
        {
            Debug.WriteLine("LoadingPageModel initalised", "Info");
            await Task.Delay(TimeSpan.FromSeconds(3));

            //Random random = new Random();
            //return random.Next(0, 2) == 0;

            return code == 123456;
        }
    }
}
