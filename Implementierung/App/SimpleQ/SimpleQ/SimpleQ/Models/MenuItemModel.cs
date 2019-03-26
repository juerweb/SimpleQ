using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the MenuItemModel.
    /// </summary>
    public class MenuItemModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public MenuItemModel(string title, FreshBasePageModel pageModelTyp, string iconResourceName = null)
        {
            this.Title = title;
            this.PageModelTyp = pageModelTyp;
            if (iconResourceName != null)
            {
                this.IconResourceName = ImageSource.FromFile(iconResourceName);
            }

        }
        #endregion

        #region Fields

        private string title;
        private FreshBasePageModel pageModelTyp;
        private ImageSource iconResourceName;

        #endregion

        #region Properties + Getter/Setter Methods
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }
        public FreshBasePageModel PageModelTyp { get => pageModelTyp; set => pageModelTyp = value; }

        public ImageSource IconResourceName
        {
            get => iconResourceName;
            set
            {
                iconResourceName = value;
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

    public enum ItemType
    {
        Navigation = 0,
        Filter = 1,
        Test = 2
    }
}
