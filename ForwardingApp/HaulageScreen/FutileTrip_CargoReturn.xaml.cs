using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
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
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class FutileTrip_CargoReturn : ContentPage
	{

        //public JobDetails jobPreviousPage;
        public string uploadUri = "";
        List<AppImage> images = new List<AppImage>();
        double imageWidth;      
        string job_id;
        int reason_choice;
        JobItems jobItem;
        List<pickerValue> reasonCode;
        static byte[] scaledImageByte;
        static int uploadedImage = 0;

        public FutileTrip_CargoReturn ()
		{
			InitializeComponent ();
            pageContent();

            imageGrid.RowSpacing = 0;
            imageGrid.ColumnSpacing = 0;
            imageWidth = App.DisplayScreenWidth / 3;
            imageGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(imageWidth) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            imageGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
          
            job_id = Ultis.Settings.SessionCurrentJobId; 
                      
            reasonCode = new List<pickerValue>(App.Database.GetPickerValue("ReasonCode"));
            
            for (int j = 0; j < reasonCode.Count; j++)
            {
                ReasonPicker.Items.Add(reasonCode[j].Value);
            }

            jobItem = App.Database.GetItemAsync(job_id);

            images.Clear();
            imageGrid.Children.Clear();           
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

            Title = "Futile trip";

            if (Ultis.Settings.DeleteImage == "Yes")
            {
                displayImage();
                Ultis.Settings.DeleteImage = "No";
            }
        }

      

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        public async void reasonSelected(object sender, SelectedPositionChangedEventArgs e)
        {
            if (ReasonPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Error", "Please choose station", "Ok");
            }
            else
            {
                reason_choice = ReasonPicker.SelectedIndex;
            }
        }


        public async void takeImage(object sender, EventArgs e)
        {
            await CommonFunction.StoreImages(jobItem.EventRecordId.ToString(), this);

            displayImage();
            BackgroundTask.StartTimer();
        }    

        public async void uploadDetail(object sender, EventArgs e)
        {
            try
            {
                Ultis.Settings.RefreshMenuItem = "Yes";
                Ultis.Settings.UpdatedRecord = "RefreshJobList";
                if (ReasonPicker.SelectedIndex != -1 )
                {
                    try
                    {                      
                       
                        clsHaulageModel futileJob = new clsHaulageModel();
                        futileJob.Id = Ultis.Settings.SessionCurrentJobId;
                        futileJob.ActionId = clsHaulageModel.HaulageActionEnum.FutileTrip;
                        if (remarkTextEditor.Text == null)
                        {
                            futileJob.Remarks = "";
                        }
                        else
                        {
                            futileJob.Remarks = remarkTextEditor.Text;
                        }
                           
                        futileJob.ReasonCode = reasonCode[reason_choice].Value;

                        var client = new HttpClient();
                        client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                        var uri = ControllerUtil.postHaulageURL();
                        var content = JsonConvert.SerializeObject(futileJob);
                        var httpContent = new StringContent(content, Encoding.UTF8, "application/json");
                        var response = await client.PostAsync(uri, httpContent);
                        var reply = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(reply);
                        clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(reply);

                        if (json_response.IsGood == true)
                        {
                            confirm_icon.IsEnabled = false;
                            confirm_icon.Source = "confirmDisable.png";
                            await DisplayAlert("Success", "Job status uploaded", "OK");
                            Navigation.RemovePage(this.Navigation.NavigationStack[this.Navigation.NavigationStack.Count - 2]);

                           
                            //await Navigation.PopAsync();

                        }
                        else
                        {
                            await DisplayAlert("Error", json_response.Message, "OK");
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
    

        public void pageContent()
        {
            if (Ultis.Settings.MenuRequireAction == "Cargo_Return" )
            {
                Title = "Cargo Return";
                uploadUri = ControllerUtil.postCargoReturnURL();
                ReferenceStack.IsVisible = true;
                              
            }
            else if (Ultis.Settings.MenuRequireAction == "FutileTrip")
            {
                Title = "Futile Trip";                                    
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

        public async void displayImage()
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

        public async void UploadImage(string uploadID)
        {
            List<AppImage> recordImages = App.Database.GetRecordImagesAsync(uploadID, false);
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

                var image_client = new HttpClient();
                image_client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                var image_uri = ControllerUtil.UploadImageURL(jobItem.EventRecordId.ToString());
                var imagecontent = JsonConvert.SerializeObject(image);
                var image_httpContent = new StringContent(imagecontent, Encoding.UTF8, "application/json");

                HttpResponseMessage image_response = await image_client.PostAsync(image_uri, image_httpContent);
                var Imagereply = await image_response.Content.ReadAsStringAsync();
                Debug.WriteLine(Imagereply);
                clsResponse Imageresult = JsonConvert.DeserializeObject<clsResponse>(Imagereply);

                if (Imageresult.IsGood == true)
                {
                    uploadedImage++;
                    recordImage.Uploaded = true;
                    App.Database.SaveRecordImageAsync(recordImage);
                }
            }

            if (uploadedImage == recordImages.Count)
            {
                await DisplayAlert("Success", "Image uploded", "OK");
            }

        }
    }
}