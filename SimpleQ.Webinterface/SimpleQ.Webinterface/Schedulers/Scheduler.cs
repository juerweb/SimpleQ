using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace SimpleQ.Webinterface.Schedulers
{
    public abstract class Scheduler
    {
        protected static Logger _logger;
        protected bool schedulerStarted = false;
        protected readonly object lck = new object();

        protected Logger logger => _logger ?? (_logger = LogManager.GetLogger(GetType().FullName));
        protected abstract string Name { get; }

        public void Start()
        {
            try
            {
                logger.Debug($"Starting {Name}");
                lock (lck)
                {
                    if (schedulerStarted)
                    {
                        logger.Debug($"{Name} running already");
                        return;
                    }

                    schedulerStarted = true;
                }

                HostingEnvironment.QueueBackgroundWorkItem(ct =>
                {
                    while (true)
                    {
                        Schedule();
                    }
#pragma warning disable CS0162 // Unreachable code detected
                    lock (lck)
                    {
                        schedulerStarted = false;
                    }
                    logger.Fatal($"{Name} crashed");
#pragma warning restore CS0162 // Unreachable code detected
                });
                logger.Debug($"{Name} started successfully");
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Start: Unexpected error");
                throw ex;
            }
        }

        protected abstract void Schedule();
    }
}