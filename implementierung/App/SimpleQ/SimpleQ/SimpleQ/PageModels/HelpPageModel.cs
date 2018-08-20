using FreshMvvm;
using SimpleQ.Models;
using SimpleQ.Resources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the HelpPageModel for the HelpPage.
    /// </summary>
    public class HelpPageModel : StandardMenuPageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="HelpPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public HelpPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HelpPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public HelpPageModel(): base()
        {
            MenuItems.Add(new MenuItemModel(AppResources.FAQ, new FAQPageModel(), "ic_help_black_18.png"));
            MenuItems.Add(new MenuItemModel(AppResources.ExtendedHelpInternet, new ExtendedHelpPageModel(), "ic_public_black_18.png"));
            MenuItems.Add(new MenuItemModel(AppResources.About, new AboutPageModel(), "ic_info_black_18.png"));
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
        //The Implemenation of the INotifyPropertyChanged is in the Base Class
        #endregion
    }
}
