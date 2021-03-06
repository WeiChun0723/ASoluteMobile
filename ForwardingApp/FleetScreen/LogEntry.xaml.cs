﻿using ASolute.Mobile.Models;
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

namespace ASolute_Mobile
{
    public partial class LogEntry : ContentPage
    {
        List<AppImage> images = new List<AppImage>();
        clsResponse newLogResponse = new clsResponse();
        clsTrip trip = new clsTrip();
        string offlineLogID, uri;
        double imageWidth;
        static string existingRecordId = "", imageLinkID = "";
        List<AppImage> recordImages = new List<AppImage>();
        byte[] scaledImageByte;

        public LogEntry(string id, string title)
        {
            InitializeComponent();

            //initialized height for image grid row
            imageWidth = App.DisplayScreenWidth / 3;
            imageGridRow.Height = imageWidth;

            StackLayout main = new StackLayout();
            Label title1 = new Label
            {
                FontSize = 15,
                Text = title,
                TextColor = Color.White
            };
            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };
            main.Children.Add(title1);
            main.Children.Add(title2);
            NavigationPage.SetTitleView(this, main);

            Title = (Ultis.Settings.Language.Equals("English")) ? "Log Entry" : "Buku Log";

            existingRecordId = id;


            if (NetworkCheck.IsInternet())
            {
                logDefaultValue();
            }
            else
            {
                DisplayAlert("Reminder", "You are currently offline", "OK");
            }


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.DeleteImage == "Yes")
            {
                DisplayImage();
                Ultis.Settings.DeleteImage = "No";
            }
        }

        void StartLocationChanged(object sender, Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs e)
        {
            startLocation.Text = e.Value.ToUpper();
        }

        void EndLocationChanged(object sender, Syncfusion.SfAutoComplete.XForms.ValueChangedEventArgs e)
        {
            endLocation.Text = e.Value.ToUpper();
        }

       
        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var image = sender as Image;

            switch (image.StyleId)
            {
                case "camera_icon":
                    await CommonFunction.StoreImages(offlineLogID, this, "NormalImage");
                    DisplayImage();

                    if (!(String.IsNullOrEmpty(imageLinkID)))
                    {
                        UploadImage();
                    }
                    break;

                case "confirm_icon":
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
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                    break;
            }
        }

        async void UpdateLogBook(clsTrip data)
        {
            try
            {
                var content = await CommonFunction.CallWebService(1,data, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewLogRecordURL(),this);
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

                        endTime.Time = DateTime.Now.TimeOfDay;

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

                    Ultis.Settings.RefreshListView = "Yes";

                    imageLinkID = response.Result["LinkId"];
                    camera_icon.IsEnabled = true;
                    trip.Id = response.Result["Id"];
                    UploadImage();

                }
              
            }
            catch
            {

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
                recordImages = App.Database.GetRecordImagesAsync(offlineLogID, false);
                foreach (AppImage recordImage in recordImages)
                {
                    clsFileObject image = new clsFileObject();

                    byte[] originalPhotoImageBytes = File.ReadAllBytes(recordImage.photoFileLocation);
                    scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100);
                    image.Content = scaledImageByte;
                    image.FileName = recordImage.photoFileName;

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(imageLinkID), this);
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

                foreach (clsKeyValue location in trip.LocationList)
                {
                    AutoComplete existingLocation = App.Database.GetAutoCompleteValue(location.Value.ToUpper());

                    if (existingLocation == null)
                    {
                        existingLocation = new AutoComplete();
                    }
                    existingLocation.Value = location.Value.ToUpper();
                    existingLocation.Type = "Location";
                    App.Database.SaveAutoCompleteAsync(existingLocation);
                }

                foreach (clsCaptionValue language in trip.Captions)
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
                    else if (lblStartOdometer.Hint == lblCaption)
                    {
                        lblStartOdometer.Hint = language.Value;
                    }
                    else if (lblStartFrom.Hint == lblCaption)
                    {
                        lblStartFrom.Hint = language.Value;
                    }
                    else if (lblEnd.Text == lblCaption)
                    {
                        lblEnd.Text = language.Value;
                    }
                    else if (lblEndOdometer.Hint == lblCaption)
                    {
                        lblEndOdometer.Hint = language.Value;
                    }
                    else if (lblEndLocation.Hint == lblCaption)
                    {
                        lblEndLocation.Hint = language.Value;
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

                if(!(String.IsNullOrEmpty(imageLinkID)))
                {
                    camera_icon.IsEnabled = true;
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