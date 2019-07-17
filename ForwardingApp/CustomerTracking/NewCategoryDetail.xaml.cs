using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class NewCategoryDetail : ContentPage
    {
        string haulierCode = "";
        List<ListItems> items = new List<ListItems>();

        public NewCategoryDetail(List<ListItems> sortedItem, string selectedCategory, string haulier)
        {
            InitializeComponent();

            Title = selectedCategory;

            haulierCode = haulier;

            loading.IsVisible = true;

            categoryDetail.ItemsSource = sortedItem;

            loading.IsVisible = false;

            items = sortedItem;

        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ContainerDetails(haulierCode, ((ListItems)e.Item).Id));
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as SfButton;
            StackLayout listViewItem = (StackLayout)button.Parent;
            Label label = (Label)listViewItem.Children[0];
            ListItems item = items.Find(x => x.Summary.Contains(label.Text));

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.updatePODURL(haulierCode, item.Id), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response.IsGood)
            {
                await DisplayAlert("Success", "Confirmed Delivery", "OK");

                items.RemoveAll(i => i.Id == item.Id);
                categoryDetail.ItemsSource = null;
                categoryDetail.ItemsSource = items;

                MessagingCenter.Send<App>((App)Application.Current, "RefreshCategory");
            }

        }
    }
}
