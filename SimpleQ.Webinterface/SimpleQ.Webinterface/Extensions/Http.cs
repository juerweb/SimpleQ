using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Http
    { 
        public static HttpStatusCodeResult Ok(string desc = null) => new HttpStatusCodeResult(200, desc);

        public static HttpStatusCodeResult BadRequest(string desc = null) => new HttpStatusCodeResult(400, desc);

        public static HttpStatusCodeResult Unauthorized(string desc = null) => new HttpStatusCodeResult(401, desc);

        public static HttpStatusCodeResult NotFound(string desc = null) => new HttpStatusCodeResult(404, desc);

        public static HttpStatusCodeResult Conflict(string desc = null) => new HttpStatusCodeResult(409, desc);

        public static HttpStatusCodeResult ServiceUnavailable(string desc = null) => new HttpStatusCodeResult(503, desc);



        // Just a little joke ;)
        public static HttpStatusCodeResult FourTwenty(string desc = null) => new HttpStatusCodeResult(420, "Get stoned");
    }
}