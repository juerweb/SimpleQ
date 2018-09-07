using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SimpleQ.Webinterface.Models.Startup))]

namespace SimpleQ.Webinterface.Models
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR();
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);
        }
    }
}
