using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using PCLStorage;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
    public partial class CheckList2 : ContentPage
    {
        public CheckList previousPage;
        string uri, display, faultyItems, linkId;
        double imageWidth;
        List<clsKeyValue> itemList;
        List<AppImage> images = new List<AppImage>();
        List<AppImage> listImage = new List<AppImage>();
        bool firstline = true;

        public CheckList2(List<clsKeyValue> chkList, string id)
        {
            InitializeComponent();

            Title = "Check List (step 2)";
            linkId = id;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGridRow.Height = imageWidth;
            itemList = new List<clsKeyValue>(chkList);
            faultyItems = "";

            if (itemList.Count != 0)
            {
                if (Ultis.Settings.Language == "English")
                {
                    lblReminder.Text = "* Please take photo for faulty check items";
                    display = " faulty items: ";
                }
                else
                {
                    lblReminder.Text = "* Sila ambil foto untuk barang semak yang rosak";
                    display = " barang rosak: ";
                }

                lblCount.Text = itemList.Count.ToString() + display;
                foreach (clsKeyValue item in itemList)
                {
                    if (firstline)
                    {
                        faultyItems += item.Value;
                        firstline = false;
                    }
                    else
                    {
                        faultyItems += ", " + item.Value;
                    }
                }
                lblCheckedItem.Text = faultyItems;
            }
            else
            {
                lblCount.IsVisible = false;
                lblCheckedItem.IsVisible = false;
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

        private void AddThumbnailToImageGrid(Image image, AppImage appImage)
        {
            image.HeightRequest = imageWidth;
            image.HorizontalOptions = LayoutOptions.FillAndExpand;
            image.VerticalOptions = LayoutOptions.FillAndExpand;
            image.Aspect = Aspect.AspectFill;
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.CommandParameter = appImage;
            tapGestureRecognizer.Tapped += async (sender, e) => await Navigation.PushAsync(new ImageViewer((AppImage)((TappedEventArgs)e).Parameter));
            image.GestureRecognizers.Add(tapGestureRecognizer);
            int noOfImages = imageGrid.Children.Count();
            int noOfCols = imageGrid.ColumnDefinitions.Count();
            int rowNo = noOfImages / noOfCols;
            int colNo = noOfImages - (rowNo * noOfCols);
            imageGrid.Children.Add(image, colNo, rowNo);
        }

        public async void takeImage(object sender, EventArgs e)
        {
            lblReminder.IsVisible = false;

            await CommonFunction.StoreImages(linkId, this, "NormalImage"); 

            DisplayImage();

            BackgroundTask.StartTimer();
        }

        public async void DisplayImage()
        {
            images.Clear();
            imageGrid.Children.Clear();
            images = App.Database.GetUplodedRecordImagesAsync(linkId, "NormalImage");
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

        public async void confirmCheck(object sender, EventArgs e)
        {
            if (Odometer.Text != null && Convert.ToInt32(Odometer.Text) != 0 && Convert.ToInt32(Odometer.Text) <= 9999999)
            {
                try
                {
                    if (itemList.Count == 0)
                    {
                        uri = ControllerUtil.postCheckList(true, remarkEditor.Text, Convert.ToInt32(Odometer.Text));
                    }
                    else
                    {
                        uri = ControllerUtil.postCheckList(false, remarkEditor.Text, Convert.ToInt32(Odometer.Text));
                    }

                    var content = await CommonFunction.CallWebService(1, itemList, Ultis.Settings.SessionBaseURI, uri, this);
                    clsResponse result = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (result.IsGood == true)
                    {
                        confirm_icon.IsEnabled = false;
                        confirm_icon.Source = "confirmDisable.png";

                        await DisplayAlert("Success", "Check List uploaded", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Upload Error", result.Message, "OK");
                    }

                    await Navigation.PopToRootAsync();
                }
                catch (HttpRequestException httpException)
                {
                    await DisplayAlert("Error", httpException.Message, "OK");
                }
                catch (Exception exception)
                {
                    await DisplayAlert("Error", exception.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Odometer cannot be empty and excedd 999999", "OK");
            }
        }

    }
}