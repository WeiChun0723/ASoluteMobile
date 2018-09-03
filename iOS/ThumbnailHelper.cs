using System;
using System.Drawing;
using System.IO;
using CoreGraphics;
using ASolute_Mobile.Droid;
using Microsoft.AppCenter.Crashes;
using UIKit;
using Xamarin.Forms;

[assembly: Dependency(typeof(ThumbnailHelper))]
namespace ASolute_Mobile.Droid
{
    public class ThumbnailHelper : IThumbnailHelper
    {
        public ThumbnailHelper()
        {
        }

		public byte[] ResizeImage(byte[] imageData, float width, float height, int quality)
		{
			UIImage originalImage = ImageFromByteArray(imageData);


			float oldWidth = (float)originalImage.Size.Width;
			float oldHeight = (float)originalImage.Size.Height;
			float scaleFactor = 0f;

			if (oldWidth > oldHeight)
			{
				scaleFactor = width / oldWidth;
			}
			else
			{
				scaleFactor = height / oldHeight;
			}

			float newHeight = oldHeight * scaleFactor;
			float newWidth = oldWidth * scaleFactor;

			//create a 24bit RGB image
			using (CGBitmapContext context = new CGBitmapContext(IntPtr.Zero,
				(int)newWidth, (int)newHeight, 8,
				(int)(4 * newWidth), CGColorSpace.CreateDeviceRGB(),
				CGImageAlphaInfo.PremultipliedFirst))
			{

				RectangleF imageRect = new RectangleF(0, 0, newWidth, newHeight);

				// draw the image
				context.DrawImage(imageRect, originalImage.CGImage);

				UIKit.UIImage resizedImage = UIKit.UIImage.FromImage(context.ToImage());

				// save the image as a jpeg
				return resizedImage.AsJPEG((float)quality).ToArray();
			}
		}

		public static UIKit.UIImage ImageFromByteArray(byte[] data)
		{
			if (data == null)
			{
				return null;
			}

			UIKit.UIImage image;
			try
			{
				image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
			}
			catch (Exception exception)
			{
				Console.WriteLine("Image load failed: " + exception.Message);
                Crashes.TrackError(exception);
                return null;
			}
			return image;
		}

	}
}
