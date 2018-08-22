using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.Models
{
    /// <summary>
    /// This is the MenuItemListModel for the xyPage
    /// </summary>
    public class MenuItemListModel : ObservableCollection<MenuItemModel>, INotifyPropertyChanged
    {
        #region Constructor(s)
        public MenuItemListModel(): base()
        {

        }

        public MenuItemListModel(String header): this()
        {
            this.header = header;
        }
        #endregion

        #region Fields
        private String header;
        #endregion

        #region Properties + Getter/Setter Methods
        public string Header { get => header; set => header = value; }
        public ObservableCollection<MenuItemModel> MenuItemModels { get => this; }
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
