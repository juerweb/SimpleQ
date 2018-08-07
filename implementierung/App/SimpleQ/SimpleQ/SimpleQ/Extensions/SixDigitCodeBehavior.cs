using SimpleQ.Validations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Xamarin.Forms;

namespace SimpleQ.Extensions
{
    /// <summary>
    /// It is a Behavior for the Entry-Field on the Registerpage.
    /// </summary>
    /// <seealso cref="Xamarin.Forms.Behavior{Xamarin.Forms.Entry}" />
    public class SixDigitCodeBehavior: Behavior<Entry>
    {
        static readonly BindablePropertyKey IsValidPropertyKey = BindableProperty.CreateReadOnly("IsValid", typeof(bool), typeof(SixDigitCodeBehavior), true);

        public static readonly BindableProperty IsValidProperty = IsValidPropertyKey.BindableProperty;

        public bool IsValid
        {
            get {
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
            this.IsValid = SixDigitCodeValidation.IsValid(e.NewTextValue);
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
