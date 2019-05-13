using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.BusTicketing;
using ASolute_Mobile.CommonScreen;
using ASolute_Mobile.Models;
using ASolute_Mobile.Planner;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.Buttons;
using Xamarin.Essentials;
using Xamarin.Forms;


namespace ASolute_Mobile
{
    //all of the app share this main menu after they login to the app
    public partial class NewMainMenu : ContentPage
    {
        Label title1, title2;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();
        string firebaseID = "";

        public NewMainMenu()
        {

            InitializeComponent();

            switch(Ultis.Settings.App)
            {
                case "asolute.Mobile.AILSBUS":
                    headerImage.Source = "busticketingheader.png";
                    StartEndButton.IsVisible = true;
                    break;

                case "asolute.Mobile.AILSWMS":
                    headerImage.Source = "warehouseheader.png";
                    break;

                case "asolute.Mobile.AILSHaulage":
                    headerImage.Source = "headerBackground.png";
                    break;

                case "asolute.Mobile.AILSYard":
                    headerImage.Source = "yardheader.png";
                    break;
            }

            StackLayout main = new StackLayout();
            title1 = new Label
            {
                FontSize = 15,
                Text = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama",
                TextColor = Color.White
            };
            title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };
            main.Children.Add(title1);
            main.Children.Add(title2);
            NavigationPage.SetTitleView(this, main);

            if(!(String.IsNullOrEmpty(Ultis.Settings.StartEndStatus)))
            {
                StartEndButton.Text = Ultis.Settings.StartEndStatus;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //get the latest store local profile image
            var image = App.Database.GetUserProfilePicture(Ultis.Settings.SessionUserItem.DriverId);
            profilePicture.Source = (image != null && image.imageData != null) ? ImageSource.FromStream(() => new MemoryStream(image.imageData)) : "user_icon.png";
            LoadMainMenu();
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new UserInfo());
        }

