using NLog;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleQ.Logging
{
    public interface ILogManager
    {
        ILogger GetLog([System.Runtime.CompilerServices.CallerFilePath]string callerFilePath = "");
    }
}
