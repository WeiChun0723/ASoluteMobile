using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.SfChart.XForms;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContainerCategory : ContentPage
    {

        string providerCode;
        List<clsCaptionValue> categories;
        ObservableCollection<TrackingCategory> pieData = new ObservableCollection<TrackingCategory>();

        public ContainerCategory(string code, string provider)
        {
            InitializeComponent();
            providerCode = code;
            Title = provider + " Summary";
            getCategory();
        }

        public async void selectCategory(object sender, ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ProviderDetails(providerCode,((ListItems)e.Item).Id, ((ListItems)e.Item).Name));
            
        }

     
        protected void refreshCategoryList(object sender, EventArgs e)
        {
            getCategory();
            category_list.IsRefreshing = false;

        }

        public async void getCategory()
        {
            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getCategoryList(providerCode));
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (provider_response.IsGood)
            {
                categories = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                App.Database.deleteMainMenu();
                App.Database.deleteRecordSummary("Category");


                foreach (clsCaptionValue category in categories)
                {
                    ListItems menu = new ListItems();
                    menu.Id = category.Caption;
                    menu.Name = category.Value;
                    App.Database.SaveMenuAsync(menu);


                    SummaryItems summaryItem = new SummaryItems();
                    summaryItem.Id = category.Caption;
                    summaryItem.Caption = category.Caption;
                    summaryItem.Value = category.Value;
                    summaryItem.Display = category.Display;
                    summaryItem.Type = "Category";
                    summaryItem.BackColor = "";
                    App.Database.SaveSummarysAsync(summaryItem);

                   /* string quantity = new String(category.Value.Where(Char.IsDigit).ToArray());
                    string[] name = category.Value.Split('(');

                    TrackingCategory trackingCategory = new TrackingCategory();
                    trackingCategory.CategoryCode = category.Caption;
                    trackingCategory.Name = name[0];
                    trackingCategory.Amount = Convert.ToInt32(quantity);

                    pieData.Add(trackingCategory);*/
                }

               /* bar.ItemsSource = pieData;
                bar.XBindingPath = "Name";
                bar.YBindingPath = "Amount";


                activityIndicator.IsEnabled = false;
                activityIndicator.IsVisible = false;
                activityIndicator.IsRunning = false;*/

                 loadCateogoryList();
            }
            else
            {
                await DisplayAlert("JsonError", provider_response.Message, "OK");
            }
        }


        public async void select_Category(object sender, ChartSelectionChangingEventArgs e)
        {
            e.Cancel = true;

            if (e.SelectedDataPointIndex > -1)
            {
                int indicator = e.SelectedDataPointIndex;
                await Navigation.PushAsync(new ProviderDetails(providerCode, pieData[indicator].CategoryCode, pieData[indicator].Name));
            }


        }
         public void loadCateogoryList()
         {
             Ultis.Settings.List = "category_List";
             ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenuItems());
             category_list.ItemsSource = Item;
             category_list.HasUnevenRows = true;
             category_list.Style = (Style)App.Current.Resources["recordListStyle"];
             category_list.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

             activityIndicator.IsEnabled = false;
             activityIndicator.IsVisible = false;
             activityIndicator.IsRunning = false;
         }
    }
}
