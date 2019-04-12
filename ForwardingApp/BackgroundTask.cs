using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Models;
using ASolute_Mobile.Data;
using ASolute.Mobile.Models;
using Plugin.Geolocator;
using Newtonsoft.Json.Linq;


namespace ASolute_Mobile
{
    public class BackgroundTask
    {
        static byte[] scaledImageByte;
        static string imageEventID;
        static List<AppImage> recordImages = new List<AppImage>();
        static Plugin.Geolocator.Abstractions.Position position = null;
        static string address = "";
        static string location = "0,0";

        public static async Task GetGPS()
        {
            Getlocation();

            if(App.gpsLocationLat.Equals(0) || App.gpsLocationLong.Equals(0))
            {
                if (position != null)
                {
                    location = String.Format("{0:0.000000}", position.Latitude) + "," + String.Format("{0:0.000000}", position.Longitude);
                }
                else
                {
                    location = "0,0";
                }

            }
            else
            {
                location = String.Format("{0:0.000000}", App.gpsLocationLat) + "," + String.Format("{0:0.000000}", App.gpsLocationLong);
            }

            if(!(String.IsNullOrEmpty(Ultis.Settings.SessionSettingKey)) && NetworkCheck.IsInternet())
            {
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var sendGPSURI = ControllerUtil.getGPSTracking(location, address);
                    var content = await client.GetAsync(sendGPSURI);
                    var response = await content.Content.ReadAsStringAsync();
                    clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(response);
                    Debug.WriteLine(response);
                }
                catch
                {

                }
               
            }
        }

        public static async void Getlocation()
        {
            try
            {
                var locator = CrossGeolocator.Current;
                position = await locator.GetPositionAsync();

                if (position.Equals(null))
                {
                    position = await locator.GetLastKnownLocationAsync();

                    /*var getAddress = await locator.GetAddressesForPositionAsync(position);
                    var addressDetail = getAddress.FirstOrDefault();


                    address = addressDetail.Thoroughfare;

                    if (!String.IsNullOrEmpty(addressDetail.Locality) && addressDetail.Locality != "????")
                    {
                        address += "," + addressDetail.Locality;
                    }

                    if (!String.IsNullOrEmpty(addressDetail.PostalCode) && addressDetail.PostalCode != "????")
                    {
                        address += "," + addressDetail.PostalCode;
                    }

                    if (!String.IsNullOrEmpty(addressDetail.AdminArea) && addressDetail.AdminArea != "????")
                    {
                        address += "," + addressDetail.AdminArea;
                    }

                    if (!String.IsNullOrEmpty(addressDetail.CountryName) && addressDetail.CountryName != "????")
                    {
                        address += "," + addressDetail.CountryName;
                    }*/
                }

            }
            catch (Exception ex)
            {
                // Unable to get location
            }
        }
       
        public static void Logout()
        {
            Logout(null);
        }

        public static void Logout(Page contentPage)
        {
            if (contentPage != null)
            {
                contentPage.IsBusy = true;
            }

            App.Database.DeleteUserImage(Ultis.Settings.SessionUserItem.DriverId);
            Ultis.Settings.SessionSettingKey = "";
            Ultis.Settings.Language = "";
            
            if (contentPage != null)
            {
                contentPage.IsBusy = false;
            }
            if (contentPage != null)
            {
                MainPage mainPage = GetMainPage(contentPage);

                if (mainPage != null)
                {
                    mainPage.Detail = new CustomNavigationPage(new LoginPage());
                    mainPage.IsPresented = false;
                }
                else
                {
                    throw new Exception("Wrong implementation.");
                }
            }
        }

        private static MainPage GetMainPage(Page page)
        {
            if (page.GetType() == typeof(MainPage))
            {
                return (MainPage)page;
            }
            return GetMainPage((Xamarin.Forms.Page)page.Parent);
        }

        public static void StartTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(5),  () =>
            {

                //Task.Run(async () => { await BackgroundUploadImage(); });

                BackgroundUploadImage();

                if (recordImages.Count != 0)
                {

                    return true; // True = Repeat again, False = Stop the timer
                }
                else
                {
                    return false;
                }

            });
        }


        public static async void BackgroundUploadImage()
        {
           
            try
            {
                recordImages = App.Database.GetPendingRecordImages(false);
                foreach (AppImage recordImage in recordImages)
                {
                    if(recordImage.type != "ProfilePic")
                    {
                        clsFileObject image = new clsFileObject();

                        if (recordImage.type == "signature")
                        {
                            image.Content = recordImage.imageData;
                        }
                        else
                        {
                            byte[] originalPhotoImageBytes = File.ReadAllBytes(recordImage.photoFileLocation);
                            scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100);
                            image.Content = scaledImageByte;
                        }

                        image.FileName = recordImage.photoFileName;

                        string eventID;

                        if (!(String.IsNullOrEmpty(imageEventID)))
                        {
                            eventID = imageEventID;
                        }
                        else
                        {
                            eventID = recordImage.id;
                        }

                        var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventID), null);
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                        recordImage.Uploaded = true;
                        App.Database.SaveRecordImageAsync(recordImage);
                    }
                }

                //uploadedImage = false;
                imageEventID = "";
            }
            catch
            {

            }
           
        }
    }

}
        

 