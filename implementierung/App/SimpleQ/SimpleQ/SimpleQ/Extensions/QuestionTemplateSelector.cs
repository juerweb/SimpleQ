using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Extensions
{
    public class QuestionTemplateSelector : DataTemplateSelector
    {
        public DataTemplate YNQTemplate { get; set; }
        public DataTemplate TLQTemplate { get; set; }
        public DataTemplate OWQTemplate { get; set; }
        public DataTemplate GAQTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            return YNQTemplate;
        }
    }
}
