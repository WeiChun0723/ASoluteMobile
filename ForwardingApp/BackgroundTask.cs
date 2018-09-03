﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Microsoft.AppCenter.Crashes;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Models;
using ASolute_Mobile.Data;
using ASolute.Mobile.Models;
using Plugin.Geolocator;
using Newtonsoft.Json.Linq;
using System.Collections.ObjectModel;
using Acr.UserDialogs;
using System.Drawing;

namespace ASolute_Mobile
{
    public class BackgroundTask
    {
        static byte[] scaledImageByte;
        static string uploadUri;
        static string imageEventID;
        static int uploadedImage = 0;
        static string history,backColor;
        static string previousLocation = "";

        public BackgroundTask()
        {
        }

        public static async void StartListening()
        {
         
            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(60), 0, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 10,
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = true
            });
            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;           
        }

        public static void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                var position = e.Position;
                string location = position.Latitude + "," + position.Longitude;

                if (location != previousLocation)
                {
                    try
                    {

                        var client = new HttpClient();
                        client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                        var uri = ControllerUtil.getGPSTracking(location);
                        var response = await client.GetAsync(uri);
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (gps_response.IsGood)
                        {
                            previousLocation = location;
                           
                        }
                    }
                    catch
                    {

                    }
                }
              
            });

        }
  
        public static async Task UploadLatestRecord()
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
                                    uploadUri = ControllerUtil.postJobDetailURL();
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
                                uploadedImage = 0;
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
                                                                
                               CommonFunction.AppActivity(history, "Succeed", json_response.Message);                                                                                             

                                if (jobItem.UpdateType == "FutileUpdate" || jobItem.UpdateType == "CargoReturnUpdate")
                                {
                                    imageEventID = json_response.Result["EventRecordId"];
                                }

                                await UploadImage(jobItem.Id);
                              
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

                                CommonFunction.AppActivity(history, "Failed", json_response.Message);
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
                                await UploadImage(refuelData.ID);
                                Ultis.Settings.UpdatedRecord = "Yes";
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    CommonFunction.AppActivity("Add refuel record", "Succeed", status.Message);
                                }
                                else
                                {
                                    CommonFunction.AppActivity("Isi minyak entri", "Berjaya", status.Message);
                                }
                                
                            }
                            else
                            {
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    CommonFunction.AppActivity("Add refuel record", "Failed", status.Message);
                                }
                                else
                                {
                                    CommonFunction.AppActivity("Isi minyak entri", "Gagal", status.Message);
                                }    
                            }
                        }                          
                    }
                    catch(Exception exception)
                    {
                        if (Ultis.Settings.Language.Equals("English"))
                        {
                            CommonFunction.AppActivity("Sync refuel record", "Failed", exception.Message);
                        }
                        else
                        {
                            CommonFunction.AppActivity("Isi minyak entri", "Gagal", exception.Message);
                        }
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

                                /*if(Log.StartOdometer != 0 )
                                {
                                    Ultis.Settings.logOdometer = log_response.Result["StartOdometer"];
                                }
                                else if (Log.EndOdometer != 0)
                                {
                                    Ultis.Settings.logOdometer = log_response.Result["EndOdometer"];
                                }*/
                                
                                Log.Id = log_response.Result["Id"];
                                App.Database.SaveLogRecordAsync(Log);
                                imageEventID = log_response.Result["LinkId"];                                
                                await UploadImage(Log.OfflineID);                                                                 

                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    CommonFunction.AppActivity(history, "Succeed", log_response.Message);
                                }
                                else
                                {
                                    CommonFunction.AppActivity(history, "Berjaya", log_response.Message);
                                }
                            }
                            else
                            {
                                Log.Done = 1;
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    CommonFunction.AppActivity(history, "Failed", log_response.Message);
                                }
                                else
                                {
                                    CommonFunction.AppActivity(history, "Gagal", log_response.Message);
                                }
                            }

                            
                            App.Database.SaveLogRecordAsync(Log);
                        }
                    }
                    catch
                    {

                    }
                  
                    /*try
                    {
                        List<JobNoList> jobValue = new List<JobNoList>(App.Database.GetJobNo(Ultis.Settings.WareHouse_JobNo,false));
                        List<string> jobsData = new List<string>();
                        for (int i = 0; i < jobValue.Count; i++)
                        {
                            jobsData.Add(jobValue[i].JobNoValue);
                        } 

                        var client = new HttpClient();
                        string baseAddress = Ultis.Settings.SessionBaseURI;
                        var url = baseAddress + ControllerUtil.postNewCargoRecordURL("hello");
                        Uri uri = new Uri(url);
                        var content = JsonConvert.SerializeObject("jobsData");
                        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(uri, httpContent);
                        var reply = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(reply);

                        clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(reply);
                        ActivityLog result = new ActivityLog();
                        if (json_response.IsGood == true)
                        {
                            result.activity = "Job No uploaded";
                            result.status = "Status: Success";
                            result.message = "Message: " + json_response.Message;                         
                        }
                        else
                        {
                            result.activity = "Job No upload fail";
                            result.status = "Status: Failed";
                            result.message = "Message: " + json_response.Message;
                        }

                        App.Database.SaveActivity(result);
                    }                  
                    catch (Exception exception)
                    {
                       
                    }*/
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
                           await GetWebService(ControllerUtil.getDownloadTruckListURL(), "JobList", contentPage, "JobItem");
                           await GetWebService(ControllerUtil.getDownloadPendingLoadURL(), "JobList", contentPage, "PendingLoad");
                           await GetWebService(ControllerUtil.getDownloadPendingRecURL(), "JobList", contentPage, "PendingReceiving");                           
                        }
                        else if(Ultis.Settings.App == "Haulage")
                        {
                            await GetWebService(ControllerUtil.getReasonListURL(), "ReasonList", contentPage, "");
                            await GetWebService(ControllerUtil.getDownloadHaulageListURL(), "HaulageJobList", contentPage, "HaulageJob");
                        }
                        else if (Ultis.Settings.App == "Fowarding")
                        {
                            await GetWebService(ControllerUtil.getDownloadFowardListURL(), "JobList", contentPage, "JobItem");
                        }

                    }
                    catch (Exception exception)
                    {
                        await contentPage.DisplayAlert("Download Error", exception.Message, "OK");
                        Crashes.TrackError(exception);
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
            Ultis.Settings.SessionUserItem.UserId = "";
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
                        App.Database.deleteHaulage("HaulageJob");
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
                await contentPage.DisplayAlert("Download Error", json_response.Message, "OK");
            }
            
        }

        public static async Task UploadImage(string uploadID)
        {
            List<AppImage> recordImages = App.Database.GetRecordImagesAsync(uploadID, false);
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
                    scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100,false);
                    image.Content = scaledImageByte;
                }

                image.FileName = recordImage.photoFileName;

                var image_client = new HttpClient();
                image_client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                var image_uri = ControllerUtil.getUploadImageURL(imageEventID);
                var imagecontent = JsonConvert.SerializeObject(image);
                var image_httpContent = new StringContent(imagecontent, Encoding.UTF8, "application/json");

                HttpResponseMessage image_response = await image_client.PostAsync(image_uri, image_httpContent);
                var Imagereply = await image_response.Content.ReadAsStringAsync();
                Debug.WriteLine(Imagereply);
                clsResponse Imageresult = JsonConvert.DeserializeObject<clsResponse>(Imagereply);

                if (Imageresult.IsGood == true)
                {
                    uploadedImage++;
                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
                   
                }
            }
        
        }
    }

}
        

 