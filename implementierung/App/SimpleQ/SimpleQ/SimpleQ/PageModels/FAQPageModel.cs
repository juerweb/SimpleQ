using FreshMvvm;
using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Linq;
using SimpleQ.Resources;
using SimpleQ.Pages;
using System.Diagnostics;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the FAQPageModel for the FAQPage.
    /// </summary>
    public class FAQPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        /// <summary>
        /// Initializes a new instance of the <see cref="FAQPageModel"/> class.
        /// With Parameter like Services
        /// </summary>
        /// <param name="param">The parameter.</param>
        public FAQPageModel(object param): this()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAQPageModel"/> class.
        /// Without Parameter
        /// </summary>
        public FAQPageModel()
        {
            faqs = new ObservableCollection<FAQModel>();
            faqs.Add(new FAQModel("Question1", AppResources.LoremIpsum));
            faqs.Add(new FAQModel("Question2", AppResources.LoremIpsum));
            faqs.Add(new FAQModel("Question3", AppResources.LoremIpsum));
            faqs.Add(new FAQModel("Question4", AppResources.LoremIpsum));
            faqs.Add(new FAQModel("Question5", AppResources.LoremIpsum));
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
        /// <summary>
        /// The selected FAQ
        /// </summary>
        private FAQModel selectedFAQ;
        /// <summary>
        /// The faqs Collection
        /// </summary>
        private ObservableCollection<FAQModel> faqs;
        #endregion

        #region Properties + Getter/Setter Methods
        /// <summary>
        /// Gets or sets the selected FAQ.
        /// </summary>
        /// <value>
        /// The selected FAQ.
        /// </value>
        public FAQModel SelectedFAQ
        {
            get => selectedFAQ;
            set
            {   
                selectedFAQ = value;
                OnPropertyChanged();



                if (selectedFAQ != null)
                {
                    OpenSelectedFAQ(selectedFAQ.IsActive);
                }

            }
        }

        /// <summary>
        /// Gets or sets the faqs Collection
        /// </summary>
        /// <value>
        /// The faqs collection
        /// </value>
        public ObservableCollection<FAQModel> FAQs
        {
            get => faqs;
            set
            {
                faqs = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        #endregion

        #region Methods
        /// <summary>
        /// Disables all active FAQs
        /// </summary>
        private void DisableAllActiveFAQs()
        {
            FAQs.Where(faq => faq.IsActive == true).ToList().ForEach(faq => faq.IsActive = false);
        }

        /// <summary>
        /// Opens the selected FAQ.
        /// </summary>
        private void OpenSelectedFAQ(Boolean isItTheSame)
        {
            DisableAllActiveFAQs();
            if (!isItTheSame)
            {
                this.selectedFAQ.IsActive = true;
            }
            SelectedFAQ = null;

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
