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
    public partial class ProviderDetails : ContentPage
    {
        string providerCode;
        List<clsCaptionValue> categories;

        public ProviderDetails(string code, string provider)
        {
            InitializeComponent();
            providerCode = code;
            Title = provider;
            getCategory();
        }

        protected void refreshContainerList(object sender, EventArgs e)
        {
            GetContainer();
            container_list.IsRefreshing = false;
        }

        public void CategorySelected(object sender, SelectedPositionChangedEventArgs e)
        {
            GetContainer();
        }

        public async void GetContainer()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerList(providerCode, categories[categoryPicker.SelectedIndex].Caption));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (container_response.IsGood)
            {
                var containers = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deleteMainMenu();
                App.Database.deleteMenuItems("Container");

                foreach (clsDataRow container in containers)
                {
                    AppMenu menu = new AppMenu();
                    menu.menuId = container.Id;
                    App.Database.SaveMenuAsync(menu);

                    foreach (clsCaptionValue summaryList in container.Summary)
                    {
                        SummaryItems summaryItem = new SummaryItems();
                        summaryItem.Id = container.Id;
                        summaryItem.Caption = summaryList.Caption;
                        summaryItem.Value = summaryList.Value;
                        summaryItem.Display = summaryList.Display;
                        summaryItem.Type = "Container";
                        summaryItem.BackColor = container.BackColor;
                        App.Database.SaveSummarysAsync(summaryItem);
                    }
                }

                loadContainerList();
            }
            else
            {
                await DisplayAlert("JsonError", container_response.Message, "OK");
            }
        }

        public async void getCategory()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getCategoryList(providerCode));
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(provider_response.IsGood)
            {
                categoryPicker.Items.Clear();

                categories = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                foreach(clsCaptionValue category in categories)
                {
                    categoryPicker.Items.Add(category.Value);
                }
            }
            else
            {
                await DisplayAlert("JsonError", provider_response.Message, "OK");
            }
        }

        public void loadContainerList()
        {
            Ultis.Settings.ListType = "container_List";
            ObservableCollection<AppMenu> Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenuItems());
            container_list.ItemsSource = Item;
            container_list.HasUnevenRows = true;
            container_list.Style = (Style)App.Current.Resources["recordListStyle"];
            container_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
        }
    }
}
