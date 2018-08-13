using SimpleQ.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace SimpleQ.Extensions
{
    public class ItemTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CategorieTemplate { get; set; }
        public DataTemplate NavigationTemplate { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            MainMenuItemModel itemModel = (MainMenuItemModel)item;

            if (itemModel.IconResourceName == null)
            {
                return CategorieTemplate;
            }
            else
            {
                return NavigationTemplate;
            }
        }
    }
}
