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

        public static HttpStatusCodeResult Forbidden(string desc = null) => new HttpStatusCodeResult(403, desc);

        public static HttpStatusCodeResult NotFound(string desc = null) => new HttpStatusCodeResult(404, desc);

        public static HttpStatusCodeResult Conflict(string desc = null) => new HttpStatusCodeResult(409, desc);

        public static HttpStatusCodeResult InternalServerError(string desc = null) => new HttpStatusCodeResult(500, desc);

        public static HttpStatusCodeResult ServiceUnavailable(string desc = null) => new HttpStatusCodeResult(503, desc);



        // Just for fun
        public static HttpStatusCodeResult FourTwenty(string desc = null) => new HttpStatusCodeResult(420, "Get stoned");
        public static HttpStatusCodeResult SixSixSix(string desc = null) => new HttpStatusCodeResult(666, "Possessed");
    }
}