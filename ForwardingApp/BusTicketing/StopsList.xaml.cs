using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.BusTicketing
{

    public partial class StopsList : CarouselPage
    {
        public class clsBusTicket
        {
            public string StopId { get; set; }
            public string StopName { get; set; }
            public double Rate { get; set; }
            public List<ListItems> Stops { get; set; }
            public string BackColor { get; set; }
        }

        ObservableCollection<ListItems> localStoredStops ;
        List<clsBusTicket> stops;
        //List<SummaryItems> listViewStops = new List<SummaryItems>();
        ListView BusStopList;
        int OutCount = 0;
        ActivityIndicator activityIndicator;
        bool firstLoad = true;
        string action;

        public StopsList(string name)
        {

            InitializeComponent();

            action = name;

            SelectedItem = Children[1];

            PageContent();

            localStoredStops = new ObservableCollection<ListItems>(App.Database.GetMainMenu(action));

            if (NetworkCheck.IsInternet() && localStoredStops.Count == 0)
            {
                GetStopList();
            }
            else
            {
                LoadStoplist();
            }
        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            if (!firstLoad)
            {
                try
                {
                    int index = Children.IndexOf(CurrentPage);

                    if (index == 0)
                    {

                        if (OutCount != 0)
                        {
                            OutCount--;
                          
                            LoadStoplist();
                        }
                        else
                        {
                            OutCount = 0;
                            LoadStoplist();
                        }

                    }
                    else if (index == 2)
                    {

                        if (OutCount < localStoredStops.Count)
                        {

                            OutCount++;
                            LoadStoplist();
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayAlert("Error", ex.Message, "OK");
                }
            }
        }

        public async void PageContent()
        {
            try
            {
                StackLayout mainLayout = new StackLayout
                {
                    Padding = new Thickness(10, 10, 10, 10)
                };

                mainLayout.Children.Clear();

                BusStopList = new ListView
                {
                    SeparatorColor = Color.White,
                    BackgroundColor = Color.White,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = 250
                };

                BusStopList.ItemTapped += BusStopList_ItemTapped;

                activityIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    IsVisible = true
                };


                DataTemplate dt = new DataTemplate(() =>
               {
                   Label stopName = new Label
                   {
                       HorizontalOptions = LayoutOptions.FillAndExpand,
                       TextColor = Color.Black,
                       FontSize = 20
                   };

                   stopName.SetBinding(Label.TextProperty, "StopName");

                   StackLayout layout = new StackLayout
                   {
                       HorizontalOptions = LayoutOptions.FillAndExpand,
                       VerticalOptions = LayoutOptions.FillAndExpand
                   };

                   layout.SetBinding(BackgroundColorProperty, new Binding("BackColor"));
                   layout.Children.Add(stopName);

                   var menuFrame = new Frame
                   {
                       Margin = 5,
                       HasShadow = true,
                       Content = layout
                   };

                   menuFrame.SetBinding(BackgroundColorProperty, new Binding("BackColor"));

                   return new ViewCell { View = menuFrame };
               });

                BusStopList.RowHeight = 100;
                BusStopList.ItemTemplate = dt;
                BusStopList.Refreshing += BusStopList_Refreshing;

                mainLayout.Children.Add(activityIndicator);
                mainLayout.Children.Add(BusStopList);

                CurrentPage.Content = mainLayout;
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        void BusStopList_Refreshing(object sender, EventArgs e)
        {
            if(NetworkCheck.IsInternet())
            {
                GetStopList();
            }

        }

        async void BusStopList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            //string menuAction = ((SummaryItems)e.Item).StopName;

            await Navigation.PushAsync(new Ticket(localStoredStops[OutCount], ((SummaryItems)e.Item).StopName, ((SummaryItems)e.Item).Rate));
        }

        public async void GetStopList()
        {
            try
            {
                string listtype = (action == "Inbound Trip") ? "InboundList" : "OutboundList";

                var content = await CommonFunction.CallWebService(0, null, "https://api.asolute.com/host/api/", ControllerUtil.getBusStops(listtype), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    stops = JObject.Parse(content)["Result"].ToObject<List<clsBusTicket>>();


                    App.Database.deleteRecords(action);
                    App.Database.deleteRecordSummary(action);


                    foreach (clsBusTicket stop in stops)
                    {

                        ListItems items = new ListItems
                        {
                            StopId = stop.StopId,
                            StopName = stop.StopName,
                            Rate = stop.Rate,
                            Category = action
                        };

                        App.Database.SaveItemAsync(items);

                        foreach (ListItems station in stop.Stops)
                        {
                            SummaryItems summaryItem = new SummaryItems
                            {
                                Id = stop.StopId,
                                StopId = station.StopId,
                                StopName = station.StopName,
                                Rate = station.Rate,
                                BackColor = "#ffffff"
                            };
                            summaryItem.Type = action;

                           
                            App.Database.SaveSummarysAsync(summaryItem);
                        }
                    }

                    var test = App.Database.GetMainMenu(action);

                    LoadStoplist();
                }
                else
                {
                    await DisplayAlert("Error", response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        void LoadStoplist()
        {
            try
            {
                activityIndicator.IsRunning = true;
                Ultis.Settings.List = "BusTicketing";
                string id = "";

                localStoredStops = new ObservableCollection<ListItems>(App.Database.GetMainMenu(action));

                id = localStoredStops[OutCount].StopId;

                Title = "Route: " + id;
                var Item = new List<SummaryItems>(App.Database.GetSummarysAsync(localStoredStops[OutCount].StopId, action));

                Item.Insert(0, new SummaryItems { StopId = localStoredStops[OutCount].StopId, StopName = localStoredStops[OutCount].StopName, Rate = localStoredStops[OutCount].Rate, BackColor = "#32cd32" });

                BusStopList.ItemsSource = Item;
                activityIndicator.IsVisible = false;

                CurrentPage = Children[1];
                firstLoad = false;
            }
            catch (Exception ex)
            {
                string test = ex.Message;
            }

        }
    }
}
