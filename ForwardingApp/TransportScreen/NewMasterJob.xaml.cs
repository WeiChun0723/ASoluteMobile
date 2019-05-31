using System;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class NewMasterJob : ContentPage
    {
        public NewMasterJob(ListItems item)
        {
            InitializeComponent();

            Title = item.Name;
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new NewMasterJobPopUp());
        }
    }
}
