using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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

        public ProviderDetails(string code, string provider, string category)
        {
            InitializeComponent();
            providerCode = code;
            categorycode = provider;
            Title = category;

            loading.IsRunning = true;
            loading.IsVisible = true;
            loading.IsEnabled = true;

            Device.BeginInvokeOnMainThread(async () =>
            {
                App.Database.deleteMainMenu();
                App.Database.deleteMenuItems("Container");

                await GetContainer();

            });

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
       

        protected async void refreshContainerList(object sender, EventArgs e)
        {
            App.Database.deleteMainMenu();
            App.Database.deleteMenuItems("Container");
       
            await GetContainer();

            container_list.IsRefreshing = false;
        }

        private async void SearchContainer(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                   loadContainerList();
                }

                else
                {
                    try
                    {
                        List<AppMenu> test = new List<AppMenu>(App.Database.GetMainMenuItems());
                        container_list.ItemsSource = test.Where(x => x.menuId.Contains(searchKey) || x.name.Contains(searchKey) || x.summary.Contains(searchKey));
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Please try again", "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again", "OK");
            }

        }

        public async Task LoadItems()
        {

            await GetContainer();
   
        }

        public void switchMap(object sender, EventArgs e)
        {
            
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
                    string summary = "";
                    bool firstLine = true;
                    AppMenu menu = new AppMenu();
                    menu.menuId = container.Id;
                    menu.category = "Container";

                    if(!(String.IsNullOrEmpty(container.BackColor)))
                    {
                        menu.background = container.BackColor;
                    }
                    else
                    {
                        menu.background = "#ffffff";
                    }

                    foreach (clsCaptionValue summaryList in container.Summary)
                    {
                    
                        if(firstLine)
                        {
                            summary += summaryList.Value + "\r\n" + "\r\n";
                            firstLine = false;
                        }
                        else
                        {
                            summary += summaryList.Caption + "  :  " + summaryList.Value + "\r\n" + "\r\n";
                        }

                        if (String.IsNullOrEmpty(summaryList.Caption) || summaryList.Caption == "")
                        {
                            menu.name = summaryList.Value;
                        }
                        if(summaryList.Caption.Equals("Booking"))
                        {
                            menu.booking = summaryList.Value;
                        }
                        if (summaryList.Caption.Equals("Customer Ref"))
                        {
                            menu.customerRef = summaryList.Value;
                        }

                    }
                    menu.summary = summary;

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
            List<AppMenu> Item = new List<AppMenu>(App.Database.GetMainMenuItems());
            container_list.ItemsSource = Item;
            container_list.HasUnevenRows = true;
   
            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;
        }
    }
}
