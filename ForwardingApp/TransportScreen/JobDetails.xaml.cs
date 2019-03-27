using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using PCLStorage;
using SignaturePad.Forms;
using Xamarin.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class JobDetails : ContentPage
    {
        string jobID, telNo, location,action;
        bool reqSign = false;
        long eventRecordID;
        double imageWidth;
        static List<DetailItems> job_Details;
        bool first_appear = true;
        private byte[] scaledImageByte;
        List<AppImage> images = new List<AppImage>();

        public JobDetails(TruckModel record)
        {
            InitializeComponent();

            jobID = record.RecordId;
            eventRecordID = record.EventRecordId;
            action = record.Action;

            if(!(String.IsNullOrEmpty(record.TelNo)))
            {
                phone_icon.IsVisible = true;
                telNo = record.TelNo;
            }

            if (!(String.IsNullOrEmpty(record.Latitude)))
            {
                map_icon.IsVisible = true;
                location = record.Latitude + "," + record.Longitude;
            }

            if(record.ReqSign)
            {
                signatureStack.IsVisible = true;
            }

            PageContent();

            imageGrid.RowSpacing = 0;
            imageGrid.ColumnSpacing = 0;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            //DisplayImage();
        }

        async void DisplayImage()
        {
            images.Clear();
           imageGrid.Children.Clear();

           images = App.Database.GetUplodedRecordImagesAsync(jobID, "NormalImage");

           foreach (AppImage Image in images)
           {
               byte[] imageByte;

               IFile actualFile = await FileSystem.Current.GetFileFromPathAsync(Image.photoThumbnailFileLocation);
               Stream stream = await actualFile.OpenAsync(PCLStorage.FileAccess.Read);

               using (MemoryStream ms = new MemoryStream())
               {
                   stream.Position = 0; // needed for WP (in iOS and Android it also works without it)!!
                   stream.CopyTo(ms);  // was empty without stream.Position = 0;
                   imageByte = ms.ToArray();
               }
               var image = new Image();
               image.Source = ImageSource.FromStream(() => new MemoryStream(imageByte));
               AddThumbnailToImageGrid(image, Image);
           }
        }



        public void PageContent()
        {
            if (first_appear == true)
            {
                job_Details = App.Database.GetDetailsAsync(jobID);
                first_appear = false;
               
                foreach (DetailItems detailItem in job_Details)
                {
                    Label captionLabel = new Label();
                    Label valueLabel = new Label();

                    StackLayout stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, Padding = new Thickness(0, 0, 0, 10) };

                    if(detailItem.Caption.Equals("Pickup") || detailItem.Caption.Equals("Drop-off"))
                    {
                        Title = detailItem.Value;
                    }

                    if (detailItem.Display == true)
                    {
                       
                        if (!(String.IsNullOrEmpty(detailItem.Caption)))
                        {

                            captionLabel.FontAttributes = FontAttributes.Bold;
                            captionLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                            captionLabel.Text = detailItem.Caption + ":  ";
                            captionLabel.WidthRequest = 120;

                            valueLabel.FontAttributes = FontAttributes.Bold;
                            valueLabel.Text = detailItem.Value;

                            stackLayout.Children.Add(captionLabel);
                            stackLayout.Children.Add(valueLabel);
                            jobDetails.Children.Add(stackLayout);
                        }
                        else
                        {
                            if(!(String.IsNullOrEmpty(detailItem.Value)))
                            {
                                captionLabel.FontAttributes = FontAttributes.Bold;
                                captionLabel.HorizontalOptions = LayoutOptions.FillAndExpand;
                                captionLabel.Text = detailItem.Value ;
                                jobDetails.Children.Add(captionLabel);
                           }
                        }
                    }
                }
            }

        }

        //bring user to the futile trip screen
        public async void futileTrip(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new TransportScreen.Futile_CargoReturn("Futile",jobID,eventRecordID));
        }

        //activate phone when user press the phone icon 
        public void callTelNo(object sender, EventArgs e)
        {
            Device.OpenUri(new Uri(String.Format("tel:{0}", telNo)));
        }

        //activate map when user press the map icon
        public void navigateToDest(object sender, EventArgs e)
        {

            Device.OpenUri(new Uri(String.Format("geo:{0}", location)));
        }

        public async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(jobID, this);
            UploadImage(jobID);
            try
            {
       
                images.Clear();
                imageGrid.Children.Clear();
               
                images = App.Database.GetUplodedRecordImagesAsync(jobID, "NormalImage");

                foreach (AppImage Image in images)
                {
                    byte[] imageByte;

                    IFile actualFile = await FileSystem.Current.GetFileFromPathAsync(Image.photoThumbnailFileLocation);
                    Stream stream = await actualFile.OpenAsync(PCLStorage.FileAccess.Read);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.Position = 0; // needed for WP (in iOS and Android it also works without it)!!
                        stream.CopyTo(ms);  // was empty without stream.Position = 0;
                        imageByte = ms.ToArray();
                    }
                    var image = new Image();
                    image.Source = ImageSource.FromStream(() => new MemoryStream(imageByte));
                    AddThumbnailToImageGrid(image, Image);
                }
            }
            catch
            {
                await DisplayAlert("display", "Error on display", "ok");
            }

        }

        public async void updateJob(object sender, EventArgs e)
        {
            bool done =false;

            if (signatureStack.IsVisible == true)
            {
                Stream signatureImage = await signature.GetImageStreamAsync(SignatureImageFormat.Png, strokeColor: Color.Black, fillColor: Color.White);

                if (signatureImage != null)
                {
                    await CommonFunction.StoreSignature(jobID, signatureImage, this);
                    UploadImage(jobID);
                    done = true;
                }
                else
                {
                    await DisplayAlert("Signature problem", "Please sign for the job.", "OK");
                }
            }

            if (done == true || signatureStack.IsVisible == false)
            {
                confirm_icon.IsEnabled = false;
                confirm_icon.Source = "confirmDisable.png";
                futile_icon.IsEnabled = false;
                futile_icon.Source = "futileDisable.png";

                clsTruckingModel truck = new clsTruckingModel();
                truck.Id = jobID;
                if(!(String.IsNullOrEmpty(remarkTextEditor.Text)))
                {
                    truck.Remarks = remarkTextEditor.Text;
                }
                else
                {
                    truck.Remarks = " ";
                }

                var content = await CommonFunction.PostRequestAsync(truck, Ultis.Settings.SessionBaseURI, ControllerUtil.postJobDetailURL(action));
                clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(update_response.IsGood)
                {
                    Ultis.Settings.CargoReturn = jobID + "," + eventRecordID + "," + reqSign;
                    await DisplayAlert("Success", "Job updated.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", update_response.Message, "OK");
                    confirm_icon.IsEnabled = true;
                    confirm_icon.Source = "confirm.png";
                    futile_icon.IsEnabled = true;
                    futile_icon.Source = "futile.png";
                }
            }
        }

        public async void UploadImage(string uploadID)
        {
            int uploadedImage = 0;
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
                    scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100);
                    image.Content = scaledImageByte;
                }

                image.FileName = recordImage.photoFileName;

                var content = await CommonFunction.PostRequestAsync(image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventRecordID.ToString()));
                clsResponse image_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (image_response.IsGood == true)
                {
                    uploadedImage++;
                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
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

    }
}
