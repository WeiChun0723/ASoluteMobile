using ASolute_Mobile.CustomRenderer;
using CoreAnimation;
using CoreGraphics;
using HaulageApp.iOS;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CustomEntry), typeof(CustomEntryIOS))]
namespace HaulageApp.iOS
{
    public class CustomEntryIOS :EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement == null)
            {
                Control.Layer.CornerRadius = 20;
                Control.Layer.BorderWidth = 3f;
                Control.Layer.BorderColor = Xamarin.Forms.Color.DeepPink.ToCGColor();
                Control.Layer.BackgroundColor = Xamarin.Forms.Color.DeepPink.ToCGColor();

                Control.LeftView = new UIKit.UIView(new CGRect(0, 0, 10, 0));
                Control.LeftViewMode = UIKit.UITextFieldViewMode.Always;

                var element = (CustomEntry)this.Element;

                var textField = this.Control;
                if (!string.IsNullOrEmpty(element.Image))
                {
                    switch (element.ImageAlignment)
                    {
                        case ImageAlignment.Left:
                            textField.LeftViewMode = UITextFieldViewMode.Always;
                            textField.LeftView = GetImageView(element.Image, element.ImageHeight, element.ImageWidth);
                            break;
                        case ImageAlignment.Right:
                            textField.RightViewMode = UITextFieldViewMode.Always;
                            textField.RightView = GetImageView(element.Image, element.ImageHeight, element.ImageWidth);
                            break;
                    }
                }

                textField.BorderStyle = UITextBorderStyle.None;
                CALayer bottomBorder = new CALayer
                {
                    Frame = new CGRect(0.0f, element.HeightRequest - 1, this.Frame.Width, 1.0f),
                    BorderWidth = 2.0f,
                    BorderColor = element.LineColor.ToCGColor()
                };

                textField.Layer.AddSublayer(bottomBorder);
                textField.Layer.MasksToBounds = true;
            }

        }


        private UIView GetImageView(string imagePath, int height, int width)
        {
            var uiImageView = new UIImageView(UIImage.FromBundle(imagePath))
            {
                Frame = new RectangleF(0, 0, width, height)
            };
            UIView objLeftView = new UIView(new System.Drawing.Rectangle(0, 0, width + 10, height));
            objLeftView.AddSubview(uiImageView);

            return objLeftView;
        }
    }
}
