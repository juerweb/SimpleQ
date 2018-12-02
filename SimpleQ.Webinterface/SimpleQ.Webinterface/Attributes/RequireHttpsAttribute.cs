using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SimpleQ.Webinterface.Attributes
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS Required"
                };
            }
            else
            {
                var clientKey = actionContext.Request.Headers.Authorization.Parameter;
                var serverKey = File.ReadAllText(HostingEnvironment.MapPath(@"~\Authorization\private.key"));

                if (string.IsNullOrEmpty(clientKey) || clientKey != serverKey)
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Invalid client authorization key"
                    };
                }
                else
                {
                    base.OnAuthorization(actionContext);
                }
            }
        }
    }
}