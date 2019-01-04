using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Web;
using System.Web.Hosting;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace SimpleQ.Webinterface.Attributes
{
    public class RequireAuthAttribute : AuthorizationFilterAttribute
    {
        private Logger logger = LogManager.GetCurrentClassLogger();

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            logger.Debug("Authorization entered");
            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
            {
                logger.Debug("Non-HTTPS request");
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "HTTPS Required"
                };
            }
            else
            {
                //var store = new X509Store(StoreLocation.CurrentUser);
                //store.Open(OpenFlags.ReadOnly);

                //var clientCert = actionContext.Request.GetClientCertificate();
                //if (clientCert == null)
                //{
                //    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                //    {
                //        ReasonPhrase = "Client Certificate Required"
                //    };

                //}
                //else
                //{
                //    var certs = store.Certificates.Find(X509FindType.FindByThumbprint, clientCert.Thumbprint, true);
                //    if (certs.Count == 0)
                //    {
                //        actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                //        {
                //            ReasonPhrase = "Invalid Client Certificate"
                //        };
                //    }
                //    else
                //    {
                //        base.OnAuthorization(actionContext);
                //        logger.Debug("Request authorized");
                //    }
                //}
                //store.Close();

                var clientKey = actionContext.Request.Headers.Authorization.Parameter;
                var serverKey = File.ReadAllText(HostingEnvironment.MapPath(@"~\Authorization\private.key"));

                if (string.IsNullOrEmpty(clientKey) || clientKey != serverKey)
                {
                    logger.Debug("Invalid client authorization key");
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Invalid client authorization key"
                    };
                }
                else
                {
                    base.OnAuthorization(actionContext);
                    logger.Debug("Request authorized");
                }
            }
        }
    }
}