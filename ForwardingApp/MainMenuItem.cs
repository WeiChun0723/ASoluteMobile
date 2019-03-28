using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.BusTicketing;
using ASolute_Mobile.Models;
using ASolute_Mobile.Planner;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public class MainMenuItem : ListViewCommonScreen
    {
        public bool doubleBackToExitPressedOnce = false;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();
        //string firebaseID = "firebase";

        public MainMenuItem()
        {
            search.IsVisible = false;

            ListViewCommonScreen.title1.Text = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama";

            listView.ItemTapped += async (sender, e) =>
            {
                loading.IsVisible = true;

                string menuAction = ((ListItems)e.Item).Id;

                switch (menuAction)
                {
                    case "LogBook":
                        LogHistory log = new LogHistory(((ListItems)e.Item).Name);
                        await Navigation.PushAsync(log);
                        break;

                    case "FuelCost":
                        RefuelHistory refuel = new RefuelHistory(((ListItems)e.Item).Name);
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
                        //await Navigation.PushAsync(new TransportScreen.JobList(((AppMenu)e.Item).action, ((AppMenu)e.Item).name));
                        //await Navigation.PushAsync(new HaulageScreen.JobList(((ListItems)e.Item).Name));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getListURL()));
                        // await Navigation.PushAsync(new ChatRoom());
                        break;

                    case "MasterJobList":
                        await Navigation.PushAsync(new TransportScreen.JobList(((ListItems)e.Item).Action, ((ListItems)e.Item).Name));
                        break;
                    case "CargoReturn":
                        /*string return_id = "test";
                        string return_eventID = "1000";
                        if(!(String.IsNullOrEmpty(Ultis.Settings.CargoReturn)))
                         {
                             string[] info = Ultis.Settings.CargoReturn.Split(',');
                             return_id = info[0];
                             return_eventID = info[1];

                         }*/
                        //long test = Convert.ToInt64(return_eventID);
                        //await Navigation.PushAsync(new TransportScreen.Futile_CargoReturn("CargoReturn", return_id, test));
                        break;

                    case "RunSheet":
                        await Navigation.PushAsync(new HaulageScreen.RunSheet(((ListItems)e.Item).Name));
                        break;

                    case "Shunting":
                        await Navigation.PushAsync(new HaulageScreen.Shunting(((ListItems)e.Item).Name));
                        break;
                    case "PendingCollection":
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getPendingCollectionURL()));
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
                        //Ultis.Settings.Title = ((AppMenu)e.Item).name;
                        //await Navigation.PushAsync(new WMS_Screen.TallyInList(((AppMenu)e.Item).name));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getTallyInList()));
                        break;
                    case "PalletTrx":
                        await Navigation.PushAsync(new WMS_Screen.PalletMovement(((ListItems)e.Item).Name));
                        break;
                    case "Packing":
                        //await Navigation.PushAsync(new WMS_Screen.Packing(((AppMenu)e.Item).name));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getPackingList()));
                        break;
                    case "TallyOut":
                        //await Navigation.PushAsync(new WMS_Screen.TallyOut(((AppMenu)e.Item).name));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getTallyOutList()));
                        break;
                    case "FullPick":
                        //await Navigation.PushAsync(new WMS_Screen.Picking(((AppMenu)e.Item).name, ((AppMenu)e.Item).menuId));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getPickingList(((ListItems)e.Item).Id)));
                        break;
                    case "LoosePick":
                        //await Navigation.PushAsync(new WMS_Screen.Picking(((AppMenu)e.Item).name, ((AppMenu)e.Item).menuId));
                        await Navigation.PushAsync(new ListViewTemplate(((ListItems)e.Item).Name, ControllerUtil.getPickingList(((ListItems)e.Item).Id)));
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

                }

                loading.IsVisible = false;

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

        protected override void OnAppearing()
        {
            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) =>
            {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });


            System.TimeSpan interval = new System.TimeSpan();
            if (!(String.IsNullOrEmpty(Ultis.Settings.UpdateTime)))
            {
                DateTime enteredDate = DateTime.Parse(Ultis.Settings.UpdateTime);
                interval = DateTime.Now.Subtract(enteredDate);
            }

            if (Ultis.Settings.RefreshMenuItem == "Yes" || interval.Hours >= 1 || interval.Hours < 0)
            {
                GetMainMenu();
                Ultis.Settings.UpdatedRecord = "RefreshJobList";
                Ultis.Settings.RefreshMenuItem = "No";
                Ultis.Settings.UpdateTime = DateTime.Now.ToString();
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
        /*protected override bool OnBackButtonPressed()
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
        }*/

        public async void GetMainMenu()
        {

            loading.IsVisible = true;

            /*OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            Ultis.Settings.FireID = firebaseID;*/

            checkItems.Clear();
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(), this);
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood == true)
                {
                    var login_Menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    Ultis.Settings.SubTitle = login_Menu.SubTitle;
                    ListViewCommonScreen.title2.Text = Ultis.Settings.SubTitle;

                    //load value from the menu in json response "CheckList"
                    for (int check = 0; check < login_response.Result["Checklist"].Count; check++)
                    {
                        string itemKey = login_response.Result["Checklist"][check]["Key"];
                        string itemValue = login_response.Result["Checklist"][check]["Value"];

                        checkItems.Add(new clsKeyValue(itemKey, itemValue));
                    }

                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteRecords("MainMenu");
                    App.Database.deleteRecordSummary("MainMenu");

                    foreach (clsDataRow mainMenu in login_Menu.MainMenu)
                    {
                        if (mainMenu.Id != "LogOff")
                        {
                            ListItems mainMenuItems = new ListItems();

                            mainMenuItems.Id = mainMenu.Id;
                            mainMenuItems.Name = mainMenu.Caption;
                            mainMenuItems.Action = mainMenu.Action;
                            mainMenuItems.Category = "MainMenu";

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
                                    mainMenuItems.Name = summaryList.Value;
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

                            App.Database.SaveMenuAsync(mainMenuItems);
                            if (existingSummaryItems != null)
                            {
                                for (; index < existingSummaryItems.Count; index++)
                                {
                                    App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
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
                    await DisplayAlert("Error", login_response.Message, "Ok");
                }
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Unable to connect", "Please try again later.", "Ok");
            }
            catch (Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }
        }

        /* public void CloseApp()
         {
             if (Device.RuntimePlatform == Device.Android)
             {

                 //LocationApp.StopLocationService();
                 DependencyService.Get<CloseApp>().close_app();

             }
         }*/

        // load the item that stored in db to the list view by using custom view cell
        public void LoadMainMenu()
        {
            loading.IsVisible = false;

            Ultis.Settings.List = "Main_Menu";
            ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu("MainMenu"));
            listView.ItemsSource = Item;
            listView.HasUnevenRows = true;
            //IOS need to predefine the row height if not will truncated 
            if (Device.RuntimePlatform == Device.iOS)
            {
                listView.RowHeight = 150;
            }
            listView.Style = (Style)App.Current.Resources["recordListStyle"];
            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
        }

    }
}
