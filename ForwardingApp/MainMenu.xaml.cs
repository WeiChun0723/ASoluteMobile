using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace ASolute_Mobile
{
    //[XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenu : ContentPage
    {
        public bool doubleBackToExitPressedOnce = false;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();
        string previousLocation = "";
        string chklocation = "0";
        string firebaseID,testing;
        int count = 0;

        public MainMenu()
        {
            InitializeComponent();

            Task.Run(async () => { await StartListening(); });

            OneSignal.Current.IdsAvailable(IdsAvailable);

        }

        public async Task StartListening()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(60), 0, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                DeferralTime = TimeSpan.FromSeconds(1),
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = false,
                
            });

            CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
        }

        public async void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            var position = e.Position;
            chklocation = Math.Round(position.Latitude, 2) + "," + Math.Round(position.Longitude, 2);

            if (chklocation != previousLocation)
            {
                try
                {
                    previousLocation = Math.Round(position.Latitude, 2) + "," + Math.Round(position.Longitude, 2);
                    var locator = CrossGeolocator.Current;
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
            if(Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing",(sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch(Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });

            Title = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama";


            if (NetworkCheck.IsInternet() && Ultis.Settings.UpdatedRecord.Equals("Yes") ) 
            {
                getMainMenu();
                Ultis.Settings.UpdatedRecord = "No";
            }
            else
            {                
                loadMainMenu();
            }

        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            //this.ToolbarItems.Clear();

        }


        //press twice back button to exit the app within 3 second
        protected override bool OnBackButtonPressed()
        {                    
           if (doubleBackToExitPressedOnce)
           {
              base.OnBackButtonPressed();
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
    
        public async void selectMenu(object sender, ItemTappedEventArgs e)
        {
            string menuAction = ((AppMenu)e.Item).menuId;

            switch (menuAction)
            {
                case "LogBook":
                    LogHistory log = new LogHistory();
                    log.previousPage = this;
                    await Navigation.PushAsync(log);
                    break;

                case "FuelCost":
                    RefuelHistory refuel = new RefuelHistory(menuAction);
                    refuel.previousPage = this;
                    await Navigation.PushAsync(refuel);
                    break;

                case "EqInquiry":
                    EquipmentInquiry equipment = new EquipmentInquiry();
                    equipment.previousPage = this;
                    await Navigation.PushAsync(equipment);
                    break;
               
                case "CargoRec":
                    await Navigation.PushAsync(new  TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "CargoLoad":
                    await Navigation.PushAsync(new TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "CargoMeasure":
                    await Navigation.PushAsync(new TransportScreen.PendingReceiving_Loading(menuAction));
                    break;

                case "JobList":
                    //await Navigation.PushAsync(new TransportScreen.JobLists());
                    //await Navigation.PushAsync(new TransportScreen.JobList());
                    await Navigation.PushAsync(new HaulageScreen.JobList());
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
                    await Navigation.PushAsync(new HaulageScreen.RunSheet());
                    break;

                case "Shunting":
                    await Navigation.PushAsync(new HaulageScreen.Shunting(menuAction));
                    break;
                case "PendingCollection":
                    await Navigation.PushAsync(new HaulageScreen.PendingCollection(menuAction));
                    break;
                case "DriverRFC" :
                    await Navigation.PushAsync(new HaulageScreen.DriverRFC());
                    break;
            }
       
            MainMenuList.SelectedItem = null;
        }

        private void IdsAvailable(string userID, string pushToken)
        {
            Debug.WriteLine("UserID:" + userID);
            Debug.WriteLine("pushToken:" + pushToken);

        }

        public async void getMainMenu()
        {
            OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            Ultis.Settings.FireID = firebaseID;

            checkItems.Clear();
            try
            {
                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(firebaseID));
                clsResponse login_response = JsonConvert.DeserializeObject <clsResponse>(content);

                if (login_response.IsGood == true)
                {
                    var login_Menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();
                    
                    //load value from the menu in json response "CheckList"
                     for(int check = 0; check < login_response.Result["Checklist"].Count; check++)
                     {

                         string itemKey = login_response.Result["Checklist"][check]["Key"];
                         string itemValue = login_response.Result["Checklist"][check]["Value"];
                         
                         checkItems.Add(new clsKeyValue(itemKey,itemValue));
                     }
                                    
                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteMainMenu();
                    App.Database.deleteMenuItems("MainMenu");

                    foreach (clsDataRow mainMenu in login_Menu.MainMenu)
                    {
                        AppMenu existingRecord = App.Database.GetMenuRecordAsync(mainMenu.Id);
                        
                        if(mainMenu.Id != "LogOff")
                        {
                            if (existingRecord == null || (!(existingRecord != null)))
                            {
                                if (existingRecord == null)
                                {
                                    existingRecord = new AppMenu();
                                }

                                existingRecord.menuId = mainMenu.Id;


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

                                    if(String.IsNullOrEmpty(summaryList.Caption))
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

                    loadMainMenu();
                    //loadDashBoard();
                    // if the check list not empty then display the check list page
                   if (checkItems.Count != 0)
                    {                  
                        CheckList chkList = new CheckList(checkItems,login_Menu.CheckListLinkId);
                        chkList.previousPage = this;
                        NavigationPage.SetHasBackButton(chkList, false);
                        await Navigation.PushAsync(chkList);
                    }
                }
                else
                {
                    if(login_response.Message == "Invalid Session !")
                    {
                        BackgroundTask.Logout(this);
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }
                    else
                    {
                        await DisplayAlert("Error", login_response.Message, "Ok");
                    }
                    
                }

                activityIndicator.IsRunning = false;
                activityIndicator.IsVisible = false;
            }
            catch(HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later", "Ok");
            }
            catch(Exception exception)
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

        public void refreshMainMenu(object sender, EventArgs e)
        {
            getMainMenu();
            MainMenuList.IsRefreshing = false;
                    
        }


        // load the item that stored in db to the list view by using custom view cell
        public void loadMainMenu()
        {
            Ultis.Settings.ListType = "Main_Menu";
            ObservableCollection<AppMenu> Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());
            MainMenuList.ItemsSource = Item;
            MainMenuList.HasUnevenRows = true;
            MainMenuList.Style = (Style)App.Current.Resources["recordListStyle"];
            MainMenuList.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
        }

        int row = 0;
        int column = 0;

        public async void loadDashBoard()
        {
            ObservableCollection<AppMenu> Items = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());

            try
            {
                for (int i = 1; i < Items.Count; i++)
                {
                    string menuAction = Items[i].menuId;
                    Dashboard_Template dashboard = new Dashboard_Template();

                    switch (menuAction)
                    {
                        case "JobList":
                            dashboard.Icon = "jobList.png";
                            break;
                        default:
                            dashboard.Icon = "refuel.png";
                            break;
                    }
                   

                    dashboard.Label = Items[i].name;

                    var showLabel = new TapGestureRecognizer();
                    showLabel.Tapped += async (sender, e) =>
                    {
                        await DisplayAlert("OK", "Hi " + dashboard.Label, "OK");
                    };
                    dashboard.GestureRecognizers.Add(showLabel);

                    if (row < test.RowDefinitions.Count)
                    {
                        if (column < test.ColumnDefinitions.Count)
                        {

                            test.Children.Add(dashboard, column, row);
                            column++;
                        }
                        else
                        {
                            row++;
                            column = 0;

                            test.Children.Add(dashboard, column, row);
                            column++;
                        }

                    }


                }

            }
            catch(Exception e)
            {
                await DisplayAlert("gg", e.Message, "KO");
            }


        }
    }
}