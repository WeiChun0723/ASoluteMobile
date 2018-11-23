﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace ASolute_Mobile
{
	public class MainMenuItem : ListViewCommonScreen
    {

        public bool doubleBackToExitPressedOnce = false;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();
        string previousLocation = "";
        string chklocation = "0";
        string firebaseID;


        public MainMenuItem()
        {
           
            Title = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama";

            listView.ItemTapped += async (sender, e) =>
            {
                string menuAction = ((AppMenu)e.Item).menuId;

                switch (menuAction)
                {
                    case "LogBook":
                        LogHistory log = new LogHistory();
                        await Navigation.PushAsync(log);
                        break;

                    case "FuelCost":
                        RefuelHistory refuel = new RefuelHistory(((AppMenu)e.Item).name);                
                        await Navigation.PushAsync(refuel);
                        break;

                    case "EqInquiry":
                        EquipmentInquiry equipment = new EquipmentInquiry();            
                        await Navigation.PushAsync(equipment);
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
                       //await Navigation.PushAsync(new TransportScreen.JobLists(((AppMenu)e.Item).action, ((AppMenu)e.Item).name));
                        //await Navigation.PushAsync(new TransportScreen.JobList(((AppMenu)e.Item).action, ((AppMenu)e.Item).name));
                        await Navigation.PushAsync(new HaulageScreen.JobList(((AppMenu)e.Item).name));
                       // await Navigation.PushAsync(new ChatRoom());
                        break;

                    case "MasterJobList":
                        await Navigation.PushAsync(new TransportScreen.JobList(((AppMenu)e.Item).action, ((AppMenu)e.Item).name));
                        break;
                    case "CargoReturn":
                        string return_id = "test";
                        string return_eventID = "1000";
                        /*if(!(String.IsNullOrEmpty(Ultis.Settings.CargoReturn)))
                         {
                             string[] info = Ultis.Settings.CargoReturn.Split(',');
                             return_id = info[0];
                             return_eventID = info[1];

                         }*/
                        long test = Convert.ToInt64(return_eventID);
                        await Navigation.PushAsync(new TransportScreen.Futile_CargoReturn("CargoReturn", return_id, test));
                        break;

                    case "RunSheet":
                        await Navigation.PushAsync(new HaulageScreen.RunSheet(((AppMenu)e.Item).name));
                        break;

                    case "Shunting":
                        await Navigation.PushAsync(new HaulageScreen.Shunting(menuAction));
                        break;
                    case "PendingCollection":
                        await Navigation.PushAsync(new HaulageScreen.PendingCollection(menuAction));
                        break;
                    case "DriverRFC":
                        await Navigation.PushAsync(new HaulageScreen.DriverRFC());
                        break;
                }

                listView.SelectedItem = null;
            };

            listView.Refreshing += (sender, e) =>
            {
                GetMainMenu();
                Ultis.Settings.UpdatedRecord = "RefreshJobList";
                Ultis.Settings.RefreshMenuItem = "NO";
                listView.IsRefreshing = false;
            };

        }

        public async Task StartListening()
        {
            var locator = CrossGeolocator.Current;


            if (!locator.IsListening)
            {

                //Start listening to location updates
                await locator.StartListeningAsync(TimeSpan.FromSeconds(60), 0, true,new Plugin.Geolocator.Abstractions.ListenerSettings
                {
                    ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    ListenForSignificantChanges = true,
                    DeferLocationUpdates = true,
                    DeferralDistanceMeters = 1,
                    DeferralTime = TimeSpan.FromSeconds(1),
                    PauseLocationUpdatesAutomatically = false
                });

                locator.PositionChanged += Current_PositionChanged;
  
            }
        }


        public async void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var position = e.Position;
            var locator = CrossGeolocator.Current;


            if(position == null)
            {
                Task.Run(async () => { position = await locator.GetLastKnownLocationAsync(); }).Wait();
            }

            chklocation = Math.Round(position.Latitude, 4) + "," + Math.Round(position.Longitude, 4);

            if (chklocation != previousLocation)
            {
                try
                {
                    previousLocation = Math.Round(position.Latitude, 4) + "," + Math.Round(position.Longitude, 4);

                    var getAddress = await locator.GetAddressesForPositionAsync(position);
                    var addressDetail = getAddress.FirstOrDefault();
                    string address = addressDetail.Thoroughfare + "," + addressDetail.PostalCode + "," + addressDetail.Locality + "," + addressDetail.AdminArea + "," + addressDetail.CountryName;
                    string location = position.Latitude + "," + position.Longitude;

                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var uri = ControllerUtil.getGPSTracking(location, address);
                    var response = await client.GetAsync(uri);
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(content);
                    clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (gps_response.IsGood)
                    {
                       // await DisplayAlert("OK", chklocation, "OK");
                        previousLocation = location;

                        App.gpsLocationLat = position.Latitude;
                        App.gpsLocationLong = position.Longitude;
                    }
                }
                catch (Exception ex)
                {
                    string error = ex.Message;
                }

            }
        }

        protected override void OnAppearing()
        {
            Task.Run(async () => { await StartListening(); });

            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });

            if (NetworkCheck.IsInternet() && Ultis.Settings.RefreshMenuItem.Equals("Yes"))
            {
                GetMainMenu();
                Ultis.Settings.UpdatedRecord = "RefreshJobList";
                Ultis.Settings.RefreshMenuItem = "NO";
            }
            else
            {
                LoadMainMenu();
            }

        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        //press twice back button to exit the app within 3 second
        protected override bool OnBackButtonPressed()
        {
            if (doubleBackToExitPressedOnce)
            {
                base.OnBackButtonPressed();
                MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
                CloseApp();
            }

            doubleBackToExitPressedOnce = true;
            var toastConfig = new ToastConfig("Press back button again to exit");
            toastConfig.SetDuration(3000);
            toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(12, 131, 193));
            UserDialogs.Instance.Toast(toastConfig);

            Task.Run(async () =>
            {
                await Task.Delay(3000);
                doubleBackToExitPressedOnce = false;
            }
            );

            return true;
        }

        public async void GetMainMenu()
        {
            loading.IsRunning = true;
            loading.IsVisible = true;

            OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            Ultis.Settings.FireID = firebaseID;

            checkItems.Clear();
            try
            {
              
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(firebaseID));
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood == true)
                {
                    var login_Menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    //load value from the menu in json response "CheckList"
                    for (int check = 0; check < login_response.Result["Checklist"].Count; check++)
                    {

                        string itemKey = login_response.Result["Checklist"][check]["Key"];
                        string itemValue = login_response.Result["Checklist"][check]["Value"];

                        checkItems.Add(new clsKeyValue(itemKey, itemValue));
                    }

                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteMainMenu();
                    App.Database.deleteMenuItems("MainMenu");

                    foreach (clsDataRow mainMenu in login_Menu.MainMenu)
                    {
                        AppMenu existingRecord = App.Database.GetMenuRecordAsync(mainMenu.Id);

                        if (mainMenu.Id != "LogOff")
                        {
                            if (existingRecord == null || (!(existingRecord != null)))
                            {
                                if (existingRecord == null)
                                {
                                    existingRecord = new AppMenu();
                                }

                                existingRecord.menuId = mainMenu.Id;
                                existingRecord.name = mainMenu.Caption;


                                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(mainMenu.Id, "MainMenu");

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
                                        existingRecord.name = summaryList.Value;
                                    }

                                    summaryItem.Id = mainMenu.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = "MainMenu";
                                    summaryItem.BackColor = mainMenu.BackColor;
                                    App.Database.SaveSummarysAsync(summaryItem);
                                    index++;
                                }

                                App.Database.SaveMenuAsync(existingRecord);
                                if (existingSummaryItems != null)
                                {
                                    for (; index < existingSummaryItems.Count; index++)
                                    {
                                        App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                                    }
                                }
                            }
                        }
                    }

                    foreach (clsDataRow contextMenu in login_Menu.ContextMenu)
                    {
                        List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(contextMenu.Id, "ContextMenu");

                        int index = 0;
                        foreach (clsCaptionValue summaryList in contextMenu.Summary)
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

                            summaryItem.Id = contextMenu.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = "ContextMenu";
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

                    LoadMainMenu();
                    //loadDashBoard();
                    // if the check list not empty then display the check list page
                    if (checkItems.Count != 0)
                    {
                        CheckList chkList = new CheckList(checkItems, login_Menu.CheckListLinkId);
                        NavigationPage.SetHasBackButton(chkList, false);
                        await Navigation.PushAsync(chkList);
                    }
                }
                else
                {
                    if (login_response.Message == "Invalid Session !")
                    {
                        BackgroundTask.Logout(this);
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }

                }

              
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }

           
        }

        public void CloseApp()
        {
            if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
            {
                DependencyService.Get<CloseApp>().close_app();
            }
        }

        // load the item that stored in db to the list view by using custom view cell
        public void LoadMainMenu()
        {
            loading.IsRunning = false;
            loading.IsVisible = false;

            Ultis.Settings.ListType = "Main_Menu";
            ObservableCollection<AppMenu> Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());
            listView.ItemsSource = Item;
            listView.HasUnevenRows = true;
            listView.Style = (Style)App.Current.Resources["recordListStyle"];
            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
        }

    }
}