using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Foundation;
using SimpleQ.Extensions;
using UIKit;

namespace SimpleQ.iOS
{
    public class CloseApplication : ICloseApplication
    {
        void ICloseApplication.CloseApplication()
        {
            Thread.CurrentThread.Abort();
        }
    }
}