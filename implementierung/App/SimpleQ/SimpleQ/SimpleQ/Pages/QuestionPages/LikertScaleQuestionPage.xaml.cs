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
		}

        private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            Slider slider = (Slider)sender;
            double roundValue = Math.Round(slider.Value);
            slider.Value = roundValue;
            ((LikertScaleQuestionPageModel)this.BindingContext).CurrentValue = (int)roundValue;
        }
    }
}