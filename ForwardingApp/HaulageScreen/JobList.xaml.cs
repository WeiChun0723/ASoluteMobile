using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.HaulageScreen
{
	public partial class JobList : ContentPage
	{
        int doneStatus = 0;

        public JobList (string title)
		{
			InitializeComponent ();

            Ultis.Settings.App = "Haulage";

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
        }

        protected async override void OnAppearing()
        {
            try
            {
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

                if (Ultis.Settings.SessionUserItem.TruckId.Length == 0)
                {
                    scan_icon.IsEnabled = false;
                }

                if (NetworkCheck.IsInternet() && Ultis.Settings.UpdatedRecord.Equals("RefreshJobList"))
                {
                    Ultis.Settings.UpdatedRecord = "No";
                    GetJobList();
                }
                else
                {
                    loadJobList();
                }
            }
            catch
            {
                await DisplayAlert("Error","Please refresh menu and try again", "OK");
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        protected void GetJobList()
        {
            Task.Run(async () => { await  BackgroundTask.DownloadLatestRecord(this);}).Wait();

            loadJobList();
        }

        public async void selectJob(object sender, ItemTappedEventArgs e)
        {
            Ultis.Settings.SessionCurrentJobId = ((JobItems)e.Item).Id;
            JobDetails jobDetail = new JobDetails(((JobItems)e.Item).ActionId, ((JobItems)e.Item).ActionMessage);
            Ultis.Settings.Action = ((JobItems)e.Item).ActionId;
            jobDetail.previousPage = this;
            await Navigation.PushAsync(jobDetail);
        }

        public async void BarCodeScan(object sender, EventArgs e)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                await Navigation.PushAsync(scanPage);
                scanPage.OnScanResult += (result) =>
                {                    
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        scanPage.PauseAnalysis();                       
                        var client = new HttpClient();
                        client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                        var uri = ControllerUtil.addJobURL(result.Text);
                        var response = await client.GetAsync(uri);
                        var content = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine(content);
                        clsResponse jsonResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (jsonResponse.IsGood)
                        {
                            Ultis.Settings.RefreshMenuItem = "Yes";
                            Ultis.Settings.UpdatedRecord = "RefreshJobList";
                            GetJobList();
                            displayToast("Job added to job list.");                           
                            scanPage.ResumeAnalysis();
                        }
                        else
                        {
                            var answer = await DisplayAlert("Error", jsonResponse.Message, "OK", "Cancel");
                            if (answer.Equals(true))
                            {                               
                                scanPage.ResumeAnalysis();
                            }
                            else
                            {
                                await Navigation.PopAsync();
                            }
                        }
                    });
                };
            }
            catch
            {
                await DisplayAlert("Error", "Please try again later", "OK");
            }
               
        }

        public void jobListRefresh(object sender, EventArgs e)
        {       
            try
            {
                Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(this); }).Wait();
                loadJobList();
                jobList.IsRefreshing = false;
            }
            catch
            {
                BackgroundTask.Logout(this);
               DisplayAlert("Error", "Invalid session.", "OK");
            }

        }

        public void loadJobList()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                jobList.RowHeight = 200;
            }

            Ultis.Settings.List = "Job_List";
            ObservableCollection<JobItems> jobs = new ObservableCollection<JobItems>(App.Database.GetJobItems(doneStatus, "HaulageJob"));
            jobList.ItemsSource = jobs;
            jobList.HasUnevenRows = true;
            jobList.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (jobs.Count == 0)
            {
                jobList.IsVisible = true;
                noData.IsVisible = true;
            }
            else
            {
                jobList.IsVisible = true;
                noData.IsVisible = false;
            }

        }

        public void displayToast(string message)
        {
            var toastConfig = new ToastConfig(message);
            toastConfig.SetDuration(2000);
            toastConfig.Position = 0;
            toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
            if (message == "Job added to job list.")
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(50, 205, 50));
            }
           
            UserDialogs.Instance.Toast(toastConfig);
        }

    }
}