using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SimpleQ.PageModels.Services
{
    public interface IDialogService
    {
        void ShowErrorDialog(int errorCode);
        void ShowDialog(String title, String body);

        Task<bool> ShowReallySureDialog();
    }
}
