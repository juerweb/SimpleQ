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
            Checkbox box = (Checkbox)sender;
            AnswerOption option = (AnswerOption)(box.BindingContext);

            PolytomousOMQuestionPageModel pageModel = (PolytomousOMQuestionPageModel)(this.BindingContext);
            pageModel.IsChecked[option] = box.IsChecked;

            if (box.IsChecked)
            {
                pageModel.IsQuestionAnswered = true;
            }
            else
            {
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

        }
    }
}