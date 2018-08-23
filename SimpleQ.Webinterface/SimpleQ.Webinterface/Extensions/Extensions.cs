using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SimpleQ.Webinterface.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<T> TakeRandom<T>(this IEnumerable<T> src, int amount = 1)
        {
            if (amount > src.Count()) throw new ArgumentException();

            Random rnd = new Random();
            var list = src.ToList();
            for(int i = 0; i < amount; i++)
            {
                int idx = rnd.Next(amount);
                yield return list[idx];
                list.RemoveAt(idx);
            }
        }

        public static byte[] GetSHA512(this string str)
        {
            using (var alg = SHA512.Create())
            {
                return alg.ComputeHash(Encoding.UTF8.GetBytes(str));
            }
        }
    }
}