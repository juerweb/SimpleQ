using IntelliAbb.Xamarin.Controls;
using SimpleQ.PageModels.QuestionPageModels;
using SimpleQ.Shared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimpleQ.Pages.QuestionPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class PolytomousOMQuestionPage : ContentPage
	{
		public PolytomousOMQuestionPage()
		{
			InitializeComponent ();
		}

        private void Checkbox_IsCheckedChanged(object sender, TappedEventArgs e)
        {
            PolytomousOMQuestionPageModel pageModel = (PolytomousOMQuestionPageModel)(this.BindingContext);
            foreach (Boolean b in pageModel.IsChecked.Values)
            {
                if (b)
                {
                    pageModel.IsQuestionAnswered = true;
                    return;
                }
            }
            pageModel.IsQuestionAnswered = false;
        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                KeyValuePair<AnswerOption, bool> keyValue = (KeyValuePair<AnswerOption, bool>)e.SelectedItem;
                PolytomousOMQuestionPageModel pageModel = (PolytomousOMQuestionPageModel)(this.BindingContext);
                pageModel.IsChecked[keyValue.Key] = true;
                listView.SelectedItem = null;
            }
        }
    }
}