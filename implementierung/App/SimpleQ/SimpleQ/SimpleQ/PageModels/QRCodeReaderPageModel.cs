using FreshMvvm;
using SimpleQ.PageModels.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpleQ.PageModels
{
    /// <summary>
    /// This is the QRCodeReaderPageModel for the Page xy.
    /// </summary>
    public class QRCodeReaderPageModel : FreshBasePageModel, INotifyPropertyChanged
    {
        #region Constructor(s)
        public QRCodeReaderPageModel()
        {
            ScanCommand = new ScanQRCodeCommand();
        }
        #endregion

        #region Fields
        #endregion

        #region Properties + Getter/Setter Methods
        #endregion

        #region Commands
        public ScanQRCodeCommand ScanCommand
        {
            get;
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
