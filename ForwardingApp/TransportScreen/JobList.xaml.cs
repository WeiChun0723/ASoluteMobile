using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class JobList : ContentPage
    {

        ObservableCollection<TruckModel> records = new ObservableCollection<TruckModel>();

        public JobList()
        {
            InitializeComponent();

            Title = "Job List";

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
        }
    }
}
