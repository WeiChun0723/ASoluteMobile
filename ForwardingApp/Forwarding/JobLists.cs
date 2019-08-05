using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute_Mobile.HaulageScreen;
using ASolute_Mobile.Models;
using Xamarin.Forms;

namespace ASolute_Mobile.Forwarding
{
    public class JobLists : ContentPage
    {
        int doneStatus;
        private bool appearedBefore = false;
        public bool gotoCompletedPage { get; set; }

        public JobLists(int doneStatus)
        {
            NavigationPage.SetHasNavigationBar(this, false);

            this.doneStatus = doneStatus;
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            if (gotoCompletedPage == true)
            {
                gotoCompletedPage = false;
                JobListTabbedPage jobListTabbedPage = (JobListTabbedPage)this.Parent.Parent;
                NavigationPage completedTab = (NavigationPage)(jobListTabbedPage).Children[1];
                jobListTabbedPage.SelectedItem = completedTab;
                jobListTabbedPage.CurrentPage = completedTab;
                completedTab.Focus();
                return;
            }

            if (!appearedBefore)
            {
                appearedBefore = true;
                try
                {
                    if (doneStatus == 0)
                    {
                        await BackgroundTask.DownloadLatestJobs(this);
                    }
                }
                catch (HttpRequestException exception)
                {
                    await DisplayAlert("Offline Mode", "Cant connect to server", "Ok");
                }
            }

            var activity = new ActivityIndicator
            {
                IsEnabled = true,
                IsVisible = true,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                IsRunning = true
            };

            var layout = new StackLayout();

            ObservableCollection<ListItems> jobItems = new ObservableCollection<ListItems>(App.Database.GetJobs(doneStatus));

            Ultis.Settings.List = "fwd";

            ListView listView = new ListView()
            {
                ItemsSource = jobItems,
                HasUnevenRows = true,
                Style = (Style)Application.Current.Resources["recordListStyle"],
                ItemTemplate = new DataTemplate(typeof(CustomListViewCell))
            };

            layout.Children.Add(listView);

            listView.ItemTapped += (sender, e) =>
            {
                Ultis.Settings.SessionCurrentJobId = ((ListItems)e.Item).Id;
                NewJobDetails jobDetail = new NewJobDetails();
                jobDetail.previousPage = this;
                Navigation.PushAsync(jobDetail);
            };

            listView.IsPullToRefreshEnabled = true;

            listView.Refreshing += (sender, e) =>
            {
                if (doneStatus == 0 || doneStatus== 1)
                {
                    Task.Run(async () => {
                        await BackgroundTask.DownloadLatestJobs(this);
                        await BackgroundTask.UploadLatestJobs();
                    }).Wait();
                }

                ListView list = (ListView)sender;
                list.ItemsSource = App.Database.GetJobs( doneStatus);
                list.IsRefreshing = false;
            };

            activity.IsRunning = false;

            Content = layout;
        }

    }
}

