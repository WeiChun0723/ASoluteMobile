using ASolute_Mobile.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public class JobLists :ContentPage
    {
        string sessionKey = Ultis.Settings.SessionSettingKey;
        int doneStatus = 0;
        private bool appearedBefore = false;
        public bool gotoCompletedPage { get; set; }
        public MainMenu previousPage;
        public string uri = "";
        ObservableCollection<JobItems> jobs,receiving,loading;

        public JobLists()
        {
            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Job Lists";
            }
            else
            {
                Title = "Senarai kerja";
            }
            
            Ultis.Settings.App = "Haulage";
        }

        protected override async void OnAppearing()
        {
            if (Ultis.Settings.UpdatedRecord.Equals("Yes"))
            {
                await BackgroundTask.DownloadLatestRecord(this);
                Ultis.Settings.UpdatedRecord = "No";
            }
            
            var activity = new ActivityIndicator
            {
                IsEnabled = true,
                IsVisible = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsRunning = true
            };          

            var layout = new StackLayout();

            ListView listView = new ListView()
            {

                HasUnevenRows = true,
                Style = (Style)App.Current.Resources["recordListStyle"],
                SeparatorColor = Color.White
                
            };

            Image noData = new Image()
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = "nodatafound.png",
                IsVisible = false
            };

            Image scanBC = new Image()
            {
                HorizontalOptions = LayoutOptions.EndAndExpand,
                Source = "barCode.png",
                IsVisible = true,                
            };

            var scanCode = new TapGestureRecognizer();
            scanCode.Tapped += async (sender, e) =>
            {

            };
            scanBC.GestureRecognizers.Add(scanCode);


            layout.Children.Add(listView);
            layout.Children.Add(noData);
            layout.Children.Add(activity);

            if (Ultis.Settings.MenuAction.Equals("Job_List"))
            {
               
                jobs = new ObservableCollection<JobItems>(App.Database.GetJobItems(doneStatus, "HaulageJob"));
                

                listView.ItemsSource = jobs;
                Ultis.Settings.ListType = "Job_List";               
               
            }           

            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));


            if (jobs.Count == 0)
            {
                listView.IsVisible = false;
                noData.IsVisible = true;
            }
            else
            {
                listView.IsVisible = true;
                noData.IsVisible = false;

            }

            listView.ItemTapped += (sender, e) =>
            {
                Ultis.Settings.SessionCurrentJobId = ((JobItems)e.Item).Id;
                JobDetails jobDetail = new JobDetails(((JobItems)e.Item).ActionId, ((JobItems)e.Item).ActionMessage);
                Ultis.Settings.ActionID = ((JobItems)e.Item).ActionId;
                //jobDetail.previousPage = this;
                Navigation.PushAsync(jobDetail);
            };

            listView.IsPullToRefreshEnabled = true;

            listView.Refreshing += (sender, e) =>
            {
                if (doneStatus == 0)
                {
                    Task.Run(async () => {
                        await BackgroundTask.DownloadLatestRecord(this);
                        await BackgroundTask.UploadLatestRecord();
                    }).Wait();
                }
                ListView list = (ListView)sender;
               
                if(Ultis.Settings.App.Equals("Haulage"))
                {
                    list.ItemsSource = App.Database.GetJobItems(doneStatus, "HaulageJob"); 
                }
                list.IsRefreshing = false;
            };

            activity.IsRunning = false;
            activity.IsVisible = false;

            Content = layout;
        }

       
    }
}
