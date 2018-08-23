using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the OWQModel for the xyPage
    /// </summary>
    public class OWQModel : QuestionModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public OWQModel(string questionDesc, string categorie, int questionId) : base(questionDesc, categorie, questionId, QuestionType.OWQ)
        {

        }

        public OWQModel()
        {

        }
        #endregion

        #region Fields
        private String answer;
        #endregion

        #region Properties + Getter/Setter Methods
        public string Answer { get => answer; set => answer = value; }
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
}
