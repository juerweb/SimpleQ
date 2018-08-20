using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels.QuestionPageModels
{
    /// <summary>
    /// This is the OWQPageModel for the Page xy.
    /// </summary>
    public class OWQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="OWQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public OWQPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OWQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public OWQPageModel()
        {

        }


        /// <summary>
        /// Initializes the specified initialize data.
        /// </summary>
        /// <param name="initData">The initialize data.</param>
        public override void Init(object initData)
        {
            base.Init(initData);
        }
        #endregion

        #region Fields
        #endregion

        #region Properties + Getter/Setter Methods
        #endregion

        #region Commands
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
