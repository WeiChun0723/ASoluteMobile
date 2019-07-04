using System;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class NewCategoryDetail : ContentPage
    {
        public NewCategoryDetail(List<ListItems> items, string selectedCategory)
        {
            InitializeComponent();

            Title = selectedCategory;

            loading.IsVisible = true;

            categoryDetail.ItemsSource = items;

            loading.IsVisible = false;
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {

        }
    }
}
