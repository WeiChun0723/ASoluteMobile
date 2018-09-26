using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class MyProviders : ContentPage
    {
        public MyProviders()
        {
            InitializeComponent();
            Title = "Home";
            getProviderList();
        }

        public async void selectProvider(object sender, ItemTappedEventArgs e)
        {
            string providerCode = ((AppMenu)e.Item).menuId;

            await Navigation.PushAsync(new ProviderDetails(((AppMenu)e.Item).menuId));
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
        }
    }
}
