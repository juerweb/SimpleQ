﻿using FreshMvvm;
using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    /// <summary>
    /// This is the QuestionService.
    /// </summary>
    public class QuestionService : IQuestionService
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public QuestionService(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QuestionService"/> class.
        /// Without Parameter
        /// </summary>
        public QuestionService()
        {
            Questions = new ObservableCollection<QuestionModel>();
            AnsweredQuestions = new List<QuestionModel>();

            this.AddQuestion(new YNQModel("Sind Sie männlich?", "YNQ Test", 0));
            this.AddQuestion(new TLQModel("Sind Sie anwesend?", "TLQ Test", 1));
            this.AddQuestion(new OWQModel("Beschreiben Sie sich mit einem Wort oder doch mit zwei oder vielleicht nur mit einem. O.k. bitte nur mit einem Wort beschreiben!", "OWQ Test", 2));
            this.AddQuestion(new GAQModel("Was ist Ihre Lieblingsfarbe?", "GAQ Test", 1, new String[] { "Grün", "Rot", "Gelb", "Blau" }));
        }
        #endregion

        #region Fields
        private ObservableCollection<QuestionModel> questions;
        private List<QuestionModel> answeredQuestions;
        #endregion

        #region Properties + Getter/Setter Methods
        public ObservableCollection<QuestionModel> Questions { get => questions; private set => questions = value; }
        public List<QuestionModel> AnsweredQuestions { get => answeredQuestions; set => answeredQuestions = value; }
        #endregion

        #region Commands
        #endregion

        #region Methods
        public void QuestionAnswered(QuestionModel question)
        {
            Debug.WriteLine("Question Service with question from type: " + question.GetType(), "Info");
        }

        public void AddQuestion(QuestionModel question)
        {
            this.questions.Add(question);
            if (!(App.MainMasterPageModel.MenuItems[0].Count(menuItem => menuItem.Title == question.Categorie) > 0))
            {
                //categorie does not exists

                App.MainMasterPageModel.AddPage(question.Categorie, ItemType.Categorie, new Test1PageModel(), null);
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        #endregion
    }
}
