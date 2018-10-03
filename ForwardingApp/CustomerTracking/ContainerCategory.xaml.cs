using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContainerCategory : ContentPage
    {

        string providerCode;
        List<clsCaptionValue> categories;

        public ContainerCategory(string code, string provider)
        {
            InitializeComponent();
            providerCode = code;
            Title = provider + " Summary";
            getCategory();
        }

        public async void selectCategory(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ProviderDetails(providerCode,((AppMenu)e.Item).menuId, ((AppMenu)e.Item).name));
        }

      

        protected void refreshCategoryList(object sender, EventArgs e)
        {
            getCategory();
            category_list.IsRefreshing = false;

        }

        public async void getCategory()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getCategoryList(providerCode));
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (provider_response.IsGood)
            {
            
                categories = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                App.Database.deleteMainMenu();
                App.Database.deleteMenuItems("Category");


                foreach (clsCaptionValue category in categories)
                {
                    AppMenu menu = new AppMenu();
                    menu.menuId = category.Caption;
                    menu.name = category.Value;
                    App.Database.SaveMenuAsync(menu);


                    SummaryItems summaryItem = new SummaryItems();
                    summaryItem.Id = category.Caption;
                    summaryItem.Caption = category.Caption;
                    summaryItem.Value = category.Value;
                    summaryItem.Display = category.Display;
                    summaryItem.Type = "Category";
                    summaryItem.BackColor = "";
                    App.Database.SaveSummarysAsync(summaryItem);

                }

                loadCateogoryList();
            }
            else
            {
                await DisplayAlert("JsonError", provider_response.Message, "OK");
            }
        }

        public void loadCateogoryList()
        {
            Ultis.Settings.ListType = "category_List";
            ObservableCollection<AppMenu> Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());
            category_list.ItemsSource = Item;
            category_list.HasUnevenRows = true;
            category_list.Style = (Style)App.Current.Resources["recordListStyle"];
            category_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            loading.IsEnabled = false;
            loading.IsVisible = false;
            loading.IsRunning = false;
        }
    }
}
