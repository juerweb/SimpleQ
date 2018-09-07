using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpleQ.PageModels.Services;
using SimpleQ.Models;

namespace SimpleQ.UnitTest
{
    /// <summary>
    /// Zusammenfassungsbeschreibung für QuestionServiceUnitTest
    /// </summary>
    [TestClass]
    public class QuestionServiceUnitTest
    {

        [TestMethod]
        public void QuestionAnsweredTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            QuestionModel testQuestion = new QuestionModel("Test", "ABC", 1);
            questionService.Questions.Add(testQuestion);
            questionService.PublicQuestions.Add(testQuestion);

            questionService.QuestionAnswered(testQuestion);

            //Assert
            Assert.IsFalse(questionService.PublicQuestions.Contains(testQuestion));
            Assert.IsFalse(questionService.Questions.Contains(testQuestion));
        }

        [TestMethod]
        public void MoveQuestionTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            QuestionModel testQuestion = new QuestionModel("Test", "ABC", 1);
            questionService.Questions.Add(testQuestion);
            questionService.PublicQuestions.Add(testQuestion);

            questionService.MoveQuestion(testQuestion);

            //Assert
            Assert.IsFalse(questionService.PublicQuestions.Contains(testQuestion));
            Assert.IsFalse(questionService.Questions.Contains(testQuestion));
        }

        [TestMethod]
        public void AddQuestionTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            QuestionModel testQuestion = new QuestionModel("Test", "ABC", 1);
            questionService.Questions.Add(testQuestion);

            //Assert
            Assert.IsTrue(questionService.Questions.Contains(testQuestion));
        }

        [TestMethod]
        public void SetCategorieFilterTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            QuestionModel testQuestion1 = new QuestionModel("Test", "ABC", 1);
            QuestionModel testQuestion2 = new QuestionModel("Test", "CDF", 1);
            QuestionModel testQuestion3 = new QuestionModel("Test", "ABC", 1);
            QuestionModel testQuestion4 = new QuestionModel("Test", "", 1);

            questionService.Questions.Add(testQuestion1);
            questionService.Questions.Add(testQuestion2);
            questionService.Questions.Add(testQuestion3);
            questionService.Questions.Add(testQuestion4);

            questionService.SetCategorieFilter("ABC");

            //Assert
            Assert.IsTrue(questionService.PublicQuestions.Count == 2);

            questionService.SetCategorieFilter("CDF");

            Assert.IsTrue(questionService.PublicQuestions.Count == 1);
        }

        [TestMethod]
        public void GetQuestionWithRightTypeTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            object testQuestion1 = new YNQModel("Test", "ABC", 1);
            object testQuestion2 = new TLQModel("Test", "CDF", 1);
            object testQuestion3 = new OWQModel("Test", "ABC", 1);
            object testQuestion4 = new GAQModel("Test", "", 1, new String[] { "Test" });

            //Assert
            QuestionModel question1 = questionService.GetQuestionWithRightType(testQuestion1);
            Assert.AreEqual(question1.GetType(), typeof(YNQModel));

            QuestionModel question2 = questionService.GetQuestionWithRightType(testQuestion2);
            Assert.AreEqual(question2.GetType(), typeof(TLQModel));

            QuestionModel question3 = questionService.GetQuestionWithRightType(testQuestion3);
            Assert.AreEqual(question3.GetType(), typeof(OWQModel));

            QuestionModel question4 = questionService.GetQuestionWithRightType(testQuestion4);
            Assert.AreEqual(question4.GetType(), typeof(GAQModel));
        }

        [TestMethod]
        public void IsPublicQuestionsEmptyTest()
        {
            //Arrange
            IQuestionService questionService = new QuestionService(true);


            //Act
            QuestionModel testQuestion1 = new QuestionModel("Test", "ABC", 1);
            QuestionModel testQuestion2 = new QuestionModel("Test", "CDF", 1);
            QuestionModel testQuestion3 = new QuestionModel("Test", "ABC", 1);
            QuestionModel testQuestion4 = new QuestionModel("Test", "", 1);

            questionService.Questions.Add(testQuestion1);
            questionService.Questions.Add(testQuestion2);
            questionService.Questions.Add(testQuestion3);
            questionService.Questions.Add(testQuestion4);

            questionService.SetCategorieFilter("ABC");

            //Assert
            Assert.IsFalse(questionService.IsPublicQuestionsEmpty);

            questionService.SetCategorieFilter("....");

            Assert.IsTrue(questionService.IsPublicQuestionsEmpty);

            questionService.SetCategorieFilter("CDF");

            Assert.IsFalse(questionService.IsPublicQuestionsEmpty);

        }
    }
}
