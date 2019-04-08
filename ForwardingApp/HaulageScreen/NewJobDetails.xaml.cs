using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ASolute_Mobile.WoosimPrinterService;
using ASolute_Mobile.WoosimPrinterService.library.Cmds;
using Newtonsoft.Json;
using PCLStorage;
using Rg.Plugins.Popup.Services;
using SignaturePad.Forms;
using Xamarin.Forms;
using XLabs.Forms.Controls;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class NewJobDetails : ContentPage
    {
        ListItems jobItem;
        List<DetailItems> jobDetails;
        List<AppImage> images = new List<AppImage>();
        clsHaulageModel haulageJob = new clsHaulageModel();
        string jobCode, bookingCode;
        bool connectedPrinter = false, signed = false;
        double imageWidth;
        

        public NewJobDetails()
        {
            InitializeComponent();

            //initialized height for image grid row
            imageWidth = App.DisplayScreenWidth / 3;
            imageGridRow.Height = imageWidth;

            //get job info from db
            if (jobItem == null)
            {
                jobItem = App.Database.GetJobRecordAsync(Ultis.Settings.SessionCurrentJobId);
                jobDetails = App.Database.GetDetailsAsync(Ultis.Settings.SessionCurrentJobId);
            }

            //display all the job details
            JobContent();

            //title and subtitle
            StackLayout main = new StackLayout();
            Label title1 = new Label
            {
                FontSize = 15,
                Text = (!(String.IsNullOrEmpty(jobItem.Title)) ? jobItem.Title : "Job"),
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

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //disconnect printer when this screen not visible
            if (connectedPrinter == true)
            {
                DependencyService.Get<IBthService>().disconnBTDevice();
            }
        }

        void JobContent()
        {
            //display all the job details
            int count = 0;
            foreach (DetailItems detailItem in jobDetails)
            {
                Label label = new Label();

                if (detailItem.Caption == "Pickup" || detailItem.Caption == "Drop-off")
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;
                }
                else if (detailItem.Caption == "")
                {
                    label.Text = detailItem.Value;

                }
                else if (count == 0)
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;
                    count++;
                    jobCode = detailItem.Value;

                }
                else if (detailItem.Caption == "Booking")
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;
                    bookingCode = detailItem.Value;
                }
                else
                {
                    label.Text = detailItem.Caption + ":  " + detailItem.Value;

                }

                label.FontAttributes = FontAttributes.Bold;

                StackLayout stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, Padding = new Thickness(0, 5, 0, 5) };
                stackLayout.Children.Add(label);
                jobDesc.Children.Add(stackLayout);
            }

            map_icon.IsVisible = (!(String.IsNullOrEmpty(jobItem.Latitude))) ? true : false;
            phone_icon.IsVisible = (!(String.IsNullOrEmpty(jobItem.TelNo))) ? true : false;

            trailerIDEntry.Text = jobItem.TrailerId;
            trailerIDEntry.IsEnabled = (!(String.IsNullOrEmpty(jobItem.TrailerId))) ? false : true;
            //indicate which control to be show
            if (!(String.IsNullOrEmpty(jobItem.ActionId)))
            {
                switch (jobItem.ActionId)
                {
                    case "EmptyPickup":

                        signatureStack.IsVisible = (jobItem.ReqSign) ? true : false;
                        SplitContainerNumber();
                        break;

                    case "Point1_Chk" : case "Point2_Chk":
                        TrailerDetailGrid.IsVisible = false;
                        signatureStack.IsVisible = (jobItem.ReqSign) ? true : false;
                        SplitContainerNumber();
                        break;
                }

            }

            sealNoEntry.Text = jobItem.SealNo;
            //indicate the seal no is mandatory, optional or read only
            if (jobItem.ActionId == "Point1_Chk" || jobItem.ActionId == "Point2_Chk")
            {
                switch (jobItem.SealMode)
                {
                    case "R":
                        sealNoEntry.IsEnabled = false;
                        break;

                    case "O":
                        sealNoEntry.IsEnabled = true;
                        break;

                    case "M":
                        sealNoEntry.IsEnabled = true;
                        sealNoEntry.LineColor = Color.LightYellow;
                        break;

                    case "H":
                        sealNo.IsVisible = false;
                        sealNoEntry.IsVisible = false;
                        break;
                }
            }

        }

        void SplitContainerNumber()
        {
            if(!(String.IsNullOrEmpty(jobItem.ContainerNo)))
            {
                try
                {
                    string input = jobItem.ContainerNo;
                    contPrefix.Text = input.Substring(0, 4);
                    contNum.Text = input.Substring(4, 7);
                    contPrefix.IsEnabled = false;
                    contNum.IsEnabled = false;
                }
                catch (Exception exception)
                {
                    DisplayAlert("Sub string Error", exception.Message, "OK");
                }
            }

        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var field = sender as CustomEntry;

            switch (field.StyleId)
            {
                case "contPrefix":
                    if (contPrefix.Text.Length == 4)
                    {
                        contNum.Focus();
                    }
                    break;

                case "mgwEntry":
                    if (mgwEntry.Text.Length == 5)
                    {
                        tareEntry.Focus();
                    }
                    break;
            }
        }

        void Handle_CheckedChanged(object sender, XLabs.EventArgs<bool> e)
        {
            var chkBox = sender as CheckBox;

            switch (chkBox.StyleId)
            {
                case "chkYes":
                    if (chkNo.Checked && chkYes.Checked)
                    {
                        chkNo.Checked = false;

                    }
                    if (TrailerDetailGrid.IsVisible)
                    {
                        mgwEntry.Focus();
                    }
                    break;

                case "chkNo":
                    if (chkYes.Checked && chkNo.Checked)
                    {
                        chkYes.Checked = false;

                    }
                    break;
            }

        }

        async void IconTapped(object sender, EventArgs e)
        {
            var image = sender as Image;

            //indicate each icon function in this page
            switch (image.StyleId)
            {
                case "phone_icon":
                    Device.OpenUri(new Uri(String.Format("tel:{0}", jobItem.TelNo)));
                    break;

                case "map_icon":
                    Device.OpenUri(new Uri(String.Format("geo:{0}", jobItem.Latitude + "," + jobItem.Longitude)));
                    break;

                case "barCode_icon":
                    await PopupNavigation.Instance.PushAsync(new BarCodePopUp(jobCode, bookingCode));
                    break;

                case "print_icon":
                    PrintConsigmentNote();
                    break;

                case "camera_icon":
                    await CommonFunction.StoreImages(jobItem.EventRecordId.ToString(), this, "NormalImage");
                    await DisplayImage(); 
                    BackgroundTask.StartTimer();
                    break;

                case "futile_icon":
                    await Navigation.PushAsync(new FutileTrip_CargoReturn());
                    break;

                case "confirm_icon":
                    UpdateJob();
                    break;
            }
        }

        async void UpdateJob()
        {
            try
            {
                bool done = false;

                if (signatureStack.IsVisible == true)
                {
                    Stream signatureImage = await signature.GetImageStreamAsync(SignatureImageFormat.Png, strokeColor: Color.Black, fillColor: Color.White);

                    if (signatureImage != null)
                    {
                        if(signed == false)
                        {
                            await CommonFunction.StoreSignature(jobItem.EventRecordId.ToString(), signatureImage, this);
                            done = true;
                            BackgroundTask.StartTimer();

                            signed = true;
                        }
                    }
                    else
                    {
                        await DisplayAlert("Signature Error", "Please sign for the job.", "OK");

                    }
                }

                if (done == true || signatureStack.IsVisible == false)
                {
                    if (!(String.IsNullOrEmpty(trailerIDEntry.Text)) && !(String.IsNullOrEmpty(contPrefix.Text)) && !(String.IsNullOrEmpty(contNum.Text)))
                    {
                        clsCommonFunc checker = new clsCommonFunc();
                        bool status = checker.CheckContainerDigit(contPrefix.Text + contNum.Text);

                        if (status)
                        {
                            InitializeObject();
                        }
                        else
                        {
                            var answer = await DisplayAlert("Error", "Invalid container check digit, continue?", "Yes", "No");
                            if (answer.Equals(true))
                            {
                                InitializeObject();
                            }
                            else
                            {
                                contNum.Text = String.Empty;
                                contPrefix.Text = String.Empty;
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please enter all mandatory field.", "OK");
                      
                    }
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");

            }
        }

        async void InitializeObject()
        {
            loading.IsVisible = true;
            if (!(sealNoEntry.LineColor == Color.LightYellow) || (sealNoEntry.LineColor == Color.LightYellow && !(String.IsNullOrEmpty(sealNoEntry.Text))))
            {
                try
                {
                    if (jobItem.ActionId == "EmptyPickup")
                    {
                        haulageJob.ActionId = clsHaulageModel.HaulageActionEnum.EmptyPickup;
                        haulageJob.CollectSeal = (chkYes.Checked) ? true : false;
                        haulageJob.MaxGrossWeight = (!(String.IsNullOrEmpty(mgwEntry.Text))) ? Convert.ToInt32(mgwEntry.Text) : 0;
                        haulageJob.TareWeight = (!(String.IsNullOrEmpty(tareEntry.Text))) ? Convert.ToInt32(tareEntry.Text) : 0;
                    }
                    else
                    {
                        haulageJob.ActionId = (jobItem.ActionId == "Point1_Chk") ? clsHaulageModel.HaulageActionEnum.Point1_Chk : clsHaulageModel.HaulageActionEnum.Point2_Chk;
                    }

                    haulageJob.Id = Ultis.Settings.SessionCurrentJobId;
                    haulageJob.ContainerNo = contPrefix.Text + contNum.Text;
                    haulageJob.TrailerId = trailerIDEntry.Text;
                    haulageJob.Remarks = (!(String.IsNullOrEmpty(remarkTextEditor.Text))) ? remarkTextEditor.Text : "";
                    haulageJob.SealNo = (!(String.IsNullOrEmpty(sealNoEntry.Text))) ? sealNoEntry.Text : "";

                    var content = await CommonFunction.CallWebService(1, haulageJob, Ultis.Settings.SessionBaseURI, ControllerUtil.updateHaulageJobURL(), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if(response.IsGood)
                    {
                        Ultis.Settings.RefreshMenuItem = "Yes";
                        Ultis.Settings.UpdatedRecord = "RefreshJobList";

                        if (Ultis.Settings.Language.Equals("English"))
                        {

                            await DisplayAlert("Success", "Job updated", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Berjaya", "Kemas kini berjaya.", "OK");
                        }

                        confirm_icon.IsEnabled = false;
                        confirm_icon.Source = "confirmDisable.png";
                        futile_icon.IsEnabled = false;
                        futile_icon.Source = "futileDisable.png";
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
            }
            else
            {
                await DisplayAlert("Error", "Please enter seal number.", "OK");
            }

            loading.IsVisible = false;
        }

        //add capture image to the image grid
        private void AddThumbnailToImageGrid(Image image, AppImage appImage)
        {

            try
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
                //image.GestureRecognizers.Add(tapGestureRecognizer);
                int noOfImages = imageGrid.Children.Count();
                int noOfCols = imageGrid.ColumnDefinitions.Count();
                int rowNo = noOfImages / noOfCols;
                int colNo = noOfImages - (rowNo * noOfCols);
                imageGrid.Children.Add(image, colNo, rowNo);
            }
            catch
            {

            }
        }

        public async Task DisplayImage()
        {
            try
            {
                images.Clear();
                imageGrid.Children.Clear();
                images = App.Database.GetUplodedRecordImagesAsync(jobItem.EventRecordId.ToString(), "NormalImage");
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

            }

        }

        async void PrintConsigmentNote()
        {
            print.IsVisible = true;

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getConsigmentNoteURL(Ultis.Settings.SessionCurrentJobId), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (response.IsGood)
            {
                if (response.Result.Count != 0)
                {
                    try
                    {
                        if (connectedPrinter == false)
                        {
                            bool x = await DependencyService.Get<IBthService>().connectBTDevice("00:15:0E:E6:25:23");

                            if (!x)
                            {
                                Device.BeginInvokeOnMainThread(() =>
                               {
                                   DisplayAlert("Error", "Unable connect", "OK");
                               });
                            }
                            else
                            {
                                connectedPrinter = true;
                            }
                        }

                        System.IO.MemoryStream buffer = new System.IO.MemoryStream(512);
                        string detail = "";
                        int detailCount = 0;
                        foreach (string details in response.Result)
                        {
                            detailCount++;

                            if (details.Contains("<H>"))
                            {
                                detail = details.Replace("<H>", "");
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                            }
                            else if (details.Contains("<B>"))
                            {
                                detail = details.Replace("<B>", "");
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                            }
                            else
                            {
                                detail = details;
                                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, false, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                            }


                            if (detailCount == response.Result.Count)
                            {
                                detail = detail + "\r\n\r\n";
                            }
                            else
                            {
                                detail = detail + "\r\n";
                            }

                            WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(detail));
                        }

                        WriteMemoryStream(buffer, WoosimPageMode.print());

                        DependencyService.Get<IBthService>().WriteComm(buffer.ToArray());
                    }
                    catch (Exception error)
                    {
                        await DisplayAlert("Error", error.Message, "OK");
                    }
                }
            }
            else
            {
                await DisplayAlert("Json Error", response.Message, "OK");
            }

            print.IsVisible = false;
        }

        async private void WriteMemoryStream(System.IO.MemoryStream stream, byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
