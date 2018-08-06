using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Input;

namespace SimpleQ.PageModels.Commands
{
    public class ScanQRCodeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Debug.WriteLine("Execute ScanQRCodeCommand", "Info");
            RegisterPageModel pageModel = (RegisterPageModel)parameter;

            //Start QR Code Reader

        }
    }
}
