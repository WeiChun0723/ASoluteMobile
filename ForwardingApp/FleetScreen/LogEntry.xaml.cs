using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LogEntry : ContentPage
	{
        public LogHistory previousPage;
        List<AppImage> images = new List<AppImage>();
        clsResponse newLogResponse = new clsResponse();        
        clsTrip trip = new clsTrip();
        string offlineLogID;
        double imageWidth;             
        public static string existingRecordId = "";
        string uri;
        public static int odo;

        public  LogEntry (string id)
		{
			InitializeComponent ();
            lblStartDateTime.Text = "Date & Time";
            lblEndDateTime.Text = "Date & Time";

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Log Entry";
            }
            else 
            {
                Title = "Buku Log Entri";
            }

            existingRecordId = id;          

            //generate id for the image to link with the record for offline sync 
            
            if (NetworkCheck.IsInternet())
            {
                logDefaultValue();
            }
            else
            {
                DisplayAlert("Reminder", "You are currently offline", "OK");
            }

            List<AutoComplete> locations = new List<AutoComplete>(App.Database.GetAutoCompleteValues("Location"));

            List<string> locationSuggestion = new List<string>();
            for (int i = 0; i < locations.Count; i++)
            {
                locationSuggestion.Add(locations[i].Value);
            }

            startLocation.DataSource = locationSuggestion;
            endLocation.DataSource = locationSuggestion;
            
            imageGrid.RowSpacing = 0;
            imageGrid.ColumnSpacing = 0;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.deleteImage == "Yes")
            {
                DisplayImage();
                Ultis.Settings.deleteImage = "No";
            }
        }

        public void startMeter(object sender, EventArgs e)
        {
            string _startOdometer = startOdometer.Text;

            if (_startOdometer.Length > 6)       //If it is more than your character restriction
            {
                _startOdometer = _startOdometer.Remove(_startOdometer.Length - 1);  // Remove Last character
                startOdometer.Text = _startOdometer;        //Set the Old value
            }
        }

        public void endMeter(object sender, EventArgs e)
        {
            string _endOdometer = endOdometer.Text;

            if (_endOdometer.Length > 6)       //If it is more than your character restriction
            {
                _endOdometer = _endOdometer.Remove(_endOdometer.Length - 1);  // Remove Last character
                endOdometer.Text = _endOdometer;        //Set the Old value
            }

        }

        public async void captureImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(offlineLogID, this);
            DisplayImage();
        }

        public async void confirmLog(object sender, EventArgs e)
        {            
            List<LogBookData> pendingLog = App.Database.GetTripLog(0);
         
            if (pendingLog.Count == 0 || pendingLog[0].OfflineID == offlineLogID || pendingLog[0].Id == trip.Id || !(String.IsNullOrEmpty(existingRecordId)))
            {
                if(!(String.IsNullOrEmpty(startLocation.Text)) && !(String.IsNullOrEmpty(startOdometer.Text)))
                {
            
                    try
                    {
                        App.Database.deleteLogPendingData();
                        LogBookData logData = new LogBookData();
                        string startDate_Time = startDate.Date.Year + "-" + startDate.Date.Month + "-" + startDate.Date.Day + "T" + startTime.Time.ToString();

                        logData.OfflineID = offlineLogID;
                        logData.Done = 0;
                        logData.StartTime = Convert.ToDateTime(startDate_Time);
                        logData.StartOdometer = Convert.ToInt32(startOdometer.Text);
                        logData.StartLocationName = startLocation.Text;
                        logData.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                        logData.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                        logData.StartGeoLoc = ControllerUtil.getPositionAsync();
                        App.Database.SaveLogRecordAsync(logData);

                        if (endLogEntry.IsVisible == true)
                        {
                            if (!(String.IsNullOrEmpty(startLocation.Text)) && !(String.IsNullOrEmpty(startOdometer.Text)))
                            {
                                if (Convert.ToInt32(startOdometer.Text) < Convert.ToInt32(endOdometer.Text))
                                {
                                    
                                    string endDate_Time = endDate.Date.Year + "-" + endDate.Date.Month + "-" + endDate.Date.Day + "T" + endTime.Time.ToString();
                                    logData.EndTime = Convert.ToDateTime(endDate_Time);
                                    logData.EndOdometer = Convert.ToInt32(endOdometer.Text);
                                    logData.EndLocationName = endLocation.Text;
                                    logData.Id = trip.Id;
                                    logData.EndGeoLoc = ControllerUtil.getPositionAsync();
                                    logData.LinkId = "";
                                    confirm_icon.IsEnabled = false;
                                    confirm_icon.Source = "confirmDisable.png";
                                    
                                    string status = "";
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        status = "Log record saved.";
                                    }
                                    else
                                    {
                                        status = "Rekod log disimpan.";
                                    }
                                    await DisplayAlert("", status, "OK");
                                    App.Database.SaveLogRecordAsync(logData);                                                        
                                }
                                else
                                {
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        await DisplayAlert("Error", "End Odometer must more than start odometer.", "OK");
                                    }
                                    else
                                    {
                                        await DisplayAlert("Kesilapan", "Odometer untuk akhir perlu lebih daripada odometer untuk permulaan.", "OK");
                                    }
                                }

                            }
                            else
                            {
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    await DisplayAlert("Error", "Please fill in all mandatory field.", "OK");
                                }
                                else
                                {
                                    await DisplayAlert("Kesilapan", "Sila mengisikan semua data yang diperlukan.", "OK");
                                }
                            }
                        }

                        endLogEntry.IsVisible = true;
                    }
                    catch (Exception exception)
                    {
                        await DisplayAlert("Error", exception.Message, "OK");
                    }
                }
                else
                {
                    if (Ultis.Settings.Language.Equals("English"))
                    {
                        await DisplayAlert("Error", "Please fill in all mandatory field.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Kesilapan", "Sila mengisikan semua data yang diperlukan oleh permulaan.", "OK");
                    }
                }  
            }
            else
            {
                if(Ultis.Settings.Language.Equals("English"))
                {
                    await DisplayAlert("Error", "Please close all open log before proceed", "OK");
                }
                else
                {
                    await DisplayAlert("Kesilapan", "Sila tutup semua log terbuka sebelum meneruskan", "OK");
                }
            }                              
        }

        private void AddThumbnailToImageGrid(Image image, AppImage appImage)
        {
            image.HeightRequest = imageWidth;
            image.HorizontalOptions = LayoutOptions.FillAndExpand;
            image.VerticalOptions = LayoutOptions.FillAndExpand;
            image.Aspect = Aspect.AspectFill;

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.CommandParameter = appImage;
            tapGestureRecognizer.Tapped += async (sender, e) =>
            {
                await Navigation.PushAsync(new ImageViewer((AppImage)((TappedEventArgs)e).Parameter));
            };
            image.GestureRecognizers.Add(tapGestureRecognizer);
            int noOfImages = imageGrid.Children.Count();
            int noOfCols = imageGrid.ColumnDefinitions.Count();
            int rowNo = noOfImages / noOfCols;
            int colNo = noOfImages - (rowNo * noOfCols);
            imageGrid.Children.Add(image, colNo, rowNo);
        }

        public async void DisplayImage()
        {
            try
            {
                images.Clear();
                imageGrid.Children.Clear();
                images = App.Database.GetRecordImagesAsync(offlineLogID, false);
                foreach (AppImage Image in images)
                {
                    IFile actualFile = await FileSystem.Current.GetFileFromPathAsync(Image.photoThumbnailFileLocation);
                    Stream stream = await actualFile.OpenAsync(PCLStorage.FileAccess.Read);
                    var image = new Image();
                    byte[] imageByte;
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.Position = 0; // needed for WP (in iOS and Android it also works without it)!!
                        stream.CopyTo(ms);  // was empty without stream.Position = 0;
                        imageByte = ms.ToArray();
                    }
                    image.Source = ImageSource.FromStream(() => new MemoryStream(imageByte));
                    AddThumbnailToImageGrid(image, Image);
                }
            }
            catch
            {
                await DisplayAlert("display", "Error on display", "ok");
            }
        }

        public async void logDefaultValue()
        {
            if (existingRecordId != "")
            {
                endLogEntry.IsVisible = true;
                confirm_icon.IsEnabled = true;
                offlineLogID = existingRecordId;
                uri = ControllerUtil.getLogInfoURL(existingRecordId);
            }
            else
            {
                Guid log = Guid.NewGuid();
                offlineLogID = log.ToString();
                uri = ControllerUtil.getNewLogURL();
            }

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);               
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                newLogResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                trip = JObject.Parse(content)["Result"].ToObject<clsTrip>();

                //Split the date time in the json 
                string[] start_Time = trip.StartTime.ToString().Split(' ');
                //convert and set the default time of the time picker from the json returned
                string[] start_split_time = start_Time[1].Split(':');
                int time_hour = Convert.ToInt16(start_split_time[0]);
                int time_minute = Convert.ToInt16(start_split_time[1]);
                TimeSpan ts = new TimeSpan((Int16)time_hour, (Int16)time_minute, (Int16)00);
                startTime.Time = ts;

                if(existingRecordId != "")
                {
                    startOdometer.Text = trip.StartOdometer.ToString();
                    startLocation.Text = trip.StartLocationName;

                    endOdometer.Text = trip.EndOdometer.ToString();
                    endLocation.Text = trip.EndLocationName;

                    if (trip.EndTime != null)
                    {
                        string[] end_Time = trip.EndTime.ToString().Split(' ');
                        //convert and set the default time of the time picker from the json returned
                        string[] end_split_time = end_Time[1].Split(':');
                        int end_time_hour = Convert.ToInt16(end_split_time[0]);
                        int end_time_minute = Convert.ToInt16(end_split_time[1]);
                        TimeSpan end_ts = new TimeSpan((Int16)end_time_hour, (Int16)end_time_minute, (Int16)00);
                        endTime.Time = end_ts;
                    }
                }

                foreach(clsKeyValue location in trip.LocationList)
                {                  
                    AutoComplete existingLocation = App.Database.GetAutoCompleteValue(location.Value.ToLower());

                    if (existingLocation == null)
                    {
                        existingLocation = new AutoComplete();
                    }
                    existingLocation.Value= location.Value.ToLower();
                    existingLocation.Type = "Location";
                    App.Database.SaveAutoCompleteAsync(existingLocation);
                }

                foreach(clsCaptionValue language in trip.Captions)
                {
                    string lblCaption = language.Caption;

                    if (lblStart.Text == lblCaption)
                    {
                        lblStart.Text = language.Value;                       
                    }
                    else if (lblStartDateTime.Text == lblCaption)
                    {
                        lblStartDateTime.Text = language.Value;
                        lblEndDateTime.Text = language.Value;
                    }
                    else if (lblStartOdometer.Text == lblCaption)
                    {
                        lblStartOdometer.Text = language.Value;
                    }
                    else if (lblStartFrom.Text == lblCaption)
                    {
                        lblStartFrom.Text = language.Value;
                    }
                    else if (lblEnd.Text == lblCaption)
                    {
                        lblEnd.Text = language.Value;
                    }                 
                    else if (lblEndOdometer.Text == lblCaption)
                    {
                        lblEndOdometer.Text = language.Value;
                    }
                    else if (lblEndLocation.Text == lblCaption)
                    {
                        lblEndLocation.Text = language.Value;
                    }
                }
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Json Error", exception.Message, "Ok");
            }         
        }
	}
}