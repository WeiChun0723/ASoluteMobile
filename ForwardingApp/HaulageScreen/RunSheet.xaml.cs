using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

namespace ASolute_Mobile.HaulageScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RunSheet : ContentPage
	{
        string selectDate,option = "none";

		public RunSheet ()
		{
			InitializeComponent ();

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Run Sheet";
            }
            else
            {
                Title = "Borang Perjalanan";
            }
            Ultis.Settings.App = "Haulage";
        }

        protected override void OnAppearing()
        {

            downloadRunSheet(datePicker.Date.ToString("yyyy MMMMM dd"));    
        }

        public void recordDate(object sender, DateChangedEventArgs e)
        {
            if(option == "none")
            {
                selectDate = e.NewDate.ToString("yyyy MMMMM dd");


                downloadRunSheet(selectDate);
            }

        }

        public void PreviousDate(object sender, EventArgs e)
        {

           
            previous_icon.IsEnabled = false;
            next_icon.IsEnabled = false;
            option = "previous";
            downloadRunSheet(datePicker.Date.AddDays(-1).ToString("yyyy MMMMM dd"));

        }

        public void NextDate(object sender, EventArgs e)
        {
           
            previous_icon.IsEnabled = false;
            next_icon.IsEnabled = false;
            option = "next";
            downloadRunSheet(datePicker.Date.AddDays(1).ToString("yyyy MMMMM dd"));
        }


        protected void runSheetRefresh(object sender, EventArgs e)
        {
            downloadRunSheet(datePicker.Date.ToString("yyyy MMMMM dd"));

            runSheetHistory.IsRefreshing = false;
        }

        public async void downloadRunSheet(string date)
        {
            try
            {
                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;
                runSheetHistory.IsVisible = false;
                noData.IsVisible = false;

                var client = new HttpClient();
                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                var uri = ControllerUtil.getDownloadHaulageHistoryURL(date);
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);

                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood == true)
                {
                    switch (option)
                    {
                        case "previous":
                            datePicker.Date = datePicker.Date.AddDays(-1);
                            break;
                        case "next":
                            datePicker.Date = datePicker.Date.AddDays(1);
                            break;
                        case "none":
                            break;
                    }


                    var JobList = JObject.Parse(content)["Result"].ToObject<List<clsHaulageModel>>();

                    App.Database.deleteHaulage("HaulageHistory");
                    foreach (clsHaulageModel job in JobList)
                    {
                        JobItems existingRecord = App.Database.GetPendingRecordAsync(job.Id);

                        existingRecord = new JobItems();
                        existingRecord.TruckId = job.TruckId;
                        existingRecord.ReqSign = job.ReqSign;
                        existingRecord.Id = job.Id;
                        existingRecord.Done = 0;
                        existingRecord.JobType = "HaulageHistory";
                        existingRecord.Latitude = job.Latitude;
                        existingRecord.Longitude = job.Longitude;
                        existingRecord.telNo = job.TelNo;
                        existingRecord.EventRecordId = job.EventRecordId;
                        existingRecord.TrailerId = job.TrailerId;
                        existingRecord.ContainerNo = job.ContainerNo;
                        existingRecord.MaxGrossWeight = job.MaxGrossWeight;
                        existingRecord.TareWeight = job.TareWeight;
                        existingRecord.CollectSeal = job.CollectSeal;
                        existingRecord.SealNo = job.SealNo;
                        existingRecord.ActionId = job.ActionId.ToString();
                        existingRecord.ActionMessage = job.ActionMessage;
                        App.Database.SaveJobsAsync(existingRecord);

                        List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(job.Id, "HaulageHistory");

                        int index = 0;
                        foreach (clsCaptionValue summaryList in job.Summary)
                        {
                            SummaryItems summaryItem = null;
                            if (index < existingSummaryItems.Capacity)
                            {
                                summaryItem = existingSummaryItems.ElementAt(index);
                            }

                            if (summaryItem == null)
                            {
                                summaryItem = new SummaryItems();
                            }

                            summaryItem.Id = job.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = "HaulageHistory";
                            App.Database.SaveSummarysAsync(summaryItem);
                            index++;
                        }

                        if (existingSummaryItems != null)
                        {
                            for (; index < existingSummaryItems.Count; index++)
                            {
                                App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                            }
                        }

                    }


                    option = "none";
                    refreshRunSheetHistory();
                }
                else
                {
                    if (json_response.Message == "Invalid Session !")
                    {
                        BackgroundTask.Logout(this);
                        await DisplayAlert("Error", json_response.Message, "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Error", json_response.Message, "Ok");
                    }

                }
            }
            catch(Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "OK");
            }
            
        }

        public void refreshRunSheetHistory()
        {
            
            Ultis.Settings.ListType = "Run_Sheet";
            ObservableCollection<JobItems> Item = new ObservableCollection<JobItems>(App.Database.GetJobItems(0, "HaulageHistory"));
            runSheetHistory.ItemsSource = Item;
            runSheetHistory.HasUnevenRows = true;
            runSheetHistory.Style = (Style)App.Current.Resources["recordListStyle"];
            runSheetHistory.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if(Item.Count == 0)
            {
                runSheetHistory.IsVisible = false;
                noData.IsVisible = true;
                
            }
            else
            {
                runSheetHistory.IsVisible = true;
                noData.IsVisible = false;
            }

            this.activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
            previous_icon.IsEnabled = true;
            next_icon.IsEnabled = true;
        }
    }
}