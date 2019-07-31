using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Models;
using ASolute.Mobile.Models;
using Newtonsoft.Json.Linq;


namespace ASolute_Mobile
{
    public class BackgroundTask
    {
        static byte[] scaledImageByte;
        static string imageEventID;
        static List<AppImage> recordImages = new List<AppImage>();
        //static Plugin.Geolocator.Abstractions.Position position = null;
        static string address = "";
        static string location = "";


        #region Gps Tracking
        ////Get gps from phone and send to server
        public static async void GetGPS()
        {
            try
            {
                location = (App.gpsLocationLat.Equals(0) || App.gpsLocationLong.Equals(0)) ? "" : String.Format("{0:0.0000}", App.gpsLocationLat) + "," + String.Format("{0:0.0000}", App.gpsLocationLong);

                if (!(String.IsNullOrEmpty(Ultis.Settings.SessionSettingKey)) && NetworkCheck.IsInternet() && !(String.IsNullOrEmpty(location)))
                {

                    if (Ultis.Settings.SessionUserItem.GetGPS)
                    {
                        var client = new HttpClient();
                        client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                        var sendGPSURI = ControllerUtil.getGPSTracking(location, address);
                        var content = await client.GetAsync(sendGPSURI);
                        var response = await content.Content.ReadAsStringAsync();
                        clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(response);
                        Debug.WriteLine(response);
                    }

                    App.gpsLocationLat = 0;
                    App.gpsLocationLong = 0;

                }
            }
            catch
            {

            }

        }
        #endregion

        #region Forwarding data sync
        public async static Task UploadLatestJobs()
        {
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {
                if (NetworkCheck.IsInternet())
                {
                    List<ListItems> pendingItems = App.Database.GetJobs(1);

                    foreach (ListItems item in pendingItems)
                    {
                        var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getUpdateJobURL(item.Id, item.Remark), null);

                        if (content != null)
                        {
                            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                            if(response.IsGood)
                            {
                                item.Done = 2;

                                App.Database.SaveItemAsync(item);
                            }
                        }
                    }
                }
            }
        }

