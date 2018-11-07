using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class JobLists : ListViewCommonScreen
    {
        ObservableCollection<TruckModel> records = new ObservableCollection<TruckModel>();

        public JobLists()
        {
            Title = "Job List";

            listView.ItemTapped += async (sender, e) =>
            {
                await Navigation.PushAsync(new TransportScreen.JobDetails(((TruckModel)e.Item)));
            };

            listView.Refreshing += (sender, e) =>
            {
                GetJobList();
                listView.IsRefreshing = false;
            };

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetJobList();
        }

        protected async void GetJobList()
        {
       

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getTruckListURL());
            clsResponse job_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (job_response.IsGood)
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

                    foreach (clsCaptionValue truckDetail in truck.Details)
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
            listView.ItemsSource = records;
            listView.HasUnevenRows = true;

            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;

            if (records.Count == 0)
            {
                listView.IsVisible = true;
                image.IsVisible = true;
            }
            else
            {
                listView.IsVisible = true;
                image.IsVisible = false;
            }
        }
    }
}

