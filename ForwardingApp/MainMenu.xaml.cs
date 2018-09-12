using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.HaulageScreen;
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
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainMenu : ContentPage
	{
        public bool doubleBackToExitPressedOnce = false;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();


        public MainMenu ()
		{
			InitializeComponent ();
            BackgroundTask.StartListening();          
        }

       protected override void OnAppearing()
        {
            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Main Menu";
            }
            else
            {
                Title = "Menu Utama";
            }
                      
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
                    Ultis.Settings.MenuAction = "pending_receiving";
                    JobListTabbedPage pendingReceiving = new JobListTabbedPage();
                    pendingReceiving.previousPage = this;
                    await Navigation.PushAsync(pendingReceiving);
                    break;

                case "CargoLoad":
                    Ultis.Settings.MenuAction = "pending_loading";
                    JobListTabbedPage pendingLoading = new JobListTabbedPage();
                    pendingLoading.previousPage = this;
                    await Navigation.PushAsync(pendingLoading);
                    break;

                case "JobList":
                    Ultis.Settings.MenuAction = "Job_List";
                    //JobListTabbedPage jobList = new JobListTabbedPage();
                    //jobList.previousPage = this;
                    await Navigation.PushAsync(new JobList());
                    break;

                case "CargoReturn":
                    Ultis.Settings.MenuAction = "Cargo_Return";
                    FutileTrip_CargoReturn cargoReturn = new FutileTrip_CargoReturn();
                    cargoReturn.menuPreviousPage = this;
                    await Navigation.PushAsync(cargoReturn);
                    break;

                case "AppActivity":                   
                    AppActivity activity = new AppActivity();
                    activity.previousPage = this;
                    await Navigation.PushAsync(activity);
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
            }
       
            MainMenuList.SelectedItem = null;
        }

        public async void getMainMenu()
        {
            checkItems.Clear();
            try
            {
                activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;

                var client = new HttpClient();
                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                var uri = ControllerUtil.getDownloadMenuURL();
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);               
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                //clsResponse login_response = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL());


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
                                App.Database.SaveMenuAsync(existingRecord);

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

                                    summaryItem.Id = mainMenu.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = "MainMenu";
                                    summaryItem.BackColor = mainMenu.BackColor;
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

                    /*AppMenu history = new AppMenu();
                    history.menuId = "AppActivity";
                    App.Database.SaveMenuAsync(history);

                    SummaryItems summary  = new SummaryItems();
                    summary.Id = "AppActivity";
                    summary.Caption = "";
                    if (Ultis.Settings.Language.Equals("English"))
                    {
                        summary.Value = "Activity";
                    }
                    else
                    {
                        summary.Value = "Aktiviti";
                    }                   
                    summary.Display = true;
                    summary.Type = "MainMenu";
                    App.Database.SaveSummarysAsync(summary);*/

                    loadMainMenu();

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
            //Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(this); }).Wait();
            Task.Run(async () => { await BackgroundTask.UploadLatestRecord(); }).Wait();            
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
    }
}