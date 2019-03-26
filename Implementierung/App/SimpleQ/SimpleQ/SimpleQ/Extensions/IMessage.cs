using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.Extensions
{
    public interface IMessage
    {
        void LongAlert(string message);
        void ShortAlert(string message);
    }
}