        public static async Task DownloadLatestJobs(object p)
        {
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {
                if (NetworkCheck.IsInternet())
                {
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadJobListURL(), null);

                    if (content != null)
                    {
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (response.IsGood)
                        {
                            var fwdJobs = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();

                            foreach (clsTruckingModel fwdJob in fwdJobs)
                            {

                                ListItems existingRecord = App.Database.GetJobRecordAsync(fwdJob.Id);

                                if (existingRecord == null)
                                {
                                    ListItems item = new ListItems
                                    {
                                        Id = fwdJob.Id,
                                        Latitude = fwdJob.Latitude,
                                        Longitude = fwdJob.Longitude,
                                        EventRecordId = fwdJob.EventRecordId,
                                        Category = "fwd",
                                        Done = 0,
                                        ReqSign = fwdJob.ReqSign,
                                        TelNo = fwdJob.TelNo
                                    };

                                    App.Database.SaveItemAsync(item);

                                    foreach (clsCaptionValue summaryList in fwdJob.Summary)
                                    {
                                        SummaryItems summaryItem = new SummaryItems();

                                        summaryItem.Id = fwdJob.Id;
                                        summaryItem.Caption = summaryList.Caption;
                                        summaryItem.Value = summaryList.Value;
                                        summaryItem.Display = summaryList.Display;
                                        summaryItem.Type = "fwd";
                                        summaryItem.BackColor = "#ffffff";
                                        App.Database.SaveSummarysAsync(summaryItem);
                                    }

                                    foreach (clsCaptionValue detailList in fwdJob.Details)
                                    {
                                        DetailItems detailItem = new DetailItems();
                                        detailItem.Id = fwdJob.Id;
                                        detailItem.Caption = detailList.Caption;
                                        detailItem.Value = detailList.Value;
                                        detailItem.Display = detailList.Display;
                                        App.Database.SaveDetailsAsync(detailItem);
                                    }
                                }

                            }
                        }

                    }
                }
            }
        }
        #endregion

        #region AILS BUS data sync
        //search database for pending bus trip and sync to server
        public static async Task UploadPendingRecord()
        {

            if (NetworkCheck.IsInternet())
            {
                if (!(String.IsNullOrEmpty(Ultis.Settings.SessionSettingKey)))
                {
                    //upload bus trip record
                    var pendingBusTrip = App.Database.GetPendingTrip();

                    List<clsTrip> completeTrips = new List<clsTrip>();

                    foreach (BusTrip busTrip in pendingBusTrip)
                    {
                        if (busTrip.EndTime != null && busTrip.Uploaded == false)
                        {
                            clsTrip trip = new clsTrip
                            {
                                Id = busTrip.Id,
                                TruckId = busTrip.TruckId,
                                DriverId = busTrip.DriverId,
                                StartTime = busTrip.StartTime,
                                StartOdometer = 0,
                                StartLocationName = "",
                                StartGeoLoc = busTrip.StartGeoLoc,
                                EndTime = busTrip.EndTime,
                                EndOdometer = 0,
                                EndLocationName = "",
                                EndGeoLoc = busTrip.EndGeoLoc,
                                TrxStatus = 5,
                                LinkId = "",
                                LocationList = { },
                                Captions = { }
                            };

                            completeTrips.Add(trip);
                        }
                    }

                    if (completeTrips.Count != 0)
                    {
                        var content = await CommonFunction.CallWebService(1, completeTrips, Ultis.Settings.SessionBaseURI, ControllerUtil.postTrips(), null);
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (response.IsGood)
                        {
                            /*foreach (clsTrip uploadedTrip in completeTrips)
                            {
                                var completedTrip = App.Database.GetUploadedTrip(uploadedTrip.Id);

                                if (completedTrip != null)
                                {
                                    completedTrip.Uploaded = true;

                                    App.Database.SaveBusTrip(completedTrip);
                                }
                            }*/

                            App.Database.DeleteBusTrip();

                            //upload bus ticket record
                            var pendingTicket = App.Database.GetSoldTicket();
                            List<clsTicket> tickets = new List<clsTicket>();
                            foreach (SoldTicket soldTicket in pendingTicket)
                            {
                                if (!(String.IsNullOrEmpty(soldTicket.TripId)))
                                {
                                    clsTicket ticket = new clsTicket
                                    {
                                        TrxTime = soldTicket.TrxTime,
                                        TruckId = soldTicket.TruckId,
                                        DriverId = soldTicket.DriverId,
                                        TripId = soldTicket.TripId,
                                        RouteId = soldTicket.RouteId,
                                        StopId = soldTicket.StopId,
                                        TicketType = soldTicket.TicketType,
                                        PaymentType = soldTicket.PaymentType,
                                        Amount = soldTicket.Amount
                                    };

                                    tickets.Add(ticket);
                                }
                            }

                            if (tickets.Count != 0)
                            {
                                var ticket_content = await CommonFunction.CallWebService(1, tickets, Ultis.Settings.SessionBaseURI, ControllerUtil.postTickets(), null);
                                clsResponse ticket_response = JsonConvert.DeserializeObject<clsResponse>(ticket_content);

                                if (ticket_response.IsGood)
                                {
                                    /*foreach (clsTicket ticket in tickets)
                                    {
                                        var completeTicket = App.Database.GetUploadedTicket(ticket.);

                                        if (completeTicket != null)
                                        {
                                            completeTicket.Uploaded = true;

                                            App.Database.SaveTicketTransaction(completeTicket);
                                        }
                                    }*/
                                    App.Database.DeleteTicket();

                                    var test = App.Database.gettesting();
                                }
                            }
                        }
                    }

                }
            }


        }
        #endregion


        #region logout function
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

            //App.Database.DeleteUserImage(Ultis.Settings.SessionUserItem.DriverId);
            App.Database.DeleteImage("NormalImage");
            App.Database.DeleteBusTrip();
            App.Database.DeleteTicket();
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
        #endregion


        #region Image sync
        //start timer to check db for every 5 second 
        public static void StartTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
            {
                Task.Run(async () => { await BackgroundUploadImage(); });

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

        //search image store in db (except profile pic) and send it to server with respective record link id
        public static async Task BackgroundUploadImage()
        {

            recordImages = App.Database.GetPendingRecordImages(false);
            foreach (AppImage recordImage in recordImages)
            {
                if (recordImage.type != "ProfilePic")
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

                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventID), null);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                }
            }

            imageEventID = "";

        }
        #endregion
    }

}


