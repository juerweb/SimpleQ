using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace SimpleQ.PageModels.Commands
{
    public class ManualCodeCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
