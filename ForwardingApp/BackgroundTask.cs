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
using static ASolute_Mobile.BusTicketing.StopsList;
using System.Collections.ObjectModel;
using Xamarin.Essentials;

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

        public static async void GetGPS()
        {
            //var locator = CrossGeolocator.Current;
            //position = await locator.GetPositionAsync();
            /*var position = await Geolocation.GetLastKnownLocationAsync();
           
            if (App.gpsLocationLat.Equals(0) || App.gpsLocationLong.Equals(0))
            {
                if (position != null)
                {
                    location = String.Format("{0:0.000000}", position.Latitude.ToString()) + "," + String.Format("{0:0.000000}", position.Longitude.ToString());
                }
                else
                {
                    location = "0,0";
                }
            }
            else
            {
                location = String.Format("{0:0.000000}", App.gpsLocationLat) + "," + String.Format("{0:0.000000}", App.gpsLocationLong);
            }*/

            try
            {
               

                location = (App.gpsLocationLat.Equals(0) || App.gpsLocationLong.Equals(0)) ? "" : String.Format("{0:0.000000}", App.gpsLocationLat) + "," + String.Format("{0:0.000000}", App.gpsLocationLong);

                /*if (String.IsNullOrEmpty(location))
                {
                    var position = await Geolocation.GetLastKnownLocationAsync();

                    if (position != null)
                    {
                        location = String.Format("{0:0.000000}", position.Latitude.ToString()) + "," + String.Format("{0:0.000000}", position.Longitude.ToString());
                    }
                }*/

                if (!(String.IsNullOrEmpty(Ultis.Settings.SessionSettingKey)) && NetworkCheck.IsInternet())
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
                }
            }
            catch(Exception ex)
            {

            }

        }

        //download the bus stop list and store locally 
        public static async Task DownloadBusStopList()
        {
            try
            {
                var localStoredOutboundStops = new ObservableCollection<ListItems>(App.Database.GetMainMenu("OutboundList"));
                var localStoredInboundStops = new ObservableCollection<ListItems>(App.Database.GetMainMenu("InboundList"));

                if (NetworkCheck.IsInternet())
                {
                    if (!(String.IsNullOrEmpty(Ultis.Settings.SessionSettingKey)))
                    {
                        if (localStoredOutboundStops.Count == 0)
                        {
                            var outbound_content = await CommonFunction.CallWebService(0, null, "https://api.asolute.com/host/api/", ControllerUtil.getBusStops("OutboundList"), null);
                            clsResponse outbound_response = JsonConvert.DeserializeObject<clsResponse>(outbound_content);

                            if (outbound_response.IsGood)
                            {
                                StoreData(outbound_content, "OutboundList");

                            }
                        }

                        if (localStoredInboundStops.Count == 0)
                        {
                            var inbound_content = await CommonFunction.CallWebService(0, null, "https://api.asolute.com/host/api/", ControllerUtil.getBusStops("InboundList"), null);
                            clsResponse inbound_response = JsonConvert.DeserializeObject<clsResponse>(inbound_content);

                            if (inbound_response.IsGood)
                            {
                                StoreData(inbound_content, "InboundList");

                            }
                        }

                    }
                }
            }
            catch
            {

            }
        }

        public static void StoreData(string content, string action)
        {
            List<clsBusTicket> stops = JObject.Parse(content)["Result"].ToObject<List<clsBusTicket>>();

            App.Database.deleteRecords(action);
            App.Database.deleteRecordSummary(action);

            foreach (clsBusTicket stop in stops)
            {
                ListItems items = new ListItems
                {
                    StopId = stop.StopId,
                    StopName = stop.StopName,
                    Rate = stop.Rate,
                    Category = action
                };

                App.Database.SaveMenuAsync(items);

                foreach (ListItems station in stop.Stops)
                {
                    SummaryItems summaryItem = new SummaryItems
                    {
                        Id = stop.StopId,
                        StopId = station.StopId,
                        StopName = station.StopName,
                        Rate = station.Rate,
                        BackColor = "#ffffff"
                    };
                    summaryItem.Type = action;


                    App.Database.SaveSummarysAsync(summaryItem);
                }
            }
        }


        //search database for pending bus trip and sync to server
        public static async Task UploadPendingRecord()
        {
            try
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
            catch
            {

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

        public static void StartTimer()
        {
            Device.StartTimer(TimeSpan.FromSeconds(5), () =>
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

                //uploadedImage = false;
                imageEventID = "";
            }
            catch
            {

            }

        }
    }

}


