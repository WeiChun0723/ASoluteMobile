using System;
using System.Collections.Generic;
using Xamarin.Forms;

namespace ASolute_Mobile.InputValidation
{
	public class MaxLengthValidation : Behavior<Entry>
    {
        public static readonly BindableProperty MaxLengthProperty = 
            BindableProperty.Create("MaxLength", typeof(int), typeof(MaxLengthValidation), 0);

        public int MaxLength
        {
            get { return (int)GetValue(MaxLengthProperty); }
            set { SetValue(MaxLengthProperty, value); }
        }

        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += bindable_TextChanged;
        }

        private void bindable_TextChanged(object sender, TextChangedEventArgs e)
        {
            string value = e.NewTextValue;

            if (e.NewTextValue.Length >= MaxLength)
            {
                 value = e.NewTextValue.Substring(0, MaxLength);
                ((Entry)sender).Unfocus();
            }

            ((Entry)sender).Text = value.ToUpper();
           
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;

        }
    }



}
