using FreshMvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the MainMenuItemModel for the xyPage
    /// </summary>
    public class MainMenuItemModel : INotifyPropertyChanged
    {
        #region Constructor(s)
        public MainMenuItemModel(int id, string title, Type pageModelTyp)
        {
            this.Id = id;
            this.Title = title;
            this.PageModelTyp = pageModelTyp;
        }
        #endregion

        #region Fields

        private int id;
        private string title;
        private Type pageModelTyp;

        #endregion

        #region Properties + Getter/Setter Methods
        public int Id { get => id; set => id = value; }
        public string Title { get => title; set => title = value; }
        public Type PageModelTyp { get => pageModelTyp; set => pageModelTyp = value; }
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
        Categorie = 1
    }
}
