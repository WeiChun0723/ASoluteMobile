using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Forwarding;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ASolute_Mobile.WoosimPrinterService;
using ASolute_Mobile.WoosimPrinterService.library.Cmds;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using Rg.Plugins.Popup.Services;
using SignaturePad.Forms;
using Syncfusion.XForms.Buttons;
using Xamarin.Essentials;
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
        public JobLists previousPage;

        public NewJobDetails()
        {
            InitializeComponent();

            switch (Ultis.Settings.App)
            {

                case "asolute.Mobile.Forwarding":
                    NavigationPage.SetHasNavigationBar(this, false);
                    break;

                case "asolute.Mobile.AILSHaulage":
                    haulageJobStack.IsVisible = true;
                    barCode_icon.IsVisible = true;
                    break;


            }


            //initialized height for image grid row
            imageWidth = App.DisplayScreenWidth / 3;
            imageGridRow.Height = imageWidth;

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

        #region AILS Haulage Arrived
        //refresh the job content when haulage job arrived button press 
        async void RefreshJobContent()
        {
            App.Database.deleteRecords("JobList");
            App.Database.deleteRecordSummary("JobList");
            App.Database.deleteRecordDetails();

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getHaulageJobListURL(), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (response.IsGood)
            {
                var HaulageJobList = JObject.Parse(content)["Result"].ToObject<List<clsHaulageModel>>();

                foreach (clsHaulageModel job in HaulageJobList)
                {
                    ListItems record = new ListItems
                    {
                        Id = job.Id,
                        Background = job.BackColor,
                        Category = "JobList",
                        TruckId = job.TruckId,
                        ReqSign = job.ReqSign,
                        Latitude = job.Latitude,
                        Longitude = job.Longitude,
                        TelNo = job.TelNo,
                        EventRecordId = job.EventRecordId,
                        TrailerId = job.TrailerId,
                        ContainerNo = job.ContainerNo,
                        MaxGrossWeight = job.MaxGrossWeight,
                        TareWeight = job.TareWeight,
                        CollectSeal = job.CollectSeal,
                        SealNo = job.SealNo,
                        ActionId = job.ActionId.ToString(),
                        ActionMessage = job.ActionMessage,
                        Title = job.Title,
                        SealMode = job.SealMode
                    };
                    App.Database.SaveItemAsync(record);

                    foreach (clsCaptionValue summaryList in job.Summary)
                    {
                        SummaryItems summaryItem = new SummaryItems();

                        summaryItem.Id = job.Id;
                        summaryItem.Caption = summaryList.Caption;
                        summaryItem.Value = summaryList.Value;
                        summaryItem.Display = summaryList.Display;
                        summaryItem.Type = "JobList";
                        summaryItem.BackColor = job.BackColor;
                        App.Database.SaveSummarysAsync(summaryItem);
                    }

                    foreach (clsCaptionValue detailList in job.Details)
                    {
                        DetailItems detailItem = new DetailItems();

                        detailItem.Id = job.Id;
                        detailItem.Caption = detailList.Caption;
                        detailItem.Value = detailList.Value;
                        detailItem.Display = detailList.Display;
                        App.Database.SaveDetailsAsync(detailItem);
                    }

                    if (job.Id.Equals(Ultis.Settings.SessionCurrentJobId))
                    {
                        Ultis.Settings.Action = job.ActionId.ToString();
                        Ultis.Settings.SessionCurrentJobId = job.Id;
                        jobItem = null;
                        JobContent();
                    }
                }
            }
        }
        #endregion

        void JobContent()
        {
            //get job info from db
            if (jobItem == null)
            {
                jobItem = App.Database.GetJobRecordAsync(Ultis.Settings.SessionCurrentJobId);
                jobDetails = App.Database.GetDetailsAsync(Ultis.Settings.SessionCurrentJobId);
            }

            //reset the page design
            jobDesc.Children.Clear();
            pointInStack.IsVisible = false;
            actionIconStack.IsVisible = true;
            remarkStack.IsVisible = true;
            TrailerDetailStack.IsVisible = true;
            TrailerStack.IsVisible = true;

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
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += async (sender, e) =>
                {

                    await Clipboard.SetTextAsync(label.Text);

                    var toastConfig = new ToastConfig("Text Copied");
                    toastConfig.SetDuration(2000);
                    toastConfig.Position = 0;
                    toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
                    toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(0, 128, 0));
                    UserDialogs.Instance.Toast(toastConfig);
                };
                label.GestureRecognizers.Add(tapGestureRecognizer);


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

                    case "Point1_Chk":
                    case "Point2_Chk":
                        TrailerDetailStack.IsVisible = false;
                        signatureStack.IsVisible = (jobItem.ReqSign) ? true : false;
                        SplitContainerNumber();
                        break;

                    case "Point1_In":
                    case "Point2_In":
                        lblActionMessage.Text = jobItem.ActionMessage;
                        pointInStack.IsVisible = true;
                        actionIconStack.IsVisible = false;
                        remarkStack.IsVisible = false;
                        TrailerDetailStack.IsVisible = false;
                        TrailerStack.IsVisible = false;
                        break;
                }
            }

            sealNoEntry.Text = jobItem.SealNo;
            //indicate the seal no is mandatory, optional or read only
            if (jobItem.ActionId == "Point1_Chk" || jobItem.ActionId == "Point2_Chk")
            {
                Device.BeginInvokeOnMainThread(() =>
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
                            //sealNoEntry.LineColor = Color.LightYellow;
                            sealNoView.ContainerBackgroundColor = Color.LightYellow;
                            break;

                        case "H":
                            sealNoView.IsVisible = false;
                            sealNoEntry.IsVisible = false;
                            break;
                    }
                });
            }


            //for fowarding job
            if (jobItem.Done == 1 || jobItem.Done == 2)
            {
                confirm_icon.IsEnabled = false;
                confirm_icon.Source = "confirmDisable.png";
                futile_icon.IsEnabled = false;
                futile_icon.Source = "futileDisable.png";

                if (!(String.IsNullOrEmpty(jobItem.Remark)))
                {
                    remarkTextEditor.Text = jobItem.Remark;
                }

                Task.Run(async () => { await DisplayImage(); });
            }
        }

        void SplitContainerNumber()
        {
            if (!(String.IsNullOrEmpty(jobItem.ContainerNo)))
            {
                try
                {
                    string input = jobItem.ContainerNo;
                    contPrefix.Text = input.Substring(0, 4);
                    contNum.Text = input.Substring(4, 7);

                }
                catch
                {
                    var resultString = Regex.Match(jobItem.ContainerNo, @"\d+").Value;

                    contNum.Text = resultString;

                    //DisplayAlert("Sub string Error", exception.Message, "OK");
                }
            }
        }

        async void button_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as SfButton;
            switch (button.StyleId)
            {
                case "btnArrived":
                    try
                    {
                        clsHaulageModel pointIn = new clsHaulageModel
                        {
                            Id = Ultis.Settings.SessionCurrentJobId,
                            ActionId = (jobItem.ActionId.Equals("Point1_In")) ? clsHaulageModel.HaulageActionEnum.Point1_In : clsHaulageModel.HaulageActionEnum.Point2_In
                        };

                        var content = await CommonFunction.CallWebService(1, pointIn, Ultis.Settings.SessionBaseURI, ControllerUtil.updateHaulageJobURL(), this);
                        clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (json_response.IsGood == true)
                        {
                            RefreshJobContent();
                        }
                        else
                        {
                            await DisplayAlert("Error", json_response.Message, "Okay");
                        }
                    }
                    catch (Exception exception)
                    {
                        await DisplayAlert("Error", exception.Message, "OK");
                    }
                    break;

                case "btnNotArrived":
                    await Navigation.PopAsync();
                    break;
            }
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            //var field = sender as CustomEntry;
            var field = sender as Entry;

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
                    if (TrailerDetailStack.IsVisible)
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
                    if (Ultis.Settings.App == "asolute.Mobile.Forwarding")
                    {
                        var answer = await DisplayAlert("", "Update job as futile ?", "Yes", "No");
                        if (answer.Equals(true))
                        {
                            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getFutileJobURL(jobItem.Id, remarkTextEditor.Text), this);

                            if (content != null)
                            {
                                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (response.IsGood)
                                {
                                    jobItem.Done = 2;

                                    App.Database.SaveItemAsync(jobItem);

                                    await DisplayAlert("Success", "Job updated as futile", "OK");

                                    await Navigation.PopAsync();
                                }
                            }
                        }
                    }
                    else
                    {
                        await Navigation.PushAsync(new FutileTrip_CargoReturn());
                    }
                    break;

                case "confirm_icon":
                    bool done = false;

                    if (signatureStack.IsVisible == true)
                    {
                        Stream signatureImage = await signature.GetImageStreamAsync(SignatureImageFormat.Png, strokeColor: Color.Black, fillColor: Color.White);

                        if (signatureImage != null)
                        {
                            if (signed == false)
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

                    if (Ultis.Settings.App == "asolute.Mobile.Forwarding")
                    {
                        
                        jobItem.Done = 1;
                        jobItem.Remark = remarkTextEditor.Text;
                        App.Database.SaveItemAsync(jobItem);
                        previousPage.gotoCompletedPage = true;
                        await Navigation.PopAsync();

                    }

                    //Haulage
                    else
                    {
                        if (done == true || signatureStack.IsVisible == false)
                        {
                            if (!(String.IsNullOrEmpty(trailerIDEntry.Text)) && !(String.IsNullOrEmpty(contPrefix.Text)) && !(String.IsNullOrEmpty(contNum.Text)))
                            {
                                clsCommonFunc checker = new clsCommonFunc();
                                bool status = checker.CheckContainerDigit(contPrefix.Text + contNum.Text);

                                if (status)
                                {
                                    UpdateJob();
                                }
                                else
                                {
                                    var answer = await DisplayAlert("Error", "Invalid container check digit, continue?", "Yes", "No");
                                    if (answer.Equals(true))
                                    {
                                        UpdateJob();
                                    }

                                }
                            }
                            else
                            {
                                await DisplayAlert("Error", "Please enter all mandatory field.", "OK");
                            }
                        }
                    }
                    break;
            }
        }

        async void UpdateJob()
        {
            loading.IsVisible = true;
            try
            {


                if (!(sealNoView.ContainerBackgroundColor == Color.LightYellow) || (sealNoView.ContainerBackgroundColor == Color.LightYellow && !(String.IsNullOrEmpty(sealNoEntry.Text))))
                {
                    if (!(TrailerDetailStack.IsVisible) || (TrailerDetailStack.IsVisible && (chkYes.Checked || chkNo.Checked)))
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

                        if (content != null)
                        {
                            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                            if (response.IsGood)
                            {
                                Ultis.Settings.RefreshListView = "Yes";

                                if (Ultis.Settings.Language.Equals("English"))
                                {
                                    DependencyService.Get<IAudio>().PlayAudioFile("success.mp3");
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
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Please indicate collect seal.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Please enter seal number.", "OK");
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            loading.IsVisible = false;
        }

        #region get and display image in grid
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
            //image.GestureRecognizers.Add(tapGestureRecognizer);
            int noOfImages = imageGrid.Children.Count();
            int noOfCols = imageGrid.ColumnDefinitions.Count();
            int rowNo = noOfImages / noOfCols;
            int colNo = noOfImages - (rowNo * noOfCols);
            imageGrid.Children.Add(image, colNo, rowNo);
        }

        public async Task DisplayImage()
        {
            images.Clear();
            imageGrid.Children.Clear();
            images = App.Database.GetUplodedRecordImagesAsync(jobItem.EventRecordId.ToString(), "NormalImage");
            foreach (AppImage Image in images)
            {
                IFile actualFile = await PCLStorage.FileSystem.Current.GetFileFromPathAsync(Image.photoThumbnailFileLocation);
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
        #endregion

        #region AILS Haulage print function
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
        #endregion
    }
}
