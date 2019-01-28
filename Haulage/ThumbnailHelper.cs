using System;
using System.IO;
using Android.Graphics;
using Android.Media;
using ASolute_Mobile.Droid;
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
			// Load the bitmap
			Bitmap originalImage = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);

			float oldWidth = (float)originalImage.Width;
			float oldHeight = (float)originalImage.Height;
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

			Bitmap resizedImage = Bitmap.CreateScaledBitmap(originalImage, (int)newWidth, (int)newHeight, false);
           

            int scaled_width, scaled_height;
            scaled_height = resizedImage.Height;
            scaled_width = resizedImage.Width;

            Paint paint = new Paint();
            ColorMatrix cm = new ColorMatrix();
            cm.SetSaturation(0);
            ColorMatrixColorFilter f = new ColorMatrixColorFilter(cm);
            paint.SetColorFilter(f);

            using (MemoryStream ms = new MemoryStream())
            {
               resizedImage.Compress(Bitmap.CompressFormat.Jpeg, quality, ms);
               return ms.ToArray();
            }
                    
        }
    }
}
