using SimpleQ.Validations;
using Xamarin.Forms;

namespace SimpleQ.Extensions
{
    /// <summary>
    /// This class checks, if the user only enter one word on the OWQPage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Behavior{Xamarin.Forms.Entry}" />
    public class OneWordBehavior: Behavior<Entry>
    {
        static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(OneWordBehavior), true);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get
            {
                return (bool)base.GetValue(IsValidProperty);
            }
            private set { base.SetValue(IsValidPropertyKey, value); }
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.TextChanged += HandleTextChanged;
        }

        void HandleTextChanged(object sender, TextChangedEventArgs e)
        {
            this.IsValid = OneWordValidation.IsValid(e.NewTextValue);
            if (!this.IsValid)
            {
                ((Entry)sender).TextColor = (Color)Application.Current.Resources["colorWarning"];
            }
            else
            {
                ((Entry)sender).TextColor = Color.Default;
            }
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.TextChanged += HandleTextChanged;
        }
    }
}
