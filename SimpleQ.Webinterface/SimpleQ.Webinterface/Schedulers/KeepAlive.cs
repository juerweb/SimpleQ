using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Web;

namespace SimpleQ.Webinterface.Schedulers
{
    public class KeepAlive : IDisposable
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private EventWaitHandle waitHandle = new AutoResetEvent(false);
        private readonly object lck = new object();
        private bool cancelled = false;
        private string url;

        public KeepAlive()
        {
            url = ConfigurationManager.AppSettings["SiteUrl"];
        }

        public void Start()
        {
            cancelled = false;
            Thread t = new Thread(Execute);
            t.Start();
        }

        public void KeepAliveRequest()
        {
            logger.Debug("Performing keep-alive request");
            Request();
        }

        private void Request()
        {
            try
            {
                HttpClient http = new HttpClient();
                var result = http.GetAsync(url).Result;
                logger.Trace($"Performed GET request to {url} with status code {result.StatusCode}");
            }
            catch (Exception ex)
            {
                logger.Warn(ex, $"Request to {url} failed");
            }
        }

        public void Stop()
        {
            lock (lck)
            {
                if (cancelled) return;
                cancelled = true;
                waitHandle.Set();
            }
        }

        private void Execute()
        {
            while (!cancelled)
            {
                Request();
                waitHandle.WaitOne(TimeSpan.FromMinutes(3), true);
            }
        }

        #region IDisposable Members
        public void Dispose()
        {
            Stop();
        }
        #endregion
    }
}