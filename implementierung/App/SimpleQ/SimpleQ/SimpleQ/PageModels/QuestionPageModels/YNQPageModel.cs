using FreshMvvm;
using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the YNQPageModel for the Page xy.
    /// </summary>
    public class YNQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="YNQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public YNQPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YNQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public YNQPageModel()
        {
            YesCommand = new Command(()=>QuestionAnswered(YNQAnswer.Yes));
            NoCommand = new Command(() => QuestionAnswered(YNQAnswer.No));
        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            this.question = (YNQModel)initData;
            base.Init(initData);
        }
        #endregion

        #region Fields
        YNQModel question;
        #endregion

        #region Properties + Getter/Setter Methods
        public YNQModel Question { get => question; set => question = value; }
        #endregion

        #region Commands
        public Command YesCommand { get; private set; }
        public Command NoCommand { get; private set; }
        #endregion

        #region Methods
        private void QuestionAnswered(YNQAnswer answer)
        {
            Debug.WriteLine(String.Format("User answered the question with the id {0} with {1}...", Question.QuestionId, answer), "Info");
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
