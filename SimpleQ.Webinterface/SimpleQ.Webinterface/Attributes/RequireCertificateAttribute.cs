using System;
using System.Collections.Generic;
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
    public class RequireCertificateAttribute : AuthorizationFilterAttribute
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
                var clientCert = actionContext.Request.GetClientCertificate();
                var cert = new X509Certificate2(HostingEnvironment.MapPath(@"~\Certificates\SimpleQ.cer"), "123SimpleQ...");

                if (clientCert == null || clientCert.Thumbprint != cert.Thumbprint)
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Invalid client certificate"
                    };
                }

                base.OnAuthorization(actionContext);
            }
        }
    }
}