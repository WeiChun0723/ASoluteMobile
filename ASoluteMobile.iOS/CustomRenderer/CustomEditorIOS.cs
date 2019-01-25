using ASolute_Mobile.CustomRenderer;
using CoreGraphics;
using HaulageApp.iOS;
using HaulageApp.iOS.CustomRenderer;
using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomEditor), typeof(CustomEditorIOS))]
namespace HaulageApp.iOS.CustomRenderer
{
    public class CustomEditorIOS : EditorRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Editor> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Layer.CornerRadius = 20;
                Control.Layer.BorderWidth = 3f;
                Control.Layer.BorderColor = Xamarin.Forms.Color.DeepPink.ToCGColor();
                Control.Layer.BackgroundColor = Xamarin.Forms.Color.DeepPink.ToCGColor();
        
            }
        }
    }
}
