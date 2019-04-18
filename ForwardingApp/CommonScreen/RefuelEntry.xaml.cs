using ASolute.Mobile.Models;
using ASolute_Mobile.CommonScreen;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XLabs.Ioc;
using XLabs.Platform.Device;
using XLabs.Platform.Services.Media;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile
{

    public partial class RefuelEntry : ContentPage
    {
        List<AppImage> images = new List<AppImage>();
        double imageWidth;
        
        clsFuelCostNew fuelCostNew = new clsFuelCostNew();         
        string recordID, imageEventID = "";
        int station_choice = 0, payment_choice;
        List<AppImage> recordImages = new List<AppImage>();
        byte[] scaledImageByte;
        List<string> paymentMode = new List<string>();
        List<string> stations = new List<string>();

        public RefuelEntry(string title)
        {
            // this screen need to call web servvice to get the current date cost rate, so it do not allow user to enter new record when they is no internet to call the get cost rate web service.
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

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Refuel Entry";
                liter.Placeholder = "Maximum 500 liter.";
            }
            else
            {
                Title = "Isi Minyak";
                liter.Placeholder = "Maksimum 500 liter.";
            }

            if (NetworkCheck.IsInternet())
            {
                DownloadDefaultValue();
            }

            recordID = Guid.NewGuid().ToString();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) =>
            {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });

            if (Ultis.Settings.DeleteImage == "Yes")
            {
                DisplayImage();
                Ultis.Settings.DeleteImage = "No";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var icon = sender as Image;

            switch (icon.StyleId)
            {
                case "fuelCardBarCode_icon":
                    BarCodeScan("fuel");
                    break;

                case "voucherBarCode_icon":
                    BarCodeScan("voucher");
                    break;

                case "otherBarCode_icon":
                    BarCodeScan("otherRef");
                    break;

                case "camera_icon":
                    await CommonFunction.StoreImages(recordID, this, "NormalImage");
                    DisplayImage();

                    if (!(String.IsNullOrEmpty(imageEventID)))
                    {
                        UploadImage();
                    }
                    break;

                case "confirm_icon":
                    UploadRefuelRecord();
                    break;
            }
        }

        public async void UploadRefuelRecord()
        {
            try
            {
                double fuelLiter = Convert.ToDouble(liter.Text);
                if (fuelLiter <= 500.00)
                {
                    double cost = Convert.ToDouble(costPerLiter.Text);
                    if (cost != 0.00)
                    {
                        int odo = Convert.ToInt32(odometer.Text);
                        if (fuelCostNew.PreviousOdometer <= odo)
                        {
                            //combine the date and time together               
                            string combineDate_Time = datePicker.Date.Year + "-" + datePicker.Date.Month + "-" + datePicker.Date.Day + "T" + timePicker.Time.ToString();

                            clsFuelCost refuel_Data = new clsFuelCost();
                            if (paymentComboBox.Text != null && stationComboBox.Text != null && liter.Text != null && fuelCard.Text != null)
                            {
                                try
                                {
                                    refuel_Data.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                                    refuel_Data.Odometer = Convert.ToInt32(odometer.Text);
                                    refuel_Data.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                                    //refuel_Data.VendorCode = fuelCostNew.VendorList[0].Key;
                                    refuel_Data.VendorCode = fuelCostNew.VendorList[stations.FindIndex(x => x.Equals(stationComboBox.Text))].Key;
                                    refuel_Data.PaymentMode = (paymentMode.FindIndex(x => x.Equals(paymentComboBox.Text)) == 1) ? clsFuelCost.PaymentModeEnum.Card : clsFuelCost.PaymentModeEnum.Cash;
                                    refuel_Data.RefuelDateTime = Convert.ToDateTime(combineDate_Time);
                                    refuel_Data.Quantity = Convert.ToDouble(liter.Text);
                                    refuel_Data.FuelCardNo = (String.IsNullOrEmpty(fuelCard.Text)) ? "" : fuelCard.Text;
                                    refuel_Data.VoucherNo = (String.IsNullOrEmpty(voucher.Text)) ? "" : voucher.Text;
                                    //refuel_Data.OtherRef = (String.IsNullOrEmpty(other.Text)) ? "" : other.Text;
                                    refuel_Data.OtherRef = "";
                                    refuel_Data.CostRate = Convert.ToDouble(costPerLiter.Text);

                                    var content = await CommonFunction.PostRequestAsync(refuel_Data, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewRecordURL());
                                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                                    if (response.IsGood)
                                    {
                                        imageEventID = response.Result["LinkId"];
                                        if (Ultis.Settings.Language.Equals("English"))
                                        {
                                            await DisplayAlert("Success", "Record added", "OK");
                                        }
                                        else
                                        {
                                            await DisplayAlert("Berjaya", "Record baru ditambah", "OK");
                                        }

                                        Ultis.Settings.RefreshListView = "Yes";
                                        confirm_icon.IsEnabled = false;
                                        confirm_icon.Source = "confirmDisable.png";
                                        UploadImage();

                                        await Navigation.PopAsync();
                                    }
                                    else
                                    {
                                        await DisplayAlert("", response.Message, "OK");
                                    }

                                }
                                catch
                                {
                                    string check = (Ultis.Settings.Language.Equals("English")) ? "Please fill in all the field." : "Sila isi semua data yang diperlukan.";
                                 
                                    await DisplayAlert("Error", check, "OK");
                                }
                            }
                            else
                            {
                                string check = (Ultis.Settings.Language.Equals("English")) ? "Please fill in all the field." : "Sila isi semua data yang diperlukan.";
                        
                                await DisplayAlert("Error", check, "OK");
                            }
                        }
                        else
                        {

                            string check = (Ultis.Settings.Language.Equals("English")) ? "Odometer entered cannot less than default odometer value " : "Odometer tidak boleh kurang daripda odometer asal";
                         
                            await DisplayAlert("", check, "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Cost Per Liter cannot be 0.00", "OK");
                    }
                }
                else
                {
                    string check = "";
                    if (Ultis.Settings.Language.Equals("English"))
                    {
                        check = "Fuel liter should not exceed 500 liter";
                    }
                    else
                    {
                        check = "Minyak tidak boleh melebihi 500 liter.";
                    }
                    await DisplayAlert("", check, "OK");
                }
            }
            catch
            {
                await DisplayAlert("Error", "Odometer cannot contain decimal", "OK");
            }
        }    

        public async void PaymentSelected(object sender, SelectedPositionChangedEventArgs e)
        {
            if (paymentComboBox.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please choose payment method", "Ok");
            }
            else
            {
                payment_choice = paymentComboBox.SelectedIndex;

                if(paymentComboBox.SelectedIndex == 0)
                {
                    //voucher.BackgroundColor = Color.FromHex("#FFFFE0");
                    //voucher.LineColor = Color.LightYellow;
                }
            }
        }

        void StationSelected(object sender, System.EventArgs e)
        {
            station_choice = stationComboBox.SelectedIndex;
        }

        void Handle_TextChanged(object sender, TextChangedEventArgs e)
        {
            var entry = sender as Entry;


        }

        public void LiterInput(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.NewTextValue))
                {
                    amount.Text = "RM 0.00";
                    liter.Placeholder = "Maximum 500 liter only";
                }
                else
                {

                    double fuelLiter = Convert.ToDouble(e.NewTextValue);

                    double result = Convert.ToDouble(costPerLiter.Text) * fuelLiter;

                    amount.Text = "RM" + Math.Round(result, 2);
                }
            }
            catch
            {

            }

        }

        public void CostLiter(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Convert.ToDouble(liter.Text) == 0.00)
                {
                    amount.Text = "RM 0.00";

                }
                else
                {

                    double costLiter = Convert.ToDouble(e.NewTextValue);

                    double result = Convert.ToDouble(liter.Text) * costLiter;

                    amount.Text = "RM" + Math.Round(result, 2);
                }
            }
            catch
            {

            }

        }

        //function to handler scan function
        public async void BarCodeScan(string field)
        {
            var scanPage = new ZXingScannerPage();
            await Navigation.PushAsync(scanPage);

            scanPage.OnScanResult += (result) =>
            {
                scanPage.IsScanning = false;
                scanPage.IsAnalyzing = false;

                Device.BeginInvokeOnMainThread(async () =>
                {
                    await Navigation.PopAsync();

                    switch(field)
                    {
                        case "fuel":
                            fuelCard.Text = result.Text;
                            break;

                        case "voucher":
                            voucher.Text = result.Text;
                            break;

                        case "otherRef":
                           // other.Text = result.Text;
                            break;
                    }
                });
            };
        }

        public async void DownloadDefaultValue()
        {
            try
            {
                // get the data from server as json
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getNewFuelCostURL(), this);
                clsResponse json_reponse = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_reponse.IsGood == true)
                {
                    fuelCostNew = JObject.Parse(content)["Result"].ToObject<clsFuelCostNew>();

                    foreach (clsCaptionValue labelText in fuelCostNew.Captions)
                    {
                        switch (labelText.Caption)
                        {
                            case "Station":
                                lblStation.Text = labelText.Value;
                                break;

                            case "Date & Time":
                                lblDateTime.Text = labelText.Value;
                                break;

                            case "Payment":
                                lblPayment.Text = labelText.Value;
                                break;

                            case "Liter":
                                //lblLiter.Text = labelText.Value;
                                lblLiter.Hint = labelText.Value;
                                break;

                            case "Cost per Liter":
                                //lblCost.Text = labelText.Value;
                                lblCost.Hint = labelText.Value;
                                break;

                            case "Odometer":
                                //lblOdometer.Text = labelText.Value;
                                lblOdometer.Hint= labelText.Value;
                                break;

                            case "Fuel Card":
                                //lblFuelCard.Text = labelText.Value;
                                lblFuelCard.Hint = labelText.Value;
                                break;

                            case "Voucher":
                                //lblVoucher.Text = labelText.Value;
                                lblVoucher.Hint = labelText.Value;
                                break;

                            case "Other Ref":
                                //lblOtherRef.Text = labelText.Value;
                                break;

                        }
                    }


                    //assign default time
                    timePicker.Time = fuelCostNew.RefuelDateTime.TimeOfDay;
                    //assign default cost rate
                    costPerLiter.Text = String.Format("{0:0.00}", fuelCostNew.CostRate);
                    //assign default odometer
                    odometer.Text = fuelCostNew.PreviousOdometer.ToString();
                    //assign default fuel card number
                    fuelCard.Text = (fuelCostNew.FuelCardNo != "") ? fuelCostNew.FuelCardNo : "";
                    amount.Text = "RM 0.00";

                    //set maximum date that date picker can choose 
                    datePicker.MaximumDate = DateTime.Now;


                    foreach (clsKeyValue station in fuelCostNew.VendorList)
                    {
                        stations.Add(station.Value.ToUpper());
                    }
                    /*for (int i = 0; i < fuelCostNew.VendorList.Count; i++)
                    {
                        stationPicker.Items.Add(fuelCostNew.VendorList[i].Value.ToUpper());
                    }*/
                    stationComboBox.ComboBoxSource = stations;

                    //auto select deafult station if picker only 1 item
                    if (fuelCostNew.VendorList.Count == 1)
                    {
                        stationComboBox.Text = stations[0];
                    }


                    foreach (clsKeyValue payment in fuelCostNew.PaymentModes)
                    {
                        paymentMode.Add(payment.Value.ToUpper());
                    }
                    paymentComboBox.ComboBoxSource = paymentMode;
                    paymentComboBox.Text = paymentMode[1];
                    /*for (int j = 0; j < fuelCostNew.PaymentModes.Count; j++)
                    {
                        paymentPicker.Items.Add(fuelCostNew.PaymentModes[j].Value.ToUpper());
                    }*/


                }

            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Download detail error", exception.Message, "Ok");
            }
        }

        //add capture image to the image grid
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
            images.Clear();
            imageGrid.Children.Clear();
            images = App.Database.GetUplodedRecordImagesAsync(recordID, "NormalImage");
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

        async void UploadImage()
        {
            try
            {
                recordImages = App.Database.GetPendingRecordImages(false);
                foreach (AppImage recordImage in recordImages)
                {
                    clsFileObject image = new clsFileObject();

                    byte[] originalPhotoImageBytes = File.ReadAllBytes(recordImage.photoFileLocation);
                    scaledImageByte = DependencyService.Get<IThumbnailHelper>().ResizeImage(originalPhotoImageBytes, 1024, 1024, 100);
                    image.Content = scaledImageByte;
                    image.FileName = recordImage.photoFileName;

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(imageEventID), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
                }
            }
            catch
            {

            }
        }
    }
}