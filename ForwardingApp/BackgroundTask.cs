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
        static bool uploadedImage = true;
        static string history;
        static string previousLocation = "";
        static string chklocation = "0";
        static List<AppImage> recordImages = new List<AppImage>();

        public BackgroundTask()
        {
        }

        public static async Task StartListening()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(60), 0, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false,

            });

            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
        }

        public static async void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var position = e.Position;
            chklocation = Math.Round(position.Latitude, 2) + "," + Math.Round(position.Longitude, 2);

            if (chklocation != previousLocation)
            {
                try
                {
                    previousLocation = Math.Round(position.Latitude, 2) + "," + Math.Round(position.Longitude, 2);
                    var locator = CrossGeolocator.Current;
                    var getAddress = await locator.GetAddressesForPositionAsync(position);
                    var addressDetail = getAddress.FirstOrDefault();

                    string address = addressDetail.Thoroughfare + "," + addressDetail.PostalCode + "," + addressDetail.Locality + "," + addressDetail.AdminArea + "," + addressDetail.CountryName;

                    string location = position.Latitude + "," + position.Longitude;

                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var uri = ControllerUtil.getGPSTracking(location, address);
                    var response = await client.GetAsync(uri);
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(content);
                    clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (gps_response.IsGood)
                    {
                        previousLocation = location;

                        App.gpsLocationLat = position.Latitude;
                        App.gpsLocationLong = position.Longitude;

                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }

            }
        }


        public static async Task UploadLatestRecord(ContentPage page)
        {
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {
                if (NetworkCheck.IsInternet())
                {
                    try
                    {
                        List<JobItems> jobItems = App.Database.GetDoneJobItems(1);
                        foreach (JobItems jobItem in jobItems)
                        {
                            
                            clsTruckingModel job = new clsTruckingModel();
                            job.Id = jobItem.Id;
                            job.Remarks = jobItem.Remark;

                            if (jobItem.UpdateType == "UpdateJob")
                            {
                                if(Ultis.Settings.App == "Fleet")
                                {
                                    uploadUri = ControllerUtil.postJobDetailURL(history);
                                    imageEventID = jobItem.EventRecordId.ToString();
                                }
                                else if (Ultis.Settings.App == "Courier")
                                {
                                    uploadUri = ControllerUtil.postFowardJobURL(jobItem);
                                }                               
                            }
                            else if(jobItem.UpdateType == "CargoReturnUpdate")
                            {                            
                                job.RefNo = jobItem.jobNo;
                                job.ReasonCode = jobItem.ReasonCode;
                                uploadUri = ControllerUtil.postCargoReturnURL();
                            }
                            else if(jobItem.UpdateType == "FutileUpdate")
                            {                              
                                job.ReasonCode = jobItem.ReasonCode;
                                uploadUri = ControllerUtil.postFutileTripURL();
                            }
                            
                            var client = new HttpClient();
                            client.BaseAddress = new Uri( Ultis.Settings.SessionBaseURI);

                            string reply;
                            if (Ultis.Settings.App == "Fleet")
                            {
                                var content = JsonConvert.SerializeObject(job);
                                var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                                var response = await client.PostAsync(uploadUri, httpContent);
                                reply = await response.Content.ReadAsStringAsync();
                            }
                            else 
                            {
                                var foward_response = await client.GetAsync(uploadUri);
                                reply = await foward_response.Content.ReadAsStringAsync();
                            }

                            Debug.WriteLine(reply);

                            clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(reply);
                                                        
                            if (json_response.IsGood == true)
                            {
                                //uploadedImage = 0;
                                jobItem.Done = 2;
                                App.Database.SaveJobsAsync(jobItem);


                                if (jobItem.UpdateType == "UpdateJob")
                                {
                                    history = jobItem.jobNo;
                                }
                                else if (jobItem.UpdateType == "CargoReturnUpdate")
                                {
                                    history = jobItem.jobNo + " Cargo returned";
                                }
                                else if (jobItem.UpdateType == "FutileUpdate")
                                {
                                    history = jobItem.jobNo + " Futile updated";
                                }
                                                                
                                                                                                               

                                if (jobItem.UpdateType == "FutileUpdate" || jobItem.UpdateType == "CargoReturnUpdate")
                                {
                                    imageEventID = json_response.Result["EventRecordId"];
                                }

                                BackgroundUploadImage();
                              
                            }
                            else
                            {
                                jobItem.Done = 0;
                                App.Database.SaveJobsAsync(jobItem);

                                App.Database.deletePending("JobItem");
                                App.Database.deleteSummary(jobItem.Id);
                                App.Database.deleteDetail(jobItem.Id);

                                
                                if (jobItem.UpdateType == "UpdateJob")
                                {
                                    history = jobItem.jobNo ;
                                }
                                else if (jobItem.UpdateType == "CargoReturnUpdate")
                                {
                                    history = jobItem.jobNo + " Cargo update";
                                }
                                else if (jobItem.UpdateType == "FutileUpdate")
                                {
                                    history = jobItem.jobNo + " Futile update";
                                }


                            }
                        }
                      
                    }                 
                    catch (Exception)
                    {
                        
                    }

                    try
                    {
                        List<RefuelData> refuelDatas = App.Database.PendingRefuelData();
                       
                        foreach(RefuelData refuelData in refuelDatas)
                        {                          
                            refuelData.Done = 1;
                            App.Database.SaveRecordAsync(refuelData);

                            var client = new HttpClient();
                            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                            var uri = ControllerUtil.postNewRecordURL();
                            var content = JsonConvert.SerializeObject(refuelData);
                            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                            var response = await client.PostAsync(uri, httpContent);
                            var reply = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(reply);
                            clsResponse status = JsonConvert.DeserializeObject<clsResponse>(reply);

                            if (status.IsGood == true)
                            {
                                imageEventID = status.Result["LinkId"];
                                BackgroundUploadImage();
                                Ultis.Settings.UpdatedRecord = "Yes";
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    //CommonFunction.AppActivity("Add refuel record", "Succeed", status.Message);
                                    await page.DisplayAlert("Success", "Record added", "OK");
                                }
                                else
                                {
                                    //CommonFunction.AppActivity("Isi minyak entri", "Berjaya", status.Message);
                                    await page.DisplayAlert("Berjaya", "Record baru ditambah", "OK");
                                }

                                await page.Navigation.PopAsync();
                                
                            }
                            else
                            {
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    //CommonFunction.AppActivity("Add refuel record", "Failed", status.Message);
                                    await page.DisplayAlert("Error", "Failed " + status.Message, "OK");
                                }
                                else
                                {
                                    //CommonFunction.AppActivity("Isi minyak entri", "Gagal", status.Message);
                                    await page.DisplayAlert("Gagal", status.Message, "OK");
                                }    
                            }
                        }                          
                    }
                    catch(Exception exception)
                    {
                       
                    }

                    try
                    {
                        List<LogBookData> Logs = App.Database.GetTripLog(0);
                        
                        foreach (LogBookData Log in Logs)
                        {
                                                     
                            if(Log.OfflineID == Log.Id)
                            {
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    history = "Update log book record";
                                }
                                else
                                {
                                    history = "Kemaskini data buku log";
                                }
                                                                 
                            }
                            else
                            {
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    history = "Create log book record";
                                }
                                else
                                {
                                    history = "Menambah data buku log ";
                                }
                                
                            }

                            var client = new HttpClient();
                            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                            var uri = ControllerUtil.postNewLogRecordURL();
                            var content = JsonConvert.SerializeObject(Log);
                            var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                            var response = await client.PostAsync(uri, httpContent);
                            var reply = await response.Content.ReadAsStringAsync();
                            Debug.WriteLine(reply);
                            clsResponse log_response = JsonConvert.DeserializeObject<clsResponse>(reply);

                            if (log_response.IsGood == true)
                            {
                                if (!(string.IsNullOrEmpty(Log.EndLocationName)))
                                {
                                    Log.Done = 1;
                                }
                   
                                Log.Id = log_response.Result["Id"];
                                App.Database.SaveLogRecordAsync(Log);
                                imageEventID = log_response.Result["LinkId"];                                
                                BackgroundUploadImage();                                                                 

                               
                            }
                            else
                            {
                              
                            }

                            App.Database.SaveLogRecordAsync(Log);
                        }
                    }
                    catch
                    {

                    }

                }
                
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
                        if ( Ultis.Settings.App == "Fleet")
                        {
                           //await GetWebService(ControllerUtil.getDownloadTruckListURL(), "JobList", contentPage, "JobItem");
                           //await GetWebService(ControllerUtil.getDownloadPendingLoadURL(), "JobList", contentPage, "PendingLoad");
                           //await GetWebService(ControllerUtil.getDownloadPendingRecURL(), "JobList", contentPage, "PendingReceiving");                           
                        }
                        else if(Ultis.Settings.App == "Haulage")
                        {
                            await GetWebService(ControllerUtil.getDownloadHaulageListURL(), "HaulageJobList", contentPage, "HaulageJob");
                            await GetWebService(ControllerUtil.getReasonListURL(), "ReasonList", contentPage, "");

                        }
                        else if (Ultis.Settings.App == "Fowarding")
                        {
                            await GetWebService(ControllerUtil.getDownloadFowardListURL(), "JobList", contentPage, "JobItem");
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
                                    App.Database.deleteSummary(existingRecord.Id);
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
                                existingRecord.telNo = job.TelNo;
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
                        App.Database.deleteHaulageDetail();

                        var HaulageJobList = JObject.Parse(content)["Result"].ToObject <List<clsHaulageModel>>();
                       
                        foreach (clsHaulageModel job in HaulageJobList)
                        {
                            //JobItems existingRecord = App.Database.GetPendingRecordAsync(job.Id);
                            
                            JobItems existingRecord = new JobItems();
                            
                                existingRecord.TruckId = job.TruckId;
                                existingRecord.ReqSign = job.ReqSign;
                                existingRecord.Id = job.Id;
                                existingRecord.Done = 0;
                                existingRecord.JobType = jobType;
                                existingRecord.Latitude = job.Latitude;
                                existingRecord.Longitude = job.Longitude;
                                existingRecord.telNo = job.TelNo;
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

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventID));
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
        

 