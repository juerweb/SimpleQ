using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleQ.Validations
{
    public static class OneWordValidation
    {
        private const string oneWordCodeRegex = @"^[\S]+$";

        public static Boolean IsValid(String text)
        {
            return Regex.IsMatch(text, oneWordCodeRegex, RegexOptions.IgnoreCase);
        }
    }
}
