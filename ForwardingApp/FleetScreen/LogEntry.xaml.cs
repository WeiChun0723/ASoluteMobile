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
        string offlineLogID, uri ;
        double imageWidth;             
        static string existingRecordId = "", imageLinkID = "";
        List<AppImage> recordImages = new List<AppImage>();
        byte[] scaledImageByte;

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
                Title = "Buku Log";
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

            if(!(String.IsNullOrEmpty(imageLinkID)))
            {
                UploadImage();
            }
        }

        public async void confirmLog(object sender, EventArgs e)
        {
            try
            {
                clsTrip newTrip = new clsTrip();

                if (!(String.IsNullOrEmpty(startLocation.Text)) && !(String.IsNullOrEmpty(startOdometer.Text)))
                {
                    string startDate_Time = startDate.Date.ToString("yyyy-MM-dd") + "T" + startTime.Time.ToString();

                    newTrip.StartTime = Convert.ToDateTime(startDate_Time);
                    newTrip.StartOdometer = Convert.ToInt32(startOdometer.Text);
                    newTrip.StartLocationName = startLocation.Text.ToUpper();
                    newTrip.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                    newTrip.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                    newTrip.StartGeoLoc = ControllerUtil.getPositionAsync();


                    if (endLogEntry.IsVisible == true)
                    {
                        if (!(String.IsNullOrEmpty(endLocation.Text)) && !(String.IsNullOrEmpty(endOdometer.Text)))
                        {
                            if (Convert.ToInt32(startOdometer.Text) <= Convert.ToInt32(endOdometer.Text))
                            {

                                string endDate_Time = endDate.Date.Year + "-" + endDate.Date.Month + "-" + endDate.Date.Day + "T" + endTime.Time.ToString();
                                newTrip.EndTime = Convert.ToDateTime(endDate_Time);
                                newTrip.EndOdometer = Convert.ToInt32(endOdometer.Text);
                                newTrip.EndLocationName = endLocation.Text.ToUpper();
                                newTrip.Id = trip.Id;
                                newTrip.EndGeoLoc = ControllerUtil.getPositionAsync();
                                newTrip.LinkId = "";
                                UpdateLogBook(newTrip);
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
                                await DisplayAlert("Kesilapan", "Sila mengisikan semua data yang diperlukan oleh permulaan.", "OK");
                            }
                        }

                    }
                    else
                    {
                        UpdateLogBook(newTrip);
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
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
           
            /*List<LogBookData> pendingLog = App.Database.GetTripLog(0);
         
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
                        await BackgroundTask.UploadLatestRecord(this);
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
            }  */
        }

        async void UpdateLogBook(clsTrip data)
        {
            var content = await CommonFunction.CallWebService(1, data, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewLogRecordURL());
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (response.IsGood)
            {

                if (endLogEntry.IsVisible == false)
                {
                    endLogEntry.IsVisible = true;
                    if (Ultis.Settings.Language.Equals("English"))
                    {
                        await DisplayAlert("Success", "New log record added", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Berjaya", "Record baru ditambah", "OK");
                    }

                    endTime.Time = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, (Int16)00);
                }
                else
                {
                    if (Ultis.Settings.Language.Equals("English"))
                    {
                        await DisplayAlert("Success", "Log Book record updated.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Berjaya", "Kemaskini data buku log.", "OK");
                    }
                }

                Ultis.Settings.RefreshMenuItem = "Yes";
                imageLinkID = response.Result["LinkId"];
                trip.Id = response.Result["Id"];
                UploadImage();
            }
            else
            {
                //await DisplayAlert("Error", response.Message, "OK");
                if (Ultis.Settings.Language.Equals("English"))
                {
                    await DisplayAlert("Error", response.Message, "OK");
                }
                else
                {
                    await DisplayAlert("Failed", response.Message, "OK");
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
                images = App.Database.GetUplodedRecordImagesAsync(offlineLogID, "NormalImage");
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

        async void UploadImage()
        {
            try
            {
                recordImages = App.Database.GetPendingRecordImages(false);
                foreach (AppImage recordImage in recordImages)
                {
                    clsFileObject image = new clsFileObject();

                    byte[] originalPhotoImageBytes = File.ReadAllBytes(recordImage.photoFileLocation);
                    scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100, false);
                    image.Content = scaledImageByte;
                    image.FileName = recordImage.photoFileName;

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(imageLinkID));
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
                }

            }
            catch
            {

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

                startDate.Date = trip.StartTime.Date;
                startTime.Time = trip.StartTime.TimeOfDay;
                imageLinkID = trip.LinkId;

                if (existingRecordId != "")
                {
                    startOdometer.Text = trip.StartOdometer.ToString();
                    startLocation.Text = trip.StartLocationName;

                    endOdometer.Text = trip.EndOdometer.ToString();
                    endLocation.Text = trip.EndLocationName;

                    if (!(String.IsNullOrEmpty(trip.EndTime.ToString())))
                    {
                        endDate.Date = trip.EndTime.Value.Date;
                        endTime.Time = trip.EndTime.Value.TimeOfDay;
                    }
                    else
                    {
                        endDate.Date = DateTime.Now.Date;
                        endTime.Time = DateTime.Now.TimeOfDay;
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

                List<AutoComplete> locations = new List<AutoComplete>(App.Database.GetAutoCompleteValues("Location"));

                List<string> locationSuggestion = new List<string>();
                for (int i = 0; i < locations.Count; i++)
                {
                    locationSuggestion.Add(locations[i].Value);
                }

                startLocation.DataSource = locationSuggestion;
                endLocation.DataSource = locationSuggestion;
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