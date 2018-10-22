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
    public partial class PendingReceiving_Loading : ContentPage
    {

        string title = "";
        ObservableCollection<TruckModel> records = new ObservableCollection<TruckModel>();

        public PendingReceiving_Loading(string pageTitle)
        {
            InitializeComponent();

            title = pageTitle;

            if(pageTitle.Equals("CargoRec"))
            {
                Title = "Cargo Receiving";
            }
            else if(pageTitle.Equals("CargoLoad"))
            {
                Title = "Cargo Loading";
            }
            else if(pageTitle.Equals("CargoMeasure"))
            {
                Title = "Cargo Measuring";
            }


        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetPendingList();
        }

        public async void selectPending(object sender, ItemTappedEventArgs e)
        {
         
            if(Title.Equals("Cargo Measuring"))
            {
                await Navigation.PushAsync(new SubJobList(((TruckModel)e.Item).RecordId));
            }
            else
            {
                await Navigation.PushAsync(new CargoEntry(((TruckModel)e.Item).TruckId, ((TruckModel)e.Item).RecordId, title));
            }

        }


        protected void pendingListRefresh(object sender, EventArgs e)
        {
             GetPendingList();
             pendingList.IsRefreshing = false;

        }


        protected async void GetPendingList()
        {
            records.Clear();
            string url = "";

            if (Title.Equals("Cargo Receiving"))
            {
               url = ControllerUtil.getPendingReceivingURL();
            }
            else if (Title.Equals("Cargo Loading"))
            {
                url = ControllerUtil.getPendingLoadURL();
            }
            else if (Title.Equals("Cargo Measuring"))
            {
                url = ControllerUtil.getPendingReceivingURL();
            }

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, url);
            clsResponse pending_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(pending_response.IsGood)
            {
                App.Database.DeleteTruckModel();

                List<clsTruckingModel> trucks = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();
               
                foreach (clsTruckingModel truck in trucks)
                {
                    string summary = "";
                    TruckModel record = new TruckModel();

                    record.TruckId = truck.TruckId;
                    record.RecordId = truck.Id;

                    foreach(clsCaptionValue truckValue in truck.Summary)
                    {
                       
                        summary += truckValue.Caption + "  :  " + truckValue.Value + "\r\n" + "\r\n";

                        if(truckValue.Caption.Equals("Job No."))
                        {
                            record.JobNo = truckValue.Value;
                        }
                    }

                    record.Summary = summary;

                    //App.Database.SaveTruckModelAsync(record);
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
            pendingList.ItemsSource = records;
            pendingList.HasUnevenRows = true;

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
