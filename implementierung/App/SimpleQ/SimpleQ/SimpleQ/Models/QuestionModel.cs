using System;
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
        public QuestionModel(String questionDesc, String categorie, int questionId)
        {
            this.questionDesc = questionDesc;
            this.categorie = categorie;
            this.questionId = questionId;
        }
        #endregion

        #region Fields
        private int questionId;
        private String questionDesc;
        private String categorie;
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
        #endregion

        #region Methods
        public static QuestionModel GetQuestionWithRightType(object question)
        {
            if (question.GetType() == typeof(YNQModel))
            {
                //YNQModel
                return (YNQModel)question;
            }
            else if (question.GetType() == typeof(TLQModel))
            {
                return (TLQModel)question;
            }
            else if (question.GetType() == typeof(OWQModel))
            {
                return (OWQModel)question;
            }
            else if (question.GetType() == typeof(GAQModel))
            {
                return (GAQModel)question;
            }
            else
            {
                return null;
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
