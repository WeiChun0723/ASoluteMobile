using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        List<ListItems> records = new List<ListItems>();
        string webServiceAction;

        public JobList(string action, string name)
        {
            InitializeComponent();

            Title = name;

            webServiceAction = action;

            GetJobList();

            if (action.Contains("MasterJobList"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if(Ultis.Settings.RefreshListView == "Yes")
            {
                GetJobList();
                Ultis.Settings.RefreshListView = "No";
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "RefreshNewMasterJobList", (sender) => {
                GetJobList();
            });
        }

        public async void selectJob(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new TransportScreen.JobDetails(((ListItems)e.Item)));
        }

        protected void pendingJobRefresh(object sender, EventArgs e)
        {
            loading.IsRunning = true;
            loading.IsVisible = true;
            loading.IsEnabled = true;
            GetJobList();
            jobList.IsRefreshing = false;
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                scanPage.AutoFocus();
                await Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {

                        await Navigation.PopAsync();

                        searchBar.Text = result.Text;

                    });
                };
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void SearchJob(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                    loadJobList();
                }

                else
                {
                    try
                    {
                        jobList.ItemsSource = records.Where(x => x.Summary.Contains(searchKey));

                    }
                    catch
                    {
                       // await DisplayAlert("Error", "Please try again", "OK");
                    }
                }
            }
            catch
            {
               // await DisplayAlert("Error", "Please try again", "OK");
            }

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
                    
                        var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.addJobURL(result.Text),this);
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

            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getTruckListURL(webServiceAction),this);
            clsResponse job_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(job_response.IsGood)
            {
                records.Clear();

                App.Database.deleteRecordDetails();

                List<clsTruckingModel> trucks = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();
        
                foreach (clsTruckingModel truck in trucks)
                {

                    string summary = "";
                    ListItems record = new ListItems
                    {
                        TruckId = truck.TruckId,
                        Id = truck.Id,
                        Action = truck.Action,
                        EventRecordId = truck.EventRecordId,
                        Latitude = truck.Latitude,
                        Longitude = truck.Longitude,
                        TelNo = truck.TelNo,
                        ReqSign = truck.ReqSign
                    };

                    if (!(String.IsNullOrEmpty(truck.BackColor)))
                    {
                        record.Background = truck.BackColor;
                    }
                    else
                    {
                        record.Background = "#ffffff";
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
                searchBar.IsVisible = false;
                barcode_icon.IsVisible = false;
            }
            else
            {
                jobList.IsVisible = true;
                noData.IsVisible = false;
            }
        }
    }
}
