using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class MyProviders : ContentPage
    {
        public MyProviders()
        {
            InitializeComponent();
            Title = "Home";

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
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = true
            });

        }


        protected async override void OnAppearing()
        {
            if (Ultis.Settings.AppFirstInstall == "First")
            {
                StartListening();
                Ultis.Settings.AppFirstInstall = "Second";
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getAutoScan());
                clsResponse autoScan_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(autoScan_response.IsGood)
                {
                    await DisplayAlert("Succeed", autoScan_response.Result, "OK");
                    getProviderList();
                }
                else
                {
                    await DisplayAlert("Error", autoScan_response.Message, "OK");
                }
            }
            else
            {
                getProviderList();
            }
                
        }

        public async void selectProvider(object sender, ItemTappedEventArgs e)
        {
   
            await Navigation.PushAsync(new ContainerCategory(((AppMenu)e.Item).menuId,((AppMenu)e.Item).name));
        }

        protected void refreshProviderList(object sender, EventArgs e)
        {
            getProviderList();
            provide_list.IsRefreshing = false;

        }

        public async void getProviderList()
        {
        
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getProviderList());
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(provider_response.IsGood)
            {
                var providers = JObject.Parse(content)["Result"].ToObject<List<clsProvider>>();

                int test = providers.Count;

                App.Database.deleteMainMenu();
                App.Database.DeleteProvider();

                foreach(clsProvider p in providers)
                {

                    AppMenu menu = new AppMenu();
                    menu.menuId = p.Code;
                    menu.name = p.Name;
                    App.Database.SaveMenuAsync(menu);

                    ProviderInfo available_provider = new ProviderInfo();
                    available_provider.Code = p.Code;
                    available_provider.Name = p.Name;
                    available_provider.Url = p.Url;

                    App.Database.SaveProvider(available_provider);
                }

                loadProviderList();
            }
            else
            {
                await DisplayAlert("JsonError", provider_response.Message, "OK");
            }
        }

        public void loadProviderList()
        {
            Ultis.Settings.ListType = "provider_List";
            ObservableCollection<AppMenu> Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());
            provide_list.ItemsSource = Item;
            provide_list.HasUnevenRows = true;
            provide_list.Style = (Style)App.Current.Resources["recordListStyle"];
            provide_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            loading.IsEnabled = false;
            loading.IsVisible = false;
            loading.IsRunning = false;
        }
    }
}
