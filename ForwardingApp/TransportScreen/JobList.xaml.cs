using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net.Http;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class JobList : ContentPage
    {

        ObservableCollection<TruckModel> records = new ObservableCollection<TruckModel>();

        public JobList()
        {
            InitializeComponent();

            Title = "Job List";

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetJobList();
        }

        public async void selectJob(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new TransportScreen.JobDetails(((TruckModel)e.Item)));
        }

        protected void pendingJobRefresh(object sender, EventArgs e)
        {
            GetJobList();
            jobList.IsRefreshing = false;
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
                    
                        var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.addJobURL(result.Text));
                        clsResponse jsonResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (jsonResponse.IsGood)
                        {
                            GetJobList();
                            //Ultis.Settings.UpdatedRecord = "Yes";
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

        protected async void GetJobList()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getTruckListURL());
            clsResponse job_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(job_response.IsGood)
            {
                records.Clear();

                App.Database.DeleteTruckModeDetail();

                List<clsTruckingModel> trucks = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();
        
                foreach (clsTruckingModel truck in trucks)
                {

                    string summary = "";
                    TruckModel record = new TruckModel();

                    record.TruckId = truck.TruckId;
                    record.RecordId = truck.Id;

                    if (!(String.IsNullOrEmpty(truck.BackColor)))
                    {
                        record.BackColor = truck.BackColor;
                    }
                    else
                    {
                        record.BackColor = "#ffffff";
                    }

                    foreach (clsCaptionValue truckSummary in truck.Summary)
                    {
                        if (!(String.IsNullOrEmpty(truckSummary.Caption)))
                        {
                            summary += truckSummary.Caption + "  :  " + truckSummary.Value + "\r\n" + "\r\n";
                        }
                    }

                    foreach(clsCaptionValue truckDetail in truck.Details)
                    {
                        DetailItems detail = new DetailItems();

                        detail.Id = truck.Id;
                        detail.Caption = truckDetail.Caption;
                        detail.Value = truckDetail.Value;
                        detail.Display = truckDetail.Display;
                        App.Database.SaveDetailsAsync(detail);
                    }

                    record.Summary = summary;

                    records.Add(record);

                }
                loadJobList();
            }
            else
            {
                await DisplayAlert("JsonError", job_response.Message, "OK");
            }
        }

        public void loadJobList()
        {
            jobList.ItemsSource = records;
            jobList.HasUnevenRows = true;

            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;

            if (records.Count == 0)
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
    }
}
