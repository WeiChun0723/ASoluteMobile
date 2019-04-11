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

            Device.BeginInvokeOnMainThread((Action)(async () =>
            {
                App.Database.deleteMainMenu();
                App.Database.deleteRecordSummary((string)"Container");

                await GetContainer();

            }));

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();


        }
       

        protected async void refreshContainerList(object sender, EventArgs e)
        {
            App.Database.deleteMainMenu();

       
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
                        List<ListItems> test = new List<ListItems>(App.Database.GetMainMenuItems());
                        container_list.ItemsSource = test.Where(x => x.Id.Contains(searchKey) || x.Name.Contains(searchKey) || x.Summary.Contains(searchKey));

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

            await Navigation.PushAsync(new ContainerDetails(providerCode, ((ListItems)e.Item).Id));
        }

        public async Task GetContainer()
        {
            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerListURL(providerCode, categorycode));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (container_response.IsGood)
            {
                 List<clsDataRow> containers = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
               
                foreach (clsDataRow container in containers)
                {
                    string summary = "";
                    bool firstLine = true;
                    ListItems menu = new ListItems();
                    menu.Id = container.Id;
                    menu.Category = "Container";

                    if(!(String.IsNullOrEmpty(container.BackColor)))
                    {
                        menu.Background = container.BackColor;
                    }
                    else
                    {
                        menu.Background = "#ffffff";
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
                            menu.Name = summaryList.Value;
                        }
                        if(summaryList.Caption.Equals("Booking"))
                        {
                            menu.Booking = summaryList.Value;
                        }
                        if (summaryList.Caption.Equals("Customer Ref"))
                        {
                            menu.CustomerRef = summaryList.Value;
                        }

                    }
                    menu.Summary = summary;

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
            Ultis.Settings.List= "container_List";
            List<ListItems> Item = new List<ListItems>(App.Database.GetMainMenuItems());
            container_list.ItemsSource = Item;
            container_list.HasUnevenRows = true;
   
            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;
        }
    }
}
