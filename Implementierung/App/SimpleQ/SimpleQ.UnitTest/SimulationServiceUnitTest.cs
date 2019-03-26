using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQ.Models;
using SimpleQ.PageModels.Services;

namespace SimpleQ.UnitTest
{
    [TestClass]
    public class SimulationServiceUnitTest
    {
        [TestMethod]
        public async Task CheckCodeTest()
        {
            //Arrange
            ISimulationService simulationService = new SimulationService();

            int testCode1 = 123456;
            int testCode2 = 111111;
            int testCode3 = 312222;
            int testCode4 = 123456;
            int testCode5 = 000000;

            //Act
            CodeValidationModel result1 = await simulationService.CheckCode(testCode1);
            CodeValidationModel result2 = await simulationService.CheckCode(testCode2);
            CodeValidationModel result3 = await simulationService.CheckCode(testCode3);
            CodeValidationModel result4 = await simulationService.CheckCode(testCode4);
            CodeValidationModel result5 = await simulationService.CheckCode(testCode5);

            //Assert
            Assert.IsTrue(result1.IsValid);
            Assert.IsFalse(result2.IsValid);
            Assert.IsFalse(result3.IsValid);
            Assert.IsTrue(result4.IsValid);
            Assert.IsFalse(result5.IsValid);
        }

        [TestMethod]
        public async Task GetDataTest()
        {
            //Arrange
            ISimulationService simulationService = new SimulationService();

            //Act
            Boolean result1 = await simulationService.GetData();

            //Assert
            Assert.IsTrue(result1);
        }
    }
}
