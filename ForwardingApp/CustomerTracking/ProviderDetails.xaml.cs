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
        string providerCode, categorycode;
        List<clsCaptionValue> categories;

        public ProviderDetails(string code, string provider, string category)
        {
            InitializeComponent();
            providerCode = code;
            categorycode = category;
            Title = provider;
            GetContainer();
        }

        protected void refreshContainerList(object sender, EventArgs e)
        {
            GetContainer();
            container_list.IsRefreshing = false;
        }

        public async void selectContainer(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ContainerDetails(providerCode, ((AppMenu)e.Item).menuId));
        }

        public async void GetContainer()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerList(providerCode, categorycode));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (container_response.IsGood)
            {
                List<clsDataRow> containers = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

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
