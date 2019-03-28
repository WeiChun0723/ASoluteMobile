using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.Utils
{
    public class CommonFunction
    {
        static ContentPage pages;
        static string returnResult;

        //call when calling the web service to get response
        public static async Task<string> GetRequestAsync(string baseAdd,string callUri)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseAdd);
            var uri = callUri;
            var response = await client.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);

            return content;
        }

        public static async Task<string> PostRequestAsync(object data, string baseAdd, string calllUri)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(baseAdd);
            var uri = calllUri;
            var content = JsonConvert.SerializeObject(data);
            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(uri, httpContent);
            var reply = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(reply);

            return reply;
        }

        public static  string ScanBarCode(ContentPage contentPage)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                contentPage.Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await contentPage.Navigation.PopAsync();
                        returnResult = result.Text;
                    });
                };
                return returnResult;
            }
            catch (Exception e)
            {
                contentPage.DisplayAlert("Error", e.Message, "OK");
            }
            return "";
        }


        //Get = 0 , Post = 1
        public static async Task<string> CallWebService(int method,object data, string baseAdd, string calllUri, Page page)
        {
            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(baseAdd);
                var uri = calllUri;
                var content = "";

                if (method == 0)
                {
                    var response = await client.GetAsync(uri);
                    content = await response.Content.ReadAsStringAsync();
                }
                else if (method == 1)
                {
                    var objectContent = JsonConvert.SerializeObject(data);
                    var httpContent = new StringContent(objectContent, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync(uri, httpContent);
                    content = await response.Content.ReadAsStringAsync();
                }

                Debug.WriteLine(content);

                clsResponse reply = JsonConvert.DeserializeObject<clsResponse>(content);

                if(reply.Message == "Invalid Session !")
                {
                    BackgroundTask.Logout(page);
                    await page.DisplayAlert("Error", reply.Message, "OK");
                }
                else
                {
                    return content;
                }
            }
            catch
            {

            }
            return "";
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

                byte[] thumbnailByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(imagesAsBytes, 256, 256, 100);
                string thumbnailFolder = HelperUtil.GetThumbnailFolder(image.photoFileLocation);
                if (!Directory.Exists(thumbnailFolder))
                {
                    Directory.CreateDirectory(thumbnailFolder);
                }
                image.photoThumbnailFileLocation = Path.Combine(thumbnailFolder, photoFileName);
                File.WriteAllBytes(image.photoThumbnailFileLocation, thumbnailByte);
                App.Database.SaveRecordImageAsync(image);
            }
            catch(Exception ex)
            {
                await contentPage.DisplayAlert("Reminder", ex.Message, "OK");
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
            catch
            {
                await contentPage.DisplayAlert("Reminder", "Please sign the job.", "OK");
            }

            
        }

        //prompt notification when receive new job notification
        public static void NewJobNotification(ContentPage page)
        {
            try
            {
                pages = page;

                var toastConfig = new ToastConfig("New job available in the job list.");
                toastConfig.SetDuration(6000);
                toastConfig.SetPosition(ToastPosition.Top);
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(12, 131, 193));
                UserDialogs.Instance.Toast(toastConfig);

                CreateToolBarItem(page);
            }
            catch
            {
                 pages.DisplayAlert("Error", "Notification error", "OK");
            }
        }


        //create notification icon in the bavigation bar
        public static void CreateToolBarItem(ContentPage contentPage)
        {
            pages = contentPage;
            contentPage.ToolbarItems.Clear();

            if (contentPage.ToolbarItems.Count == 0)
            {
                var item = new ToolbarItem
                {
                    Icon = "new_job.png",
                    Priority = 0,
                    Order = ToolbarItemOrder.Primary,

                };

                item.Clicked += async (sender, e) =>
                {
                    Ultis.Settings.NewJob = "No";
                    pages.ToolbarItems.Clear();
                    await pages.Navigation.PushAsync(new HaulageScreen.JobList("Job List"));
                };

                contentPage.ToolbarItems.Add(item);
            }

        }

    }

}
