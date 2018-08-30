using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface IDialogService
    {
        void ShowErrorDialog(int errorCode);
        void ShowDialog(String title, String body);
    }
}
