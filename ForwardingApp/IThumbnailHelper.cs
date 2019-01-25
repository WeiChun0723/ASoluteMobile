using System;
namespace ASolute_Mobile
{
   
    public interface IThumbnailHelper
    {
        byte[] ResizeImage(byte[] imageData, float width, float height, int quality, bool filter);
    }
    
}
