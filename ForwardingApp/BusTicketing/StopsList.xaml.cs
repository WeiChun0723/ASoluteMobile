using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        }

        List<clsBusTicket> stops;
        ListView BusStopList;
        int StopCount = 0;
        ActivityIndicator activityIndicator;
        bool firstLoad;

        public StopsList(string name)
        {
            try
            {
                InitializeComponent();

                SelectedItem = Children[1];

                PageContent();

                GetStopList();
            }
            catch
            {

            }

        }

        protected override void OnCurrentPageChanged()
        {
            base.OnCurrentPageChanged();

            if(!firstLoad)
            {
                try
                {
                    int index = Children.IndexOf(CurrentPage);

                    if (index == 0)
                    {

                        if (StopCount != 0)
                        {
                            StopCount--;
                            LoadStoplist();
                        }
                        else
                        {
                            StopCount = 0;
                            LoadStoplist();
                        }

                    }
                    else if (index == 2)
                    {

                        if (StopCount < stops.Count)

                        {
                            StopCount++;
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

        public void PageContent()
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
                    VerticalOptions = LayoutOptions.FillAndExpand
                };

                BusStopList.ItemTapped += BusStopList_ItemTapped;

                activityIndicator = new ActivityIndicator
                {
                    IsRunning = true,
                    IsVisible = true
                };

                /*DataTemplate dt = new DataTemplate(typeof(TextCell));
                dt.SetBinding(TextCell.TextProperty, new Binding("StopName"));
                dt.SetBinding(TextCell.TextColorProperty, new Binding("BackColor"));
                BusStopList.ItemTemplate = dt;
                BusStopList.IsPullToRefreshEnabled = false;
                //BusStopList.Refreshing += BusStopList_Refreshing;*/

                DataTemplate dt = new DataTemplate(() =>
               {
                   Label stopName = new Label{
                       HorizontalOptions =  LayoutOptions.FillAndExpand,
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

                mainLayout.Children.Add(activityIndicator);
                mainLayout.Children.Add(BusStopList);

                CurrentPage.Content = mainLayout;
            }
            catch
            {

            }
           
        }

        void BusStopList_Refreshing(object sender, EventArgs e)
        {
            GetStopList();
        }

        async void BusStopList_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            string menuAction = ((SummaryItems)e.Item).StopName;

            await Navigation.PushAsync(new Ticket(stops[StopCount].StopName, ((SummaryItems)e.Item).StopName, ((SummaryItems)e.Item).Rate.ToString()));
        }


        public async void GetStopList()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, "https://api.asolute.com/host/api/", ControllerUtil.getBusStops());
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    stops = JObject.Parse(content)["Result"].ToObject<List<clsBusTicket>>();

                    App.Database.deleteMainMenuItem("BusTicketing");
                    App.Database.deleteMenuItems("BusTicketing");

                    foreach (clsBusTicket stop in stops)
                    {
                        ListItems items = new ListItems
                        {
                            StopId = stop.StopId,
                            StopName = stop.StopName,
                            Rate = stop.Rate,
                            category = "BusTicketing"
                        };

                        App.Database.SaveMenuAsync(items);

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
                            summaryItem.Type = "BusTicketing";

                            App.Database.SaveSummarysAsync(summaryItem);
                        }
                    }

                    var test = App.Database.GetMainMenu("BusTicketing");

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
                string id = stops[StopCount].StopId;
                Title = "Route: " + id;
                var Item = new ObservableCollection<SummaryItems>(App.Database.GetSummarysAsync(id, "BusTicketing"));
                //var Item = stops[StopCount].Stops;
                Item.Insert(0, new SummaryItems { StopId = stops[StopCount].StopId, StopName = stops[StopCount].StopName, Rate = stops[StopCount].Rate, BackColor= "#32cd32" });
                BusStopList.ItemsSource = Item;
                activityIndicator.IsVisible = false;

                CurrentPage = Children[1];
                firstLoad = false;
            }
            catch(Exception ex)
            {
                string test = ex.Message;
            }

        }
    }
}
