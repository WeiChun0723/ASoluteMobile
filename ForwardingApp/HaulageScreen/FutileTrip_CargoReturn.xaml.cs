using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PCLStorage;
using Plugin.Media;
using SignaturePad.Forms;
using System;
using System.Collections.Generic;
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
	
	public partial class FutileTrip_CargoReturn : ContentPage
	{
        public string uploadUri = "";
        List<AppImage> images = new List<AppImage>();
        double imageWidth;
        //JobItems jobItem;
        ListItems jobItem;
        List<clsKeyValue> reasonCode;

        public FutileTrip_CargoReturn ()
		{
			InitializeComponent ();
            Title = "Futile trip";
            pageContent();

            //initialized height for image grid row
            imageWidth = App.DisplayScreenWidth / 3;
            imageGridRow.Height = imageWidth;

            //get picker value
            GetPickerValue();

            //jobItem = App.Database.GetItemAsync(Ultis.Settings.SessionCurrentJobId);
            jobItem = App.Database.GetJobRecordAsync(Ultis.Settings.SessionCurrentJobId);
        
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var icon = sender as Image;

            switch(icon.StyleId)
            {
                case "camera_icon":
                    await CommonFunction.StoreImages(jobItem.EventRecordId.ToString(), this, "NormalImage");
                    DisplayImage();
                    BackgroundTask.StartTimer();
                    break;

                case "confirm_icon":
                    UploadDetail();
                    break;
            }
        }

        async void GetPickerValue()
        {
            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getReasonListURL(),this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response.IsGood)
            {
                reasonCode = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                List<string> value = new List<string>();

                foreach(clsKeyValue reason in reasonCode)
                {
                    value.Add(reason.Value);
                }

                reasonPicker.ComboBoxSource = value;
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        public async void UploadDetail()
        {
            try
            {
                Ultis.Settings.RefreshMenuItem = "Yes";
                Ultis.Settings.UpdatedRecord = "RefreshJobList";
                if (!(String.IsNullOrEmpty(reasonPicker.Text)))
                {
                    try
                    {                      
                       
                        clsHaulageModel futileJob = new clsHaulageModel();
                        futileJob.Id = Ultis.Settings.SessionCurrentJobId;
                        futileJob.ActionId = clsHaulageModel.HaulageActionEnum.FutileTrip;
                        futileJob.Remarks = (remarkTextEditor.Text == null) ? "" : remarkTextEditor.Text;
                        futileJob.ReasonCode = reasonCode[reasonPicker.SelectedIndex].Key;

                        var content = await CommonFunction.CallWebService(1, futileJob, Ultis.Settings.SessionBaseURI, ControllerUtil.updateHaulageJobURL(), this);
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (response.IsGood == true)
                        {
                            confirm_icon.IsEnabled = false;
                            confirm_icon.Source = "confirmDisable.png";
                            await DisplayAlert("Success", "Job status uploaded", "OK");
                            Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);
                        }
                        else
                        {
                            await DisplayAlert("Error", response.Message, "OK");
                        }
                                                  
                    }                
                    catch (Exception exception)
                    {
                        await DisplayAlert("Error", exception.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "Please fill all mandatory field", "OK");
                }
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch(Exception)
            {
                await DisplayAlert("Reminder", "Please sign for the job.", "OK");
            }

        }
    
        //indicate for cargo return or futile trip, but cargo return seem no use , this code can be remove
        public void pageContent()
        {
            if (Ultis.Settings.MenuRequireAction == "Cargo_Return" )
            {
                Title = "Cargo Return";
                uploadUri = ControllerUtil.postCargoReturnURL();
                ReferenceStack.IsVisible = true;
                              
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
            //image.GestureRecognizers.Add(tapGestureRecognizer);
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
            images = App.Database.GetUplodedRecordImagesAsync(jobItem.EventRecordId.ToString(),"NormalImage");
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
    }
}