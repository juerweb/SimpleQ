using System;
using SimpleQ.Droid;
using NLog;
using SimpleQ.Logging;
using Xamarin.Forms;

[assembly: Dependency(typeof(NLogLogger))]
namespace SimpleQ.Droid
{
    public class NLogLogger : SimpleQ.Logging.ILogger
    {
        private Logger log;

        public NLogLogger(Logger log)
        {
            this.log = log;
        }

        public void Debug(string text, params object[] args)
        {
            log.Debug(text, args);
        }

        public void Error(string text, params object[] args)
        {
            log.Error(text, args);
        }

        public void Fatal(string text, params object[] args)
        {
            log.Fatal(text, args);
        }

        public void Info(string text, params object[] args)
        {
            log.Info(text, args);
        }

        public void Trace(string text, params object[] args)
        {
            log.Trace(text, args);
        }

        public void Warn(string text, params object[] args)
        {
            log.Warn(text, args);
        }
    }
}