        async void GetMainMenu()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response != null)
                {
                    var menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();
                    Ultis.Settings.SubTitle = menu.SubTitle;
                    title2.Text = Ultis.Settings.SubTitle;

                    //clear and load value from the menu in json response "CheckList"
                    checkItems.Clear();
                    for (int check = 0; check < response.Result["Checklist"].Count; check++)
                    {
                        string itemKey = response.Result["Checklist"][check]["Key"];
                        string itemValue = response.Result["Checklist"][check]["Value"];
                        checkItems.Add(new clsKeyValue(itemKey, itemValue));
                    }

                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteRecords("MainMenu");
                    App.Database.deleteRecordSummary("MainMenu");
                    App.Database.deleteRecordSummary("UserInfo");
                    App.Database.deleteRecordSummary("ContextMenu");
                    foreach (clsDataRow mainMenu in menu.MainMenu)
                    {
                        switch (mainMenu.Id)
                        {
                            //display expiry date info
                            case "Expiry":
                                expiryStack.IsVisible = true;
                                if (expiryGrid.IsVisible == true)
                                {
                                    expiryGrid.ColumnDefinitions.Clear();
                                }
                                mainGrid.Children.Add(expiryStack, 0, 2);
                                expiryLabel.Text = mainMenu.Caption;
                                expiryGrid.Children.Clear();
                                
                                int rowIndex = 0, columnIndex = 0;
                                foreach (clsCaptionValue expiryInfo in mainMenu.Summary)
                                {
                                    if (expiryInfo.Caption != "")
                                    {
                                        expiryGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                        StackLayout expiryInfoStack = new StackLayout
                                        {
                                            BackgroundColor = Color.FromHex(mainMenu.BackColor),
                                            Padding = new Thickness(0, 10, 0, 10)
                                        };

                                        Label date = new Label
                                        {
                                            Style = (Xamarin.Forms.Style)Application.Current.Resources["StatsNumberLabel"],
                                            Text = expiryInfo.Value,
                                            TextColor = Color.FromHex("#696969")
                                        };

                                        Label caption = new Label
                                        {
                                            Style = (Xamarin.Forms.Style)Application.Current.Resources["StatsCaptionLabel"],
                                            Text = expiryInfo.Caption,
                                            TextColor = Color.FromHex("#696969")
                                        };

                                        expiryInfoStack.Children.Add(date);
                                        expiryInfoStack.Children.Add(caption);

                                        expiryGrid.Children.Add(expiryInfoStack, columnIndex, rowIndex);
                                        columnIndex++;
                                    }

                                }
                                break;

                            default:
                                ListItems mainMenuItems = new ListItems();
                                mainMenuItems.Id = mainMenu.Id;
                                mainMenuItems.Name = mainMenu.Caption;
                                mainMenuItems.Action = mainMenu.Action;
                                mainMenuItems.Category = (mainMenu.Id == "Info") ? "UserInfo" : "MainMenu";

                                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(mainMenu.Id, mainMenuItems.Category);
                                int index = 0;
                                foreach (clsCaptionValue summaryList in mainMenu.Summary)
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

                                    if (String.IsNullOrEmpty(summaryList.Caption))
                                    {
                                        mainMenuItems.Name = summaryList.Value;
                                    }

                                    summaryItem.Id = mainMenu.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = mainMenuItems.Category;
                                    summaryItem.BackColor = mainMenu.BackColor;
                                    App.Database.SaveSummarysAsync(summaryItem);
                                    index++;
                                }
                                App.Database.SaveMenuAsync(mainMenuItems);
                                break;
                        }
                    }

                    foreach (clsDataRow contextMenu in menu.ContextMenu)
                    {
                        List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(contextMenu.Id, "ContextMenu");
                        int index = 0;
                        foreach (clsCaptionValue summaryList in contextMenu.Summary)
                        {
                            SummaryItems summaryItem = new SummaryItems();
                            summaryItem.Id = contextMenu.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = "ContextMenu";
                            App.Database.SaveSummarysAsync(summaryItem);
                            index++;
                        }
                    }

                    LoadMainMenu();

                    if (checkItems.Count != 0)
                    {
                        CheckList chkList = new CheckList(checkItems, menu.CheckListLinkId);
                        NavigationPage.SetHasBackButton(chkList, false);
                        await Navigation.PushAsync(chkList);
                    }
                }
                loading.IsVisible = false;
                pullToRefresh.IsRefreshing = false;
            }
            catch (Exception ex)
            {
                 await DisplayAlert("Exception", ex.Message, "OK");
            }
        }

        async void LoadMainMenu()
        {
            try
            {

                //load user account info
                List<SummaryItems> information = App.Database.GetSummarysAsync("Info", "UserInfo");
                userInfo.Children.Clear();
                foreach (SummaryItems userSummary in information)
                {
                    string labelStyle = (userSummary.Caption == "") ? "ProfileNameLabel" : "ProfileTagLabel";

                    Label info = new Label
                    {
                        Style = (Xamarin.Forms.Style)Application.Current.Resources[labelStyle],
                        HorizontalTextAlignment = TextAlignment.Center
                    };

                    info.Text = (userSummary.Caption == "") ? userSummary.Value : userSummary.Caption + ": " + userSummary.Value;

                    userInfo.Children.Add(info);
                }

                //load menu item with custom template
                loading.IsVisible = true;
                Ultis.Settings.List = "Main_Menu";
                ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu("MainMenu"));
                listView.ItemsSource = Item;
                listView.HeightRequest = (expiryStack.IsVisible == false) ? Item.Count * 80 : Item.Count * 100;
                listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
                System.TimeSpan interval = new System.TimeSpan();
                if (!(String.IsNullOrEmpty(Ultis.Settings.UpdateTime)))
                {
                    DateTime enteredDate = DateTime.Parse(Ultis.Settings.UpdateTime);
                    interval = DateTime.Now.Subtract(enteredDate);
                }

                if(NetworkCheck.IsInternet())
                {
                    if (Item.Count == 0 || userInfo.Children.Count == 0 || Ultis.Settings.RefreshListView == "Yes" || interval.Hours >= 1 || interval.Hours < 0)
                    {
                        GetMainMenu();
                        Ultis.Settings.RefreshListView = "No";
                        Ultis.Settings.UpdateTime = DateTime.Now.ToString();
                    }
                }

            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string menuAction = ((ListItems)e.Item).Id;
            switch (menuAction)
            {
                case "LogBook":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getLogHistoryURL(DateTime.Now.ToString("yyyy-MM-dd"))));
                    break;

                case "FuelCost":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getDownloadRefuelHistoryURL()));
                    break;

                case "EqInquiry":
                    await Navigation.PushAsync(new EquipmentInquiry());
                    break;

                case "CargoRec":
                    await Navigation.PushAsync(new TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "CargoLoad":
                    await Navigation.PushAsync(new TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "CargoMeasure":
                    await Navigation.PushAsync(new TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "JobList":
                    //await Navigation.PushAsync(new TransportScreen.JobList(((AppMenu)e.Item).action, ((AppMenu)e.Item).name));
                    //await Navigation.PushAsync(new HaulageScreen.JobList(((ListItems)e.Item).Name));
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getHaulageJobListURL()));
                    break;

                case "MasterJobList":
                    await Navigation.PushAsync(new TransportScreen.JobList(((ListItems)e.Item).Action, ((ListItems)e.Item).Name));
                    break;

                case "CargoReturn":
                    break;

                case "RunSheet":
                    await Navigation.PushAsync(new HaulageScreen.RunSheet(((ListItems)e.Item)));
                    break;

                case "Shunting":
                    await Navigation.PushAsync(new HaulageScreen.Shunting(((ListItems)e.Item).Name));
                    break;

                case "PendingCollection":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPendingCollectionURL()));
                    break;

                case "DriverRFC":
                    await Navigation.PushAsync(new HaulageScreen.DriverRFC(((ListItems)e.Item).Name));
                    break;

                case "EqList":
                    await Navigation.PushAsync(new Planner.EqCategory());
                    break;

                case "MapView":
                    await Navigation.PushAsync(new AllTruckMap(((ListItems)e.Item).Name));
                    break;

                case "TallyIn":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getTallyInListURL()));
                    break;

                case "PalletTrx":
                    await Navigation.PushAsync(new WMS_Screen.PalletMovement(((ListItems)e.Item)));
                    break;

                case "Packing":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPackingListURL()));
                    break;

                case "TallyOut":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getTallyOutListURL()));
                    break;

                case "Picking":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPickingListURL(((ListItems)e.Item).Id)));
                    break;

                case "FullPick":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPickingListURL(((ListItems)e.Item).Id)));
                    break;

                case "LoosePick":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPickingListURL(((ListItems)e.Item).Id)));
                    break;

                case "PickingVerify":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPickingListURL(((ListItems)e.Item).Id)));
                    break;

                case "InboundTrip":
                    await Navigation.PushAsync(new StopsList(((ListItems)e.Item).Name));
                    break;

                case "OutboundTrip":
                    await Navigation.PushAsync(new StopsList(((ListItems)e.Item).Name));
                    break;

                case "DailyCash":
                    await Navigation.PushAsync(new CheckOut());
                    break;

                case "PendingStorage":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getPendingStorage()));
                    break;

                case "ContainerInquiry":
                    await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item), ControllerUtil.getCollectionInquiry()));
                    break;

                case "TicketHistory":
                    await Navigation.PushAsync(new HaulageScreen.RunSheet(((ListItems)e.Item)));
                    break;
            }

            listView.SelectedItem = null;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetMainMenu();
        }
       
        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs e)
        {
            
        }

        #region AILS BUS function
        //visible for bus ticketing app for record start time and end time of the trip
        void Handle_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as SfButton;

            switch (button.Text)
            {
                case "Start":
                    Ultis.Settings.StartEndStatus = "End";
                    SaveOfflineTrip("Start");
                    break;

                case "End":
                    Ultis.Settings.StartEndStatus = "Start";
                    SaveOfflineTrip("End");
                    break;
            }

            StartEndButton.Text = Ultis.Settings.StartEndStatus;
        }

        async void SaveOfflineTrip(string startEnd)
        {
            try
            {
                BusTrip busTrip;

                var trip = App.Database.GetBusTrip();

                if (trip == null || trip.EndTime != null)
                {
                    busTrip = new BusTrip();
                    Guid randomID = Guid.NewGuid();
                    busTrip.Id = randomID.ToString();
                    busTrip.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                    busTrip.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                    busTrip.Uploaded = false;

                    Ultis.Settings.TripRecordID = randomID.ToString();
                }
                else
                {
                    busTrip = trip;
                }

                var location = await Geolocation.GetLastKnownLocationAsync();

                switch (startEnd)
                {
                    case "Start":
                        busTrip.StartTime = DateTime.Now;
                        busTrip.StartGeoLoc = location.Latitude.ToString() + "," + location.Longitude.ToString();

                        break;

                    case "End":
                        busTrip.EndTime = DateTime.Now;
                        busTrip.EndGeoLoc = location.Latitude.ToString() + "," + location.Longitude.ToString();
                        break;
                }

                App.Database.SaveBusTrip(busTrip);
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }
        #endregion
    }
}
