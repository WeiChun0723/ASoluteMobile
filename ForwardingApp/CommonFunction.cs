using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ASolute_Mobile.Utils
{
    public class CommonFunction
    {
        public CommonFunction()
        {

        }

        //capture image and store local path in db function
        public static async Task StoreImages(string id, ContentPage contentPage)
        {
            try
            {
                await CrossMedia.Current.Initialize();

                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
                {
                    //content page refer to the page that call this function (this)
                    await contentPage.DisplayAlert("No Camera", "No camera available", "OK");
                    return;
                }

                var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions { Directory = "App_Image" });

                if (file == null)
                    return;

                AppImage image = new AppImage();
                image.id = id;
                image.photoFileLocation = file.Path;
                image.Uploaded = false;

                string photoFileName = HelperUtil.GetPhotoFileName(image.photoFileLocation);

                image.photoFileName = photoFileName;
                image.type = "NormalImage";
                byte[] imagesAsBytes;
                using (var memoryStream = new MemoryStream())
                {
                    file.GetStream().CopyTo(memoryStream);
                    file.Dispose();
                    imagesAsBytes = memoryStream.ToArray();
                }

                //resize the photo and store in different directory 
                byte[] thumbnailByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(imagesAsBytes, 256, 256, 100,false);
                string thumbnailFolder = HelperUtil.GetThumbnailFolder(image.photoFileLocation);
                if (!Directory.Exists(thumbnailFolder))
                {
                    Directory.CreateDirectory(thumbnailFolder);
                }
                image.photoThumbnailFileLocation = Path.Combine(thumbnailFolder, photoFileName);
                File.WriteAllBytes(image.photoThumbnailFileLocation, thumbnailByte);
                App.Database.SaveRecordImageAsync(image);
            }
            catch(Exception exception)
            {
                await contentPage.DisplayAlert("Reminder", exception.Message, "OK");
            }
        }

        //covert signature to image and store in local db
        public static async Task StoreSignature(string id,Stream signature, ContentPage contentPage)
        {
            try
            {
                byte[] signatureAsBytes;

                using (var memoryStream = new MemoryStream())
                {
                    signature.CopyTo(memoryStream);
                    signature.Dispose();
                    signatureAsBytes = memoryStream.ToArray();
                }

                string systemDate = DateTime.Now.ToShortDateString();
                string[] date = systemDate.Split('/');
                string customDate = date[2].ToString() + date[1].ToString() + date[0].ToString();

                Random random = new Random();
                int randomID = random.Next(1000000);

                AppImage captureSignature = new AppImage();
                captureSignature.id = id;
                captureSignature.imageData = signatureAsBytes;
                captureSignature.photoFileLocation = "";
                captureSignature.photoFileName = "SIGN_" + customDate + "_" + randomID + ".png";
                captureSignature.photoThumbnailFileLocation = "";
                captureSignature.photoScaledFileLocation = "";
                captureSignature.scaleResolution = 0;
                captureSignature.Uploaded = false;
                captureSignature.type = "signature";
                App.Database.SaveRecordImageAsync(captureSignature);
            }
            catch(Exception exception)
            {
                await contentPage.DisplayAlert("Signature error", "Please sign for the job", "OK");
            }
            
        }

        public static void AppActivity(string update, string status, string message)
        {
            ActivityLog history = new ActivityLog();

            if (Ultis.Settings.Language.Equals("English"))
            {
                history.activity = update;
                history.status = "Status: " + status;
                history.message = "Message: " + message;
            }
            else
            {
                history.activity = update;
                history.status = "Status: " + status;
                history.message = "Mesej: " + message;
            }
            
            App.Database.SaveActivity(history);
        }

    }

}
