﻿using System;
using System.Collections.Generic;
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
            Console.WriteLine("Button gedrückt!");
        }
    }
}