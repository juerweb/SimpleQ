using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Literal
    {
        public static byte[] GetSHA512(this string str)
        {
            using (var alg = SHA512.Create())
            {
                return alg.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
        }

        public static TimeSpan NextMidnight
        {
            get => DateTime.Now.AddDays(1).Date - DateTime.Now;
        }
    }
}