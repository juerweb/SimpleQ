using SimpleQ.PageModels.QuestionPageModels;
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
	public partial class LikertScaleQuestionPage : ContentPage
	{
		public LikertScaleQuestionPage()
		{
			InitializeComponent ();
            this.BindingContextChanged += LikertScaleQuestionPage_BindingContextChanged;

		}

        private void LikertScaleQuestionPage_BindingContextChanged(object sender, EventArgs e)
        {
            this.slider.MaxValue = (this.BindingContext as LikertScaleQuestionPageModel).Gradation;
        }
    }
}