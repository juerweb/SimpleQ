using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface IDialogService
    {
        void ShowDialog(DialogType type, int errorCode);
    }
}
