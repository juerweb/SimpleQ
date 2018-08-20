using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the FAQModel for the FAQPage
    /// </summary>
    public class FAQModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="FAQModel"/> class.
        /// </summary>
        /// <param name="question">The question.</param>
        /// <param name="answer">The answer.</param>
        public FAQModel(string question, string answer): this()
        {
            this.question = question;
            this.answer = answer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAQModel"/> class.
        /// </summary>
        public FAQModel()
        {
            this.IsActive = false;
        }
        #endregion

        #region Fields
        /// <summary>
        /// The question
        /// </summary>
        private String question;
        /// <summary>
        /// The answer
        /// </summary>
        private String answer;
        /// <summary>
        /// Field, whichs shows the status of the faq.
        /// </summary>
        private bool isActive;

        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        /// <value>
        /// The question.
        /// </value>
        public string Question { get => question; set => question = value; }
        /// <summary>
        /// Gets or sets the answer.
        /// </summary>
        /// <value>
        /// The answer.
        /// </value>
        public string Answer { get => answer; set => answer = value; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive
        {
            get => isActive;
            set
            {
                isActive = value;
                OnPropertyChanged();
            }
        }
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
