using IntelliAbb.Xamarin.Controls;
using SimpleQ.Models;
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
            if (this.BindingContext != null)
            {
                PolytomousOMQuestionPageModel pageModel = (PolytomousOMQuestionPageModel)(this.BindingContext);
                foreach (IsCheckedModel<AnswerOption> model in pageModel.IsChecked)
                {
                    if (model.IsChecked)
                    {
                        pageModel.IsQuestionAnswered = true;
                        return;
                    }
                }
                pageModel.IsQuestionAnswered = false;
            }

        }

        private void ListView_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            ListView listView = (ListView)sender;
            if (listView.SelectedItem != null)
            {
                IsCheckedModel<AnswerOption> model = (IsCheckedModel<AnswerOption>)e.SelectedItem;
                PolytomousOMQuestionPageModel pageModel = (PolytomousOMQuestionPageModel)(this.BindingContext);
                model.IsChecked = !model.IsChecked;
                listView.SelectedItem = null;
            }
        }
    }
}