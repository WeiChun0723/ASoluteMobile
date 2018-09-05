using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.HaulageScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class JobList : ContentPage
	{
        int doneStatus = 0;

        public JobList ()
		{
			InitializeComponent ();

            Ultis.Settings.App = "Haulage";           

        }

        protected async override void OnAppearing()
        {
            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Job Lists";
            }
            else
            {
                Title = "Senarai Kerja";
            }

            string test = Ultis.Settings.UpdatedRecord;

            if (Ultis.Settings.SessionUserItem.TrailerPrefix.Length == 0)
            {
                scan_icon.IsEnabled = false;
            }

            if (NetworkCheck.IsInternet())
            {
                if(Ultis.Settings.UpdatedRecord.Equals("Yes"))
                {
                    CallWebService(true);
                }
                else
                {
                    CallWebService(true);

                }
            }           
            else
            {
                loadJobList();
            }
        }

        protected void CallWebService(bool wait)
        {
            if (wait == true)
            {
                Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(this); }).Wait();
            }
            else if (wait == false)
            {
                Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(this);});

            }            
            loadJobList();
        }

        public async void selectJob(object sender, ItemTappedEventArgs e)
        {
            Ultis.Settings.SessionCurrentJobId = ((JobItems)e.Item).Id;
            JobDetails jobDetail = new JobDetails(((JobItems)e.Item).ActionId, ((JobItems)e.Item).ActionMessage);
            Ultis.Settings.ActionID = ((JobItems)e.Item).ActionId;
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
                            CallWebService(true);
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
            catch(AggregateException test)
            {
                await DisplayAlert("Error", "Please try again later", "OK");
            }
               
        }

        public void jobListRefresh(object sender, EventArgs e)
        {            
            Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(this); }).Wait();
            loadJobList();
            jobList.IsRefreshing = false;
        }

        public void loadJobList()
        {
            Ultis.Settings.ListType = "Job_List";
            ObservableCollection<JobItems> jobs = new ObservableCollection<JobItems>(App.Database.GetJobItems(doneStatus, "HaulageJob"));
            jobList.ItemsSource = jobs;
            jobList.HasUnevenRows = true;
            jobList.Style = (Style)App.Current.Resources["recordListStyle"];
            jobList.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (jobs.Count == 0)
            {
                jobList.IsVisible = false;
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