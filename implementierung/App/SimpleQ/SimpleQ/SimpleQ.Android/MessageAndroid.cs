using Android.App;
using Android.Widget;
using SimpleQ.Droid;
using SimpleQ.Extensions;
using Xamarin.Forms;

[assembly: Dependency(typeof(MessageAndroid))]
namespace SimpleQ.Droid
{
    public class MessageAndroid : IMessage
    {
        public void LongAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Long).Show();
        }
        public void ShortAlert(string message)
        {
            Toast.MakeText(Android.App.Application.Context, message, ToastLength.Short).Show();
        }
    }
}