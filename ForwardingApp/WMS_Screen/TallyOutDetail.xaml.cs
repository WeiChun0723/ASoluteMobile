using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using PCLStorage;
using System.IO;
using System.Linq;
using ZXing.Net.Mobile.Forms;
using Acr.UserDialogs;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class TallyOutDetail : ContentPage
    {
        clsWhsHeader tallyOutDetail;
        string tallyOutID;
        double imageWidth;
        List<AppImage> images = new List<AppImage>();
        List<clsPalletTrx> palletTrxs = new List<clsPalletTrx>();
        List<clsWhsItem> tallyOutItems = new List<clsWhsItem>();

        bool tapped = true;

        public TallyOutDetail(string id)
        {
            InitializeComponent();

            tallyOutID = id;

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

            GetTallyOutDetail();

            if(tallyOutItems.Count != 0)
            {
                dataGrid.IsVisible = true;
                dataGrid.ItemsSource = tallyOutItems;
            }

        }

        async void GetTallyOutDetail()
        {

            try
            {
                loading.IsVisible = true;

                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.loadTallyOutDetail(tallyOutID));
                clsResponse tallyIn_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (tallyIn_response.IsGood)
                {
                    tallyInDesc.Children.Clear();

                    tallyOutDetail = JObject.Parse(content)["Result"].ToObject<clsWhsHeader>();

                    Label topBlank = new Label();
                    tallyInDesc.Children.Add(topBlank);

                    foreach (clsCaptionValue desc in tallyOutDetail.Summary)
                    {
                        Label caption = new Label();
                        caption.FontSize = 13;

                        if (desc.Caption.Equals(""))
                        {
                            caption.Text = "    " + desc.Value;
                            caption.FontAttributes = FontAttributes.Bold;
                        }
                        else
                        {
                            caption.Text = "    " + desc.Caption + ": " + desc.Value;
                        }

                        if (desc.Caption.Equals(""))
                        {
                            Title = "Tally Out # " + desc.Value;
                        }

                        tallyInDesc.Children.Add(caption);
                    }

                    Label bottomBlank = new Label();
                    tallyInDesc.Children.Add(bottomBlank);


                }

                loading.IsVisible = false;
            }
            catch(Exception)
            {
                await Navigation.PopAsync();
            }

        }


        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetTallyOutDetail();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }

        async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(tallyOutDetail.EventRecordId.ToString(), this, "NormalImage");
            DisplayImage();
            BackgroundTask.StartTimer();
        }

        async void scanBarCode(object sender, EventArgs e)
        {
            try
            {
                if (tapped)
                {
                    tapped = false;
                    try
                    {
                        dataGrid.IsVisible = false;
                        dataGrid.ItemsSource = null;

                        var scanPage = new ZXingScannerPage();
                        await Navigation.PushAsync(scanPage);

                        scanPage.OnScanResult += (result) =>
                        {
                            scanPage.PauseAnalysis();

                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                clsPalletTrx tallyOutPallet = new clsPalletTrx
                                {
                                    LinkId = tallyOutID,
                                    Id = result.Text
                                };

                                var content = await CommonFunction.PostRequestAsync(tallyOutPallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postTallyOutDetail());
                                clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (upload_response.IsGood)
                                {
                                    displayToast("Success");
                                    clsWhsItem item = new clsWhsItem
                                    {
                                        PalletId = result.Text,
                                        Description = Ultis.Settings.SessionUserId
                                    };

                                    tallyOutItems.Add(item);
                                    scanPage.ResumeAnalysis();
                                }
                                else
                                {
                                    displayToast(upload_response.Message);
                                    scanPage.ResumeAnalysis();
                                }
                            });

                        };
                    }
                    catch 
                    {
                       
                    }

                    tapped = true;
                }
            }
            catch
            {
               
            }
           
        }

        async void DisplayImage()
        {
            images.Clear();
            imageGrid.Children.Clear();

            images = App.Database.GetUplodedRecordImagesAsync(tallyOutID, "NormalImage");

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

        public void displayToast(string message)
        {
            var toastConfig = new ToastConfig(message);
            toastConfig.SetDuration(2000);
            toastConfig.Position = 0;
            toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
            if (message == "Success")
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(0, 128, 0));
            }
            else 
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(255, 0, 0));
            }
            UserDialogs.Instance.Toast(toastConfig);
        }
    }
}
