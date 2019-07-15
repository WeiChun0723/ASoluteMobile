using System;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class NewCategoryDetail : ContentPage
    {
        string haulierCode = "";

        public NewCategoryDetail(List<ListItems> items, string selectedCategory, string haulier)
        {
            InitializeComponent();
            
            Title = selectedCategory;

            haulierCode = haulier;

            loading.IsVisible = true;

            categoryDetail.ItemsSource = items;

            loading.IsVisible = false;
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ContainerDetails(haulierCode, ((ListItems)e.Item).Id));
        }
    }
}
