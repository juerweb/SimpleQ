using System;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQ.Webinterface.Controllers;
using SimpleQ.Webinterface.Models;
using SimpleQ.Webinterface.Models.ViewModels;

namespace SimpleQ.UnitTests.Webinterface
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestSingleResultData()
        {
            var controller = new SurveyResultsController();
            ViewResult result = controller.LoadSingleResult(1) as ViewResult;
            SingleResultModel model = result.Model as SingleResultModel;

            Assert.AreEqual("Ist N.H. ein Nazi?", model.Survey.SvyText);
            Assert.AreEqual("Politische Fragen", model.CatName);
            Assert.AreEqual("YesNo", model.TypeName);
            Assert.AreEqual(2, model.DepartmentNames.Count);
            Assert.AreEqual(4, model.Votes.Count);
            Assert.IsNull(model.FreeTextVotes);
        }
    }
}
