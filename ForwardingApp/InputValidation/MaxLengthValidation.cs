using System;
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
            if (e.NewTextValue.Length >= MaxLength)
                ((Entry)sender).Text = e.NewTextValue.Substring(0, MaxLength);

        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;

        }
    }

    public class MaximumDecimalPlace : Behavior<Entry>
    {
        public static readonly BindableProperty MaxLengthProperty =
            BindableProperty.Create("MaxDecimal", typeof(int), typeof(MaximumDecimalPlace), 0);

        public int MaxDecimal
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
            if(e.NewTextValue.Contains("."))
            {
                string[] value = e.NewTextValue.Split('.');
                string decimalValue = "";
                if(value[1].Length > MaxDecimal )
                {
                    decimalValue = value[1].Remove(value[1].Length - 1);
                }

                ((Entry)sender).Text = value[0] + decimalValue;
            }

        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= bindable_TextChanged;

        }
    }


}
