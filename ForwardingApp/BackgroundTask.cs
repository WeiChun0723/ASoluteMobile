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
        static string uploadUri;
        static string imageEventID;
        static string history;
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

      
        public static async Task DownloadLatestRecord(ContentPage contentPage)
        {
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {
                if (NetworkCheck.IsInternet())
                {
                    try
                    {

                        if(Ultis.Settings.App == "Haulage")
                        {
                            await GetWebService(ControllerUtil.getHaulageJobListURL(), "HaulageJobList", contentPage, "HaulageJob");
                            await GetWebService(ControllerUtil.getReasonListURL(), "ReasonList", contentPage, "");

                        }


                    }
                    catch (Exception exception)
                    {
                        await contentPage.DisplayAlert("Download Error", exception.Message, "OK");
                       
                    }

                }
                else
                {
                    if (contentPage != null)
                    {
                        await contentPage.DisplayAlert("Offline Mode", "Cant connect to server", "Ok");
                    }
                }
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


        public static async Task GetWebService(string callUri, string localDbName, ContentPage contentPage, string jobType)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
            var uri = callUri;
            var response = await client.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);
           
            clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(json_response.IsGood == true)
            {
                switch (localDbName)
                {
                    case "ReasonList":                       
                        App.Database.deletePickerValue();
                        var keyValue = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();
                        foreach (clsKeyValue reason in keyValue)
                        {
                            pickerValue reasonCode = new pickerValue();
                            reasonCode.Key = reason.Key; 
                            reasonCode.Value = reason.Value;
                            reasonCode.pickerType = "ReasonCode";
                            App.Database.SavePickerValue(reasonCode);
                        }                    
                        break;

                    case "JobList":
                        var JobList = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();

                        if (jobType.Equals("JobItem"))
                        {
                            App.Database.deletePending("JobItem");
                        }
                        else if (jobType.Equals("PendingLoad"))
                        {
                            App.Database.deletePending("PendingLoad");
                        }
                        else if (jobType.Equals("PendingReceiving"))
                        {
                            App.Database.deletePending("PendingReceiving");
                        }
                        //App.Database.deleteSummary("Fwd");
                        //App.Database.deleteDetail();
                    
                        foreach (clsTruckingModel job in JobList)
                        {
                            JobItems existingRecord = App.Database.GetPendingRecordAsync(job.Id);

                            if (existingRecord != null)
                            {
                                if(existingRecord.Done == 2)
                                {
                                    App.Database.deleteDoneJob(existingRecord.Id);
                                    App.Database.deleteRecordSummary(existingRecord.Id);
                                    App.Database.deleteDetail(existingRecord.Id);
                                }                            
                            }

                            if (existingRecord == null)
                            {                               
                                existingRecord = new JobItems();
                                existingRecord.TruckId = job.TruckId;
                                existingRecord.ReqSign = job.ReqSign;
                                existingRecord.Id = job.Id;
                                existingRecord.Done = 0;
                                existingRecord.JobType = jobType;
                                existingRecord.Latitude = job.Latitude;
                                existingRecord.Longitude = job.Longitude;
                                existingRecord.TelNo = job.TelNo;
                                existingRecord.EventRecordId = job.EventRecordId;
                                App.Database.SaveJobsAsync(existingRecord);

                                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(job.Id, jobType);

                                int index = 0;
                                foreach (clsCaptionValue summaryList in job.Summary)
                                {
                                    SummaryItems summaryItem = null;
                                    if (index < existingSummaryItems.Capacity)
                                    {
                                        summaryItem = existingSummaryItems.ElementAt(index);
                                    }

                                    if (summaryItem == null)
                                    {
                                        summaryItem = new SummaryItems();
                                    }

                                    summaryItem.Id = job.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = jobType;
                                    App.Database.SaveSummarysAsync(summaryItem);
                                    index++;
                                }

                                if (existingSummaryItems != null)
                                {
                                    for (; index < existingSummaryItems.Count; index++)
                                    {
                                        App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                                    }
                                }

                                List<DetailItems> existingDetailItems = App.Database.GetDetailsAsync(job.Id);

                                index = 0;
                                foreach (clsCaptionValue detailList in job.Details)
                                {
                                    DetailItems detailItem = null;
                                    if (index < existingDetailItems.Capacity)
                                    {
                                        detailItem = existingDetailItems.ElementAt(index);
                                    }

                                    if (detailItem == null)
                                    {
                                        detailItem = new DetailItems();
                                    }

                                    detailItem.Id = job.Id;
                                    detailItem.Caption = detailList.Caption;
                                    detailItem.Value = detailList.Value;
                                    detailItem.Display = detailList.Display;
                                    App.Database.SaveDetailsAsync(detailItem);
                                    index++;
                                }

                                if (existingDetailItems != null)
                                {
                                    for (; index < existingDetailItems.Count; index++)
                                    {
                                        App.Database.DeleteDetailItem(existingDetailItems.ElementAt(index));
                                    }
                                }
                            }
                        }                       
                        break;
                    case "HaulageJobList":
                        App.Database.deleteHaulage();
                        App.Database.deleteHaulageSummary("HaulageJob");
                        App.Database.deleteRecordDetails();

                        var HaulageJobList = JObject.Parse(content)["Result"].ToObject <List<clsHaulageModel>>();
                       
                        foreach (clsHaulageModel job in HaulageJobList)
                        {

                            JobItems existingRecord = new JobItems();
                            
                                existingRecord.TruckId = job.TruckId;
                                existingRecord.ReqSign = job.ReqSign;
                                existingRecord.Id = job.Id;
                                existingRecord.Done = 0;
                                existingRecord.JobType = jobType;
                                existingRecord.Latitude = job.Latitude;
                                existingRecord.Longitude = job.Longitude;
                                existingRecord.TelNo = job.TelNo;
                                existingRecord.EventRecordId = job.EventRecordId;
                                existingRecord.TrailerId = job.TrailerId;
                                existingRecord.ContainerNo = job.ContainerNo;
                                existingRecord.MaxGrossWeight = job.MaxGrossWeight;
                                existingRecord.TareWeight = job.TareWeight;
                                existingRecord.CollectSeal = job.CollectSeal;
                                existingRecord.SealNo = job.SealNo;                           
                                existingRecord.ActionId = job.ActionId.ToString();
                                existingRecord.ActionMessage = job.ActionMessage;
                                existingRecord.Title = job.Title;
                                existingRecord.SealMode = job.SealMode;
                                App.Database.SaveJobsAsync(existingRecord);

                            List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(job.Id, jobType);

                                int index = 0;
                                foreach (clsCaptionValue summaryList in job.Summary)
                                {
                                SummaryItems summaryItem = null;
                                 if (index < existingSummaryItems.Capacity)
                                 {
                                     summaryItem = existingSummaryItems.ElementAt(index);
                                 }

                                 if (summaryItem == null)
                                 {
                                     summaryItem = new SummaryItems();
                                 }

                                 summaryItem.Id = job.Id;
                                 summaryItem.Caption = summaryList.Caption;
                                 summaryItem.Value = summaryList.Value;
                                 summaryItem.Display = summaryList.Display;
                                 summaryItem.Type = jobType;
                                 summaryItem.BackColor = job.BackColor;
                                 App.Database.SaveSummarysAsync(summaryItem);
                                 index++;

                                }

                     
                                if (existingSummaryItems != null)
                                {
                                    for (; index < existingSummaryItems.Count; index++)
                                    {
                                        App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                                    }
                                }

                                List<DetailItems> existingDetailItems = App.Database.GetDetailsAsync(job.Id);

                                index = 0;
                                foreach (clsCaptionValue detailList in job.Details)
                                {
                                    DetailItems detailItem = null;
                                    if (index < existingDetailItems.Capacity)
                                    {
                                        detailItem = existingDetailItems.ElementAt(index);
                                    }

                                    if (detailItem == null)
                                    {
                                        detailItem = new DetailItems();
                                    }

                                    detailItem.Id = job.Id;
                                    detailItem.Caption = detailList.Caption;
                                    detailItem.Value = detailList.Value;
                                    detailItem.Display = detailList.Display;
                                    App.Database.SaveDetailsAsync(detailItem);
                                    index++;
                                }

                                if (existingDetailItems != null)
                                {
                                    for (; index < existingDetailItems.Count; index++)
                                    {
                                        App.Database.DeleteDetailItem(existingDetailItems.ElementAt(index));
                                    }
                                }                            
                        }
                        break;

                }
            }
            else
            {
                if (json_response.Message == "Invalid Session !")
                {
                    //BackgroundTask.Logout(contentPage);
                    await contentPage.DisplayAlert("Error", json_response.Message, "Ok");
                }
                else
                {
                    await contentPage.DisplayAlert("Error", json_response.Message, "Ok");
                }
               
            }
            
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

                    if(!(String.IsNullOrEmpty(imageEventID)))
                    {
                        eventID = imageEventID;
                    }
                    else
                    {
                        eventID = recordImage.id;
                    }

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventID),null);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
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
        

 