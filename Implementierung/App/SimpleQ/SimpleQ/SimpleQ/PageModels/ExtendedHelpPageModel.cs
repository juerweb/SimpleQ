using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the ExtendedHelpPageModel for the ExtendedHelpPage
    /// </summary>
    public class ExtendedHelpPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedHelpPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public ExtendedHelpPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtendedHelpPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public ExtendedHelpPageModel()
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
