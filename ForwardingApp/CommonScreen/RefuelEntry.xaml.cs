using ASolute.Mobile.Models;
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
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class RefuelEntry : ContentPage
	{
        public RefuelHistory previousPage;
        List<AppImage> images = new List<AppImage>();
        double imageWidth;
        clsResponse json_reponse = new clsResponse();
        clsFuelCostNew fuelCostNew = new clsFuelCostNew();         
        string newFuelID, imageEventID = "";
        int station_choice = 0, payment_choice;
        List<AppImage> recordImages = new List<AppImage>();
        byte[] scaledImageByte;

        public RefuelEntry ()
		{
            // this screen need to call web servvice to get the current date cost rate, so it do not allow user to enter new record when they is no internet to call the get cost rate web service.
			InitializeComponent ();

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

            //generate id for the image to link with the record for offline sync 
            Guid fuelRecord = Guid.NewGuid();
            newFuelID = fuelRecord.ToString();

            imageGrid.RowSpacing = 0;
            imageGrid.ColumnSpacing = 0;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            lblDateTime.Text = "Date & Time";           

            if (NetworkCheck.IsInternet())
            {
                DownloadDefaultValue();
            }
                    
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

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });

            if (Ultis.Settings.deleteImage == "Yes")
            {
                DisplayImage();
                Ultis.Settings.deleteImage = "No";
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        public async void CaptureImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(newFuelID, this);
            DisplayImage();

            if(!(String.IsNullOrEmpty(imageEventID)))
            {
                UploadImage();
            }
        }

        public async void ConfirmRefuel(object sender, EventArgs e)
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
                        if (!(fuelCostNew.PreviousOdometer >= odo))
                        {
                            //combine the date and time together               
                            string combineDate_Time = datePicker.Date.Year + "-" + datePicker.Date.Month + "-" + datePicker.Date.Day + "T" + timePicker.Time.ToString();

                            RefuelData refuel_Data = new RefuelData();
                            if (stationPicker.SelectedIndex != -1 && paymentPicker.SelectedIndex != -1 && liter.Text != null && fuelCard.Text != null)
                            {
                                try
                                {
                                    refuel_Data.ID = newFuelID;
                                    refuel_Data.Done = 0;
                                    refuel_Data.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                                    refuel_Data.Odometer = Convert.ToInt32(odometer.Text);
                                    refuel_Data.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                                    refuel_Data.VendorCode = fuelCostNew.VendorList[station_choice].Key;
                                    refuel_Data.PaymentMode = paymentPicker.SelectedIndex;
                                    refuel_Data.RefuelDateTime = Convert.ToDateTime(combineDate_Time);
                                    refuel_Data.Quantity = Convert.ToDouble(liter.Text);
                                    if (String.IsNullOrEmpty(fuelCard.Text))
                                    {
                                        refuel_Data.FuelCardNo = "";
                                    }
                                    else
                                    {
                                        refuel_Data.FuelCardNo = fuelCard.Text;
                                    }

                                    if (String.IsNullOrEmpty(voucher.Text))
                                    {
                                        refuel_Data.VoucherNo = "";
                                    }
                                    else
                                    {
                                        refuel_Data.VoucherNo = voucher.Text;
                                    }

                                    if (String.IsNullOrEmpty(other.Text))
                                    {
                                        refuel_Data.OtherRef = "";
                                    }
                                    else
                                    {
                                        refuel_Data.OtherRef = other.Text;
                                    }

                                    refuel_Data.CostRate = Convert.ToDouble(costPerLiter.Text);
                                    App.Database.SaveRecordAsync(refuel_Data);
                                    //await BackgroundTask.UploadLatestRecord(this);

                                    var content = await CommonFunction.CallWebService(1, refuel_Data, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewRecordURL());
                                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                                    if(response.IsGood)
                                    {
                                        imageEventID = response.Result["LinkId"];
                                        if (Ultis.Settings.Language.Equals("English"))
                                        {
                                            //CommonFunction.AppActivity("Add refuel record", "Succeed", status.Message);
                                            await DisplayAlert("Success", "Record added", "OK");
                                        }
                                        else
                                        {
                                            //CommonFunction.AppActivity("Isi minyak entri", "Berjaya", status.Message);
                                            await DisplayAlert("Berjaya", "Record baru ditambah", "OK");
                                        }

                                        Ultis.Settings.RefreshMenuItem = "Yes";
                                        confirm_icon.IsEnabled = false;
                                        confirm_icon.Source = "confirmDisable.png";
                                        UploadImage();
                                    }
                                    else
                                    {
                                        await DisplayAlert("", response.Message, "OK");
                                    }

                                }
                                catch
                                {
                                    string check = "";
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        check = "Please fill in all the field.";
                                    }
                                    else
                                    {
                                        check = "Sila isi semua data yang diperlukan.";
                                    }
                                    await DisplayAlert("Error", check, "OK");
                                }
                            }
                            else
                            {
                                string check = "";
                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    check = "Please fill in all the field.";
                                }
                                else
                                {
                                    check = "Sila isi semua data yang diperlukan.";
                                }
                                await DisplayAlert("Error", check, "OK");
                            }
                        }
                        else
                        {

                            string check = "";
                            if (Ultis.Settings.Language.Equals("English"))
                            {
                                check = "Odometer entered cannot less than default odometer value ";
                            }
                            else
                            {
                                check = "Odometer tidak boleh kurang daripda odometer asal";
                            }
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
                        check = "Sila isi semua data yang diperlukan.";
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
            if (paymentPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please choose payment method", "Ok");
            }
            else
            {
                payment_choice = paymentPicker.SelectedIndex;

                if(paymentPicker.SelectedIndex == 0)
                {
                    //voucher.BackgroundColor = Color.FromHex("#FFFFE0");
                    //voucher.LineColor = Color.LightYellow;
                }
            }
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

                    double result = Convert.ToDouble(costPerLiter.Text.ToString()) * fuelLiter;

                    amount.Text = "RM" + result.ToString();
                }
            }
            catch
            {
                DisplayAlert("Error", "Please try again", "OK");
            }

        }

        public void CostLiter(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (Convert.ToInt16(liter.Text) == 0)
                {
                    amount.Text = "RM 0.00";

                }
                else
                {

                    double costLiter = Convert.ToDouble(e.NewTextValue);

                    double result = Convert.ToDouble(liter.Text.ToString()) * costLiter;

                    amount.Text = "RM" + result.ToString();
                }
            }
            catch
            {
               
              
            }
           
        }

        public  void VoucherText(object sender, TextChangedEventArgs e)
        {
            string _vouchertext = voucher.Text.ToUpper();
            
            //Get Current Text
            if (_vouchertext.Length > 20)       
            {
                _vouchertext = _vouchertext.Remove(_vouchertext.Length - 1);  
               
                voucher.Unfocus();
            }
            voucher.Text = _vouchertext;
        }

        public  void OtherText(object sender, TextChangedEventArgs e)
        {
            string _othertext = other.Text.ToUpper();     
           
            if (_othertext.Length > 15)      
            {
                _othertext = _othertext.Remove(_othertext.Length - 1); 
                
                other.Unfocus();
            }

            other.Text = _othertext;
        }

        public  void Fuelscan(object sender, EventArgs e)
        {
            string field_name = "fuel";
            OnScanResult(field_name);
        }

        public  void Voucherscan(object sender, EventArgs e)
        {
            string field_name = "voucher";
            OnScanResult(field_name);
        }

        public  void Otherscan(object sender, EventArgs e)
        {
            string field_name = "otherRef";
            OnScanResult(field_name);
        }

        //function to handler scan function
        public async void OnScanResult(string field)
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

                    if (field == "fuel")
                    {
                        fuelCard.Text = result.Text;
                    }
                    else if (field == "voucher")
                    {

                        voucher.Text = result.Text;
                    }
                    else if (field == "otherRef")
                    {
                        other.Text = result.Text;
                    }
                });
            };
        }

        public async void DownloadDefaultValue()
        {
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {
                try
                {
                    // get the data from server as json
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var uri = ControllerUtil.getNewFuelCostURL();
                    var response = await client.GetAsync(uri);
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(content);
                    json_reponse = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (json_reponse.IsGood == true)
                    {
                        fuelCostNew = JObject.Parse(content)["Result"].ToObject<clsFuelCostNew>();
                      
                        foreach(clsCaptionValue language in fuelCostNew.Captions)
                        {
                            string lblCaption = language.Caption;

                            if (lblStation.Text == lblCaption)
                            {
                                lblStation.Text = language.Value;
                            }
                            else if (lblDateTime.Text == lblCaption)
                            {
                                lblDateTime.Text = language.Value;
                            }
                            else if (lblPayment.Text == lblCaption)
                            {
                                lblPayment.Text = language.Value;
                            }
                            else if (lblLiter.Text == lblCaption)
                            {
                                lblLiter.Text = language.Value;
                            }
                            else if (lblCost.Text == lblCaption)
                            {
                                lblCost.Text = language.Value;
                            }
                            else if (lblOdometer.Text == lblCaption)
                            {
                                lblOdometer.Text = language.Value;
                            }
                            else if (lblFuelCard.Text == lblCaption)
                            {
                                lblFuelCard.Text = language.Value;
                            }
                            else if (lblVoucher.Text == lblCaption)
                            {
                                lblVoucher.Text = language.Value;
                            }
                            else if (lblOtherRef.Text == lblCaption)
                            {
                                lblOtherRef.Text = language.Value;
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Json Error", json_reponse.Message, "OK");
                    }


                    timePicker.Time = fuelCostNew.RefuelDateTime.TimeOfDay;

                    costPerLiter.Text = String.Format("{0:0.00}", fuelCostNew.CostRate);
                    odometer.Text = fuelCostNew.PreviousOdometer.ToString();
                    if(fuelCostNew.FuelCardNo != "")
                    {
                        fuelCard.Text = fuelCostNew.FuelCardNo;
                    }                    
                    amount.Text = "RM 0.00";
                    datePicker.MaximumDate = DateTime.Now;
                 
                    for (int i = 0; i < fuelCostNew.VendorList.Count; i++)
                    {
                        stationPicker.Items.Add(fuelCostNew.VendorList[i].Value.ToUpper());
                    }

                    for (int j = 0; j < fuelCostNew.PaymentModes.Count; j++)
                    {
                        paymentPicker.Items.Add(fuelCostNew.PaymentModes[j].Value.ToUpper());
                    }

                    paymentPicker.SelectedIndex = 1;
                    stationPicker.SelectedIndex = 0;

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
            images = App.Database.GetUplodedRecordImagesAsync(newFuelID,"NormalImage");
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

                    var content = await CommonFunction.CallWebService(1, image, Ultis.Settings.SessionBaseURI, ControllerUtil.UploadImageURL(imageEventID));
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