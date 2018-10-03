using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
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
    public partial class ProviderDetails : ContentPage
    {
        string providerCode, categorycode;
        ObservableCollection<AppMenu> containerList = new ObservableCollection<AppMenu>();

        public ProviderDetails(string code, string provider, string category)
        {
            InitializeComponent();
            providerCode = code;
            categorycode = provider;
            Title = category;
           
            loading.IsRunning = true;
            loading.IsVisible = true;
            loading.IsEnabled = true;

            App.Database.deleteMainMenu();
            App.Database.deleteMenuItems("Container");

            GetContainer();

        }


        protected async void refreshContainerList(object sender, EventArgs e)
        {
            App.Database.deleteMainMenu();
            App.Database.deleteMenuItems("Container");
       
            await GetContainer();

            container_list.IsRefreshing = false;
        }

        private void SearchContainer(object sender, TextChangedEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(e.NewTextValue))
                {
                    loadContainerList();
                }

                else
                {
                    List<AppMenu> test = new List<AppMenu>(App.Database.GetMainMenuItems());
                    container_list.ItemsSource = test.Where(x => x.menuId.Contains(e.NewTextValue) ||  x.name.Contains(e.NewTextValue));
                    container_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

                }
            }
            catch
            {
                DisplayAlert("Error", "Please try again", "OK");
            }

        }


        public async void loadData(object sender, EventArgs e)
        {
            loading.IsRunning = true;
            loading.IsVisible = true;
            loading.IsEnabled = true;
            await LoadItems();
        }

        public async Task LoadItems()
        {

            await GetContainer();
   
        }

        public async void selectContainer(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ContainerDetails(providerCode, ((AppMenu)e.Item).menuId));
        }

        public async Task GetContainer()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerList(providerCode, categorycode));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (container_response.IsGood)
            {
                 List<clsDataRow> containers = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
               
                foreach (clsDataRow container in containers)
                {
                    AppMenu menu = new AppMenu();
                    menu.menuId = container.Id;


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

                        if(String.IsNullOrEmpty(summaryItem.Caption) || summaryItem.Caption == "")
                        {
                            menu.name = summaryItem.Value;
                        }
                      
                    }

                    App.Database.SaveMenuAsync(menu);
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
            int TEST = Item.Count;
            container_list.ItemsSource = Item;
            container_list.HasUnevenRows = true;
            container_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;
        }
    }
}
