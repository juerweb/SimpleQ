using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Helper
    {
        public static byte[] GetSHA512(this string str)
        {
            using (var alg = SHA512.Create())
            {
                return alg.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
        }

        public static DateTime Tomorrow
        {
            get => DateTime.Today.AddDays(1);
        }

        public static TimeSpan UntilNextMidnight
        {
            get => DateTime.Now.AddDays(1).Date - DateTime.Now;
        }

        public static DateTime NextDateTime(int hour, int minute)
        {
            return DateTime.Today.AddHours(hour).AddMinutes(minute) < DateTime.Now
                ? Tomorrow.AddHours(hour).AddMinutes(minute)
                : DateTime.Today.AddHours(hour).AddMinutes(minute);
        }

        public static string RandomString(int count)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var rnd = new Random();

            return new string(chars.OrderBy(c => rnd.Next()).Take(count).ToArray());
        }
    }
}