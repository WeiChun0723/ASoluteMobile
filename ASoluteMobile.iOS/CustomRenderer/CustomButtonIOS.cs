using ASolute_Mobile.CustomRenderer;
using HaulageApp.iOS.CustomRenderer;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomButton), typeof(CustomButtonIOS))]
namespace HaulageApp.iOS.CustomRenderer
{

    public class CustomButtonIOS : ButtonRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Button> e)
        {
            base.OnElementChanged(e);

            /*if (Control != null)
            {
                var button = (CustomButton)e.NewElement;

                button.SizeChanged += (s, args) =>
                {
                    var radius = Math.Min(button.Width, button.Height) / 2.0;
                    button.BorderRadius = (int)(radius);
                };
            }*/
        }
    }
}
