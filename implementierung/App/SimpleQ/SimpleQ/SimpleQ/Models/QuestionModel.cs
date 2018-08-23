﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the Base Class for all Questions.
    /// </summary>
    public class QuestionModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public QuestionModel(String questionDesc, String categorie, int questionId, QuestionType questionType): this()
        {
            this.questionDesc = questionDesc;
            this.categorie = categorie;
            this.questionId = questionId;
            this.questionType = questionType;
        }

        public QuestionModel()
        {

        }
        #endregion

        #region Fields
        private int questionId;
        private String questionDesc;
        private String categorie;
        private QuestionType questionType;
        #endregion

        #region Properties + Getter/Setter Methods
        public int QuestionId
        {
            get => questionId;
            set { questionId = value; OnPropertyChanged(); }

        }
        public string QuestionDesc
        {
            get => questionDesc;
            set { questionDesc = value; OnPropertyChanged(); }

        }
        public string Categorie
        {
            get => categorie;
            set { categorie = value; OnPropertyChanged(); }
            
        }

        public QuestionType QuestionType { get => questionType; set => questionType = value; }
        #endregion

        #region Methods
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public enum QuestionType
    {
        YNQ = 0,
        TLQ = 1,
        OWQ = 2,
        GAQ = 3
    }
}
