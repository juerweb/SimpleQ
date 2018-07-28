using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SimpleQ.Webinterface
{
    public static class Extensions
    {
        public static string GetSHA512(this string str)
        {
            using (var alg = SHA512.Create())
            {
                alg.ComputeHash(Encoding.UTF8.GetBytes(str));
                return BitConverter.ToString(alg.Hash);
            }
        }
    }
}