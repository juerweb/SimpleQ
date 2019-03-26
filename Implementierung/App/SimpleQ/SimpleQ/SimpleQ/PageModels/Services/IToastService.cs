using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.PageModels.Services
{
    public interface IToastService
    {
        void ShortMessage(String text);
        void LongMessage(String text);
    }
}
