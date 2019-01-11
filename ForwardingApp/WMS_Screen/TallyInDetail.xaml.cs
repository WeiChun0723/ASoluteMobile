using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Java.Nio.FileNio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using Xamarin.Forms;
using System.IO;
using System.Linq;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class TallyInDetail : ContentPage
    {
        clsWhsCommon tallyInDetail;
        double imageWidth;
        List<AppImage> images = new List<AppImage>();
        string id;

        public TallyInDetail(string tallyInID)
        {
            InitializeComponent();

            id = tallyInID;
        

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

            loading.IsVisible = true;

            GetTallyInDetail();

        }

        async void GetTallyInDetail()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.loadTallyInDetail(id));
            clsResponse tallyIn_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyIn_response.IsGood)
            {
                tallyInDetail = JObject.Parse(content)["Result"].ToObject<clsWhsCommon>();

                Title = "Tally In # " + tallyInDetail.DocumentNo;

                asn.Text = tallyInDetail.ContainerId;
                equipment.Text = tallyInDetail.ContainerNo;

                dataGrid.ItemsSource = tallyInDetail.Items;

            }
            else
            {
                await DisplayAlert("Error", tallyIn_response.Message, "OK");
            }

            loading.IsVisible = false;
        }

        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            clsWhsSummary product = new clsWhsSummary();
            product = ((clsWhsSummary)e.RowData);

            if (product != null)
            {
                await Navigation.PushAsync(new TallyInPalletEntry(product, id, tallyInDetail.DocumentNo));
            }
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetTallyInDetail();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }


        async void confirmTallyIn(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }

        async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(id, this);
            DisplayImage();
            BackgroundTask.StartTimer();
        }

        async void DisplayImage()
        {
            images.Clear();
            imageGrid.Children.Clear();

            images = App.Database.GetUplodedRecordImagesAsync(id, "NormalImage");

            foreach (AppImage Image in images)
            {
                byte[] imageByte;

                IFile actualFile = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(Image.photoThumbnailFileLocation);
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
