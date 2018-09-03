using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomRenderer
{
    public class CustomSwitch : Switch
    {
        public static readonly BindableProperty SwitchOffColorProperty =
     BindableProperty.Create(nameof(SwitchOffColor),
         typeof(Color), typeof(CustomSwitch),
         Color.Default);

        public Color SwitchOffColor
        {
            get { return (Color)GetValue(SwitchOffColorProperty); }
            set { SetValue(SwitchOffColorProperty, value); }
        }

        public static readonly BindableProperty SwitchOnColorProperty =
          BindableProperty.Create(nameof(SwitchOnColor),
              typeof(Color), typeof(CustomSwitch),
              Color.Default);

        public Color SwitchOnColor
        {
            get { return (Color)GetValue(SwitchOnColorProperty); }
            set { SetValue(SwitchOnColorProperty, value); }
        }

        public static readonly BindableProperty SwitchThumbColorProperty =
          BindableProperty.Create(nameof(SwitchThumbColor),
              typeof(Color), typeof(CustomSwitch),
              Color.Default);

        public Color SwitchThumbColor
        {
            get { return (Color)GetValue(SwitchThumbColorProperty); }
            set { SetValue(SwitchThumbColorProperty, value); }
        }

        public static readonly BindableProperty SwitchThumbImageProperty =
          BindableProperty.Create(nameof(SwitchThumbImage),
              typeof(string),
              typeof(CustomSwitch),
              string.Empty);

        public string SwitchThumbImage
        {
            get { return (string)GetValue(SwitchThumbImageProperty); }
            set { SetValue(SwitchThumbImageProperty, value); }
        }
    }
}
