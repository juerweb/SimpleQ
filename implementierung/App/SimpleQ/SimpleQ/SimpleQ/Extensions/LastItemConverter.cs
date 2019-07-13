using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Extensions
{
    class LastItemConverter : IValueConverter
    {
        public object Convert(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            IEnumerable<AnswerOption> items = value as IEnumerable<AnswerOption>;
            if (items != null)
            {
                return items.LastOrDefault().AnsText;
            }
            else return "";
        }

        public object ConvertBack(object value, System.Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new System.NotImplementedException();
        }
    }
}
