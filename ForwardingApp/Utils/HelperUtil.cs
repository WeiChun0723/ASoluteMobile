using System;
using System.IO;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute.Mobile.Models;

namespace ASolute_Mobile.Ultis
{
    public class HelperUtil
    {
        public HelperUtil()
        {
        }

        public static string GetScaledFolder(string fileLocation)
        {
            string currentFolderlocation = GetCurrentFolderLocation(fileLocation);
            return Path.Combine(currentFolderlocation, "scaled");
        }

        public static string GetThumbnailFolder(string fileLocation)
        {
            string currentFolderlocation = GetCurrentFolderLocation(fileLocation);
            return Path.Combine(currentFolderlocation, "thumb");
        }

        public static string GetThumbnailFile(string fileLocation)
        {
            string currentFolderlocation = GetCurrentFolderLocation(fileLocation);
            string fileName = GetPhotoFileName(fileLocation);

            return Path.Combine(GetThumbnailFolder(fileLocation), fileName);
        }

        public static string GetPhotoFileName(String filePath)
        {
            return filePath.Substring(filePath.LastIndexOf('/') + 1);
        }

        public static string GetCurrentFolderLocation(string fullFilePath)
        {
            return fullFilePath.Substring(0, fullFilePath.LastIndexOf('/'));
        }

    }
}
