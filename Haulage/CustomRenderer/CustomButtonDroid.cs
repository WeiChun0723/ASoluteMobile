using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ASolute_Mobile.CustomRenderer;
using ASoluteMobile.Droid.CustomRenderer;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonDroid))]
namespace ASoluteMobile.Droid.CustomRenderer
{
    public class CustomButtonDroid : ButtonRenderer
    {
        public CustomButtonDroid(Context context) : base(context)
        {
        }

        private GradientDrawable _normal, _pressed;

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.Button> e)
        {
            base.OnElementChanged(e);

            if (Control != null)
            {
                var button = (CustomButton)e.NewElement;

                button.SizeChanged += (s, args) =>
                {
                    var radius = (float)Math.Min(button.Width, button.Height);

                    // Create a drawable for the button's normal state
                    _normal = new Android.Graphics.Drawables.GradientDrawable();

                    if (button.BackgroundColor.R == -1.0 && button.BackgroundColor.G == -1.0 && button.BackgroundColor.B == -1.0)
                        _normal.SetColor(Android.Graphics.Color.ParseColor("#6495ed"));
                    else
                        _normal.SetColor(button.BackgroundColor.ToAndroid());

                    _normal.SetCornerRadius(radius);

                    // Create a drawable for the button's pressed state
                    _pressed = new Android.Graphics.Drawables.GradientDrawable();
                    var highlight = Context.ObtainStyledAttributes(new int[] { Android.Resource.Attribute.ColorActivatedHighlight }).GetColor(0, Android.Graphics.Color.MediumPurple);
                    _pressed.SetColor(highlight);
                    _pressed.SetCornerRadius(radius);

                    // Add the drawables to a state list and assign the state list to the button
                    var sld = new StateListDrawable();
                    sld.AddState(new int[] { Android.Resource.Attribute.StatePressed }, _pressed);
                    sld.AddState(new int[] { }, _normal);
                    Control.SetBackgroundDrawable(sld);
                };
            }
        }
    }
}