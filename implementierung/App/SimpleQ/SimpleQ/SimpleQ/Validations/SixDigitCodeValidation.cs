using Acr.UserDialogs;
using FreshMvvm;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleQ.Validations
{
    public static class SixDigitCodeValidation
    {
        private const string sixDigitCodeRegex = @"^[0-9,A-z]{6}[0-9,A-z]+$";

        public static Boolean IsValid(String code)
        {
            return Regex.IsMatch(code, sixDigitCodeRegex, RegexOptions.IgnoreCase);
        }
    }
}
