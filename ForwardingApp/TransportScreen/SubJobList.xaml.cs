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
    public partial class SubJobList : ContentPage
    {
        string jobID;
        ObservableCollection<ListItems> records = new ObservableCollection<ListItems>();
        
        public SubJobList(String JobNo)
        {
            InitializeComponent();
            jobID = JobNo;
            Title = "Sub Job List";
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetSubJobList();
        }

        protected void subJobListRefresh(object sender, EventArgs e)
        {
            GetSubJobList();
            subJobList.IsRefreshing = false;
        }

        public async void selectSubJob(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new TransportScreen.CargoItem(((ListItems)e.Item).Id));
        }

        protected async void GetSubJobList()
        {
            records.Clear();
    
            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getSubJobURL(jobID),this);
            clsResponse pending_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (pending_response.IsGood)
            {

                List<clsTruckingModel> trucks = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();

                foreach (clsTruckingModel truck in trucks)
                {
                    string summary = "";
                    ListItems record = new ListItems();

                    record.TruckId = truck.TruckId;
                    record.Id = truck.Id;

                    foreach (clsCaptionValue truckValue in truck.Summary)
                    {
                        if(!(String.IsNullOrEmpty(truckValue.Caption)))
                        {
                            summary += truckValue.Caption + "  :  " + truckValue.Value + "\r\n" + "\r\n";
                        }
                        else
                        {
                            summary += truckValue.Value + "\r\n" + "\r\n";
                        }


                        if (truckValue.Caption.Equals("Job No."))
                        {
                            record.JobNo = truckValue.Value;
                        }
                    }

                    record.Summary = summary;
                    records.Add(record);
                }

                loadPendingList();
            }
            else
            {
                await DisplayAlert("JsonError", pending_response.Message, "OK");
            }

        }

        public void loadPendingList()
        {
            subJobList.ItemsSource = records;
            subJobList.HasUnevenRows = true;

            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;

            if (records.Count == 0)
            {
                noData.IsVisible = true;
            }
            else
            {
                noData.IsVisible = false;
            }
        }
    }
}
