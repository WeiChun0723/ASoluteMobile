using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class WMS_DetailsPage : ContentPage
    {
        string uri,id, title, pickingID;
        clsWhsHeader recordDetails;
        double imageWidth;
        List<AppImage> images = new List<AppImage>();
        List<clsWhsItem> tallyOutItems = new List<clsWhsItem>();

        public WMS_DetailsPage(string recordUri,string recordId, string type)
        {
            InitializeComponent();

            uri = recordUri;
            id = recordId;
            title = type;

            switch (type)
            {
                case "Tally In":
                    camera_icon.IsVisible = true;
                    imageGrid.IsVisible = true;
                    break;
                case "Packing":
                    btnPackNow.IsVisible = true;
                    break;
                case "Tally Out":
                    camera_icon.IsVisible = true;
                    imageGrid.IsVisible = true;
                    barcode_icon.IsVisible = true;
                    break;
             
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

            GetRecordDetails();

            if (tallyOutItems.Count != 0 && title.Equals("Tally Out"))
            {
                dataGrid.IsVisible = true;
                dataGrid.ItemsSource = tallyOutItems;
            }
        }


        public async void GetRecordDetails()
        {
            loading.IsVisible = true;

            try
            {
                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, uri);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    recordDetails = JObject.Parse(content)["Result"].ToObject<clsWhsHeader>();

                    desc.Children.Clear();

                    Label topBlank = new Label();
                    desc.Children.Add(topBlank);
                    foreach (clsCaptionValue summary in recordDetails.Summary)
                    {
                        Label caption = new Label();
                        caption.FontSize = 13;
                        if (summary.Caption.Equals(""))
                        {
                            caption.Text = "    " + summary.Value;
                            caption.FontAttributes = FontAttributes.Bold;
                            Title = title + " # " + summary.Value;
                        }
                        else
                        {
                            caption.Text = "    " + summary.Caption + ": " + summary.Value;
                        }

                        if (summary.Caption.Equals(""))
                        {
                            Title = title + " # " + summary.Value;
                        }

                        desc.Children.Add(caption);
                    }
                    Label bottomBlank = new Label();
                    desc.Children.Add(bottomBlank);

                    dataGrid.AutoGenerateColumns = false;
                    dataGrid.ItemsSource = recordDetails.Items;

                    dataGrid.Columns.Clear();

                    foreach (clsKeyValue gridField in recordDetails.ItemColumns)
                    {
                        GridTextColumn gridColumn = new GridTextColumn();
                        gridColumn.MappingName = gridField.Key;
                        gridColumn.Width = (recordDetails.ItemColumns.Count == 3) ? 100 : 150;

                        gridColumn.HeaderTemplate = new DataTemplate(() =>
                        {
                            ViewCell viewCell = new ViewCell();

                            Label label = new Label
                            {
                                Text = gridField.Value,
                                BackgroundColor = Color.Transparent,
                                VerticalOptions = LayoutOptions.CenterAndExpand,
                                HorizontalOptions = LayoutOptions.CenterAndExpand,
                                HorizontalTextAlignment = TextAlignment.Center,
                                FontAttributes = FontAttributes.Bold
                            };

                            viewCell.View = label;
                            return viewCell;
                        });

                        dataGrid.Columns.Add(gridColumn);
                    }

                    if (!(String.IsNullOrEmpty(recordDetails.Id)))
                    {
                        pickingID = recordDetails.Id;
                    }
                }
                else
                {
                    await DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            loading.IsVisible = false;
        }

        async void Handle_GridTapped(object sender, GridTappedEventArgs e)
        {
            if(title.Equals("Tally In") || title.Equals("Full Pick") || title.Equals("Loose Pick"))
            {
                clsWhsItem product = new clsWhsItem();
                product = ((clsWhsItem)e.RowData);

                if (product != null)
                {
                    if(title.Equals("Tally In"))
                    {
                        await Navigation.PushAsync(new TallyInPalletEntry(product, id));
                    }
                    else
                    {
                        await Navigation.PushAsync(new PickingEntry(product, pickingID,title, recordDetails.Items));
                    }
                   
                }
            }
        }

        async void Packing_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new PackingEntry(id, Title));
        }

        async void scanBarCode(object sender, EventArgs e)
        {
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
                            LinkId = id,
                            Id = result.Text
                        };

                        var content = await CommonFunction.PostRequestAsync(tallyOutPallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postTallyOutDetail());
                        clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (upload_response.IsGood)
                        {
                            DisplayToast("Success");
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
                            DisplayToast(upload_response.Message);
                            scanPage.ResumeAnalysis();
                        }
                    });

                };
            }
            catch
            {

            }
        }

        public void DisplayToast(string message)
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

        async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(recordDetails.EventRecordId.ToString(), this);
            DisplayImage();
            BackgroundTask.StartTimer();
        }

        async void DisplayImage()
        {
            try
            {
                images.Clear();
                imageGrid.Children.Clear();

                images = App.Database.GetUplodedRecordImagesAsync(recordDetails.EventRecordId.ToString(), "NormalImage");

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
            catch
            {
                await DisplayAlert("Error", "Record event id cannot be null", "OK");
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
