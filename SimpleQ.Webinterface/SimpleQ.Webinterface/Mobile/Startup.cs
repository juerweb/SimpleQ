using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(SimpleQ.Webinterface.Mobile.Startup))]

namespace SimpleQ.Webinterface.Mobile
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();
            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(10);
        }
    }
}
