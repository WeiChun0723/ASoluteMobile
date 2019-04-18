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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.HaulageScreen
{

    public partial class RunSheet : CarouselPage
    {
        bool firstLoad = true, changePage = false;
        string selectDate, option = "none";
        DatePicker datePicker;
        ListView runSheetHistory;
        Image previous_icon, next_icon, noData;
        ActivityIndicator activityIndicator;
        DateTime currentDate;
        ListItems runSheetItem = new ListItems();

        public RunSheet(ListItems item)
        {
            InitializeComponent();

            runSheetItem = item;

            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text = item.Name,
                TextColor = Color.White
            };

            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

            PageContent();

            downloadRunSheet(DateTime.Now.ToString("yyyy MM dd"));

            SelectedItem = Children[1];


        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            int index = Children.IndexOf(CurrentPage);


            if (!firstLoad)
            {

                if (index == 0)
                {

                    currentDate = datePicker.Date.AddDays(-1);
                    changePage = true;
                    downloadRunSheet(currentDate.ToString("yyyy MM dd"));

                }
                else if (index == 2)
                {

                    currentDate = datePicker.Date.AddDays(1);
                    changePage = true;
                    downloadRunSheet(currentDate.ToString("yyyy MM dd"));

                }
                else
                {
                    PageContent();
                }

            }

        }

        protected override void OnAppearing()
        {
            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(CurrentPage);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) =>
            {

                try
                {

                    CommonFunction.NewJobNotification(CurrentPage);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        public void recordDate(object sender, DateChangedEventArgs e)
        {
            if (option == "none")
            {
                selectDate = e.NewDate.ToString("yyyy MM dd");

                downloadRunSheet(selectDate);
            }

        }

        public void PageContent()
        {
            StackLayout mainLayout = new StackLayout
            {
                Padding = new Thickness(10, 10, 10, 10)
            };

            mainLayout.Children.Clear();

            noData = new Image
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                Source = "nodatafound.png",
                IsVisible = false
            };


            activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
                IsVisible = true
            };

            StackLayout optionStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 15,
                Padding = new Thickness(15, 15, 15, 15)
            };

            Label label = new Label
            {
                Text = "Date",
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 20
            };

            datePicker = new DatePicker
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                BackgroundColor = Color.LightYellow,
                Scale = 1.0
            };

            datePicker.DateSelected += recordDate;

            StackLayout optionImageStack = new StackLayout
            {
                Orientation = StackOrientation.Horizontal,
                Spacing = 5,

            };

            previous_icon = new Image
            {
                Source = "angleLeft.png",
                WidthRequest = 40,
                HeightRequest = 40
            };

            var previous = new TapGestureRecognizer();
            previous.Tapped += PreviousDate;
            previous_icon.GestureRecognizers.Add(previous);

            next_icon = new Image
            {
                Source = "angleRight.png",
                WidthRequest = 40,
                HeightRequest = 40
            };

            var right = new TapGestureRecognizer();
            right.Tapped += NextDate;
            next_icon.GestureRecognizers.Add(right);

            optionImageStack.Children.Add(previous_icon);
            optionImageStack.Children.Add(next_icon);

            optionStack.Children.Add(label);
            optionStack.Children.Add(datePicker);
            optionStack.Children.Add(optionImageStack);

            runSheetHistory = new ListView
            {
                IsPullToRefreshEnabled = true,
                SeparatorColor = Color.White,
                BackgroundColor = Color.White,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                IsVisible = false
            };

            runSheetHistory.Refreshing += runSheetRefresh;

            mainLayout.Children.Add(optionStack);
            mainLayout.Children.Add(runSheetHistory);
            mainLayout.Children.Add(noData);
            mainLayout.Children.Add(activityIndicator);

            CurrentPage.Content = mainLayout;

            if (changePage)
            {
                datePicker.Date = currentDate;
            }
            else
            {
                datePicker.Date = DateTime.Now;
            }

        }

        public void PreviousDate(object sender, EventArgs e)
        {

            previous_icon.IsEnabled = false;
            next_icon.IsEnabled = false;
            option = "previous";
            downloadRunSheet(datePicker.Date.AddDays(-1).ToString("yyyy MM dd"));
        }

        public void NextDate(object sender, EventArgs e)
        {

            previous_icon.IsEnabled = false;
            next_icon.IsEnabled = false;
            option = "next";
            downloadRunSheet(datePicker.Date.AddDays(1).ToString("yyyy MM dd"));
        }


        protected void runSheetRefresh(object sender, EventArgs e)
        {
            downloadRunSheet(datePicker.Date.ToString("yyyy MM dd"));

            runSheetHistory.IsRefreshing = false;
        }

        public async void downloadRunSheet(string date)
        {
            try
            {
                activityIndicator.IsRunning = true;
                firstLoad = false;
                CurrentPage = Children[1];

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadHaulageHistoryURL(date), this);
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

                    App.Database.deleteRecords(runSheetItem.Id);
                    App.Database.deleteRecordSummary(runSheetItem.Id);
                    //App.Database.deleteHaulage("HaulageHistory");

                    foreach (clsHaulageModel data in JobList)
                    {
                        ListItems record = new ListItems
                        {
                            Id = data.Id,
                            Background = data.BackColor,
                            Category = runSheetItem.Id,
                            Name = runSheetItem.Name,
                            TruckId = data.TruckId,
                            ReqSign = data.ReqSign,
                            Latitude = data.Latitude,
                            Longitude = data.Longitude,
                            TelNo = data.TelNo,
                            EventRecordId = data.EventRecordId,
                            TrailerId = data.TrailerId,
                            ContainerNo = data.ContainerNo,
                            MaxGrossWeight = data.MaxGrossWeight,
                            TareWeight = data.TareWeight,
                            CollectSeal = data.CollectSeal,
                            SealNo = data.SealNo,
                            ActionId = data.ActionId.ToString(),
                            ActionMessage = data.ActionMessage,
                            Title = data.Title,
                            SealMode = data.SealMode,
                        };

                        App.Database.SaveMenuAsync(record);

                        foreach (clsCaptionValue summaryList in data.Summary)
                        {
                            SummaryItems summaryItem = new SummaryItems();

                            summaryItem.Id =  data.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = runSheetItem.Id;
                            summaryItem.BackColor = data.BackColor;
                            App.Database.SaveSummarysAsync(summaryItem);
                        }

                    }

                    option = "none";
                    refreshRunSheetHistory();

                }

            }
            catch (Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "OK");
            }

        }

        public void refreshRunSheetHistory()
        {
            if (Device.RuntimePlatform == Device.iOS)
            {
                runSheetHistory.RowHeight = 150;
            }

            Ultis.Settings.List = runSheetItem.Id;
           // ObservableCollection<JobItems> Item = new ObservableCollection<JobItems>(App.Database.GetJobItems(0, "HaulageHistory"));
            ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(runSheetItem.Id));
            //Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu("runsheet"));
            runSheetHistory.ItemsSource = Item;
            runSheetHistory.HasUnevenRows = true;
            runSheetHistory.Style = (Style)App.Current.Resources["recordListStyle"];
            runSheetHistory.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (Item.Count == 0)
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