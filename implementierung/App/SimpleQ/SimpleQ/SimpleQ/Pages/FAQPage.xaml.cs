using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SimpleQ.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FAQPage : ContentPage
	{
		public FAQPage ()
		{
			InitializeComponent ();
		}

        public void UpdateSize()
        {
        }

        private void StackLayout_SizeChanged(object sender, EventArgs e)
        {
            var stackLayout = sender as StackLayout;
            if (stackLayout != null)
            {
                var childrenHeight = stackLayout.Children.Sum(c => c.Height) + stackLayout.Children.Count * stackLayout.Spacing;
                stackLayout.HeightRequest = childrenHeight;

                var viewCell = stackLayout.Parent as ViewCell;
                viewCell?.ForceUpdateSize();
            }
        }
    }
}