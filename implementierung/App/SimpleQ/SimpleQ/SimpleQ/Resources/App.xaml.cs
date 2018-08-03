using SimpleQ.Pages;
using SimpleQ.Views;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System.Reflection;

[assembly: XamlCompilation (XamlCompilationOptions.Compile)]
namespace SimpleQ
{
	public partial class App : Application
	{
		public App ()
		{

            InitializeComponent();

            MainPage = new RegisterPage();
        }

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
