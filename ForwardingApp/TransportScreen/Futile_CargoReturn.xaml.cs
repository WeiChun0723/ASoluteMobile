using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using SignaturePad.Forms;
using Xamarin.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class Futile_CargoReturn : ContentPage
    {
        double imageWidth;
        string jobID;
        private byte[] scaledImageByte;
        long eventRecordID;
        List<clsKeyValue> keyValue;
        string reqSign;

        public Futile_CargoReturn(string title,string id,long EventrecordID)
        {
            InitializeComponent();

            Title = (title.Equals("Futile")) ? "Futile Trip" : "Cargo Return";
            jobID = id;
            eventRecordID = EventrecordID;

            if(Title.Equals("Cargo Return"))
            {

                if (!(String.IsNullOrEmpty(Ultis.Settings.CargoReturn)))
                {
                    string[] sign = Ultis.Settings.CargoReturn.Split(',');
                    reqSign = sign[2];
                }

                ReferenceStack.IsVisible = true;

                SignatureStack.IsVisible = true;

            }

            ReasonList();

            imageGrid.RowSpacing = 0;
            imageGrid.ColumnSpacing = 0;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        }

        public async void ReasonList()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getReasonListURL());
            clsResponse reason_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(reason_response.IsGood)
            {
                keyValue = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                foreach(clsKeyValue reason in keyValue)
                {
                    ReasonPicker.Items.Add(reason.Value);
                }
            }
            else
            {
                await DisplayAlert("Error", reason_response.Message, "OK");
            }
        }

        public async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(jobID, this);
            UploadImage(jobID);

            try
            {
                List<AppImage> images = new List<AppImage>();

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
                await DisplayAlert("display", "Error on display", "OK");
            }

        }

        public async void uploadDetail(object sender, EventArgs e)
        {
            bool done = false;
            try
            {
                if (SignatureStack.IsVisible == true)
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
                        await DisplayAlert("Signature Error", "Please sign for the job.", "OK");
                    }
                }

                if (done == true || SignatureStack.IsVisible == false)
                {
                    clsTruckingModel futile = new clsTruckingModel();

                    futile.Id = "";
                    futile.Remarks = remarkTextEditor.Text;
                    futile.ReasonCode = keyValue[ReasonPicker.SelectedIndex].Key;

                    if(Title.Equals("Cargo Return"))
                    {
                        futile.RefNo = ReferenceNo.Text;
                    }

                    var content = await CommonFunction.PostRequest(futile, Ultis.Settings.SessionBaseURI, ControllerUtil.postFutileTripURL());
                    clsResponse futile_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (futile_response.IsGood)
                    {
                        await DisplayAlert("Success", "Job status updated as futile.", "OK");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", futile_response.Message, "OK");
                    }
                }
            }
            catch
            {
                
            }
        }

        public async void UploadImage(string imageID)
        {
            int uploadedImage = 0;
            List<AppImage> recordImages = App.Database.GetRecordImagesAsync(imageID, false);
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

                var content = await CommonFunction.PostRequest(image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(eventRecordID.ToString()));
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
