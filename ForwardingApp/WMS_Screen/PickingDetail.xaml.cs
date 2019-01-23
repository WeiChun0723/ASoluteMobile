﻿using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PickingDetail : ContentPage
    {

        string pickingID, linkID;
        clsWhsHeader pickingDetails;

        public PickingDetail(string id , string details, string pickingName)
        {
            InitializeComponent();

            pickingID = id;

            GetPickingDetails();
        }

        async void GetPickingDetails()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.loadPickingDetail(pickingID));
            clsResponse tallyIn_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyIn_response.IsGood)
            {
                pickingDetails = JObject.Parse(content)["Result"].ToObject<clsWhsHeader>();

                pickingDesc.Children.Clear();

                Label topBlank = new Label();
                pickingDesc.Children.Add(topBlank);

                foreach (clsCaptionValue desc in pickingDetails.Summary)
                {
                    Label caption = new Label();


                    if (desc.Caption.Equals(""))
                    {
                        caption.Text = "      " + desc.Value;
                        caption.FontAttributes = FontAttributes.Bold;
                        Title = "Picking # " + desc.Value;
                    }
                    else
                    {
                        caption.Text = "      " + desc.Caption + ": " + desc.Value;
                    }

                    if (desc.Caption.Equals(""))
                    {

                        Title = "Picking # " + desc.Value;
                    }

                    pickingDesc.Children.Add(caption);
                }

                Label bottomBlank = new Label();
                pickingDesc.Children.Add(bottomBlank);
               
                dataGrid.ItemsSource = pickingDetails.Items;

                linkID = pickingDetails.Id;
            }
            else
            {
                await DisplayAlert("Error", tallyIn_response.Message, "OK");
            }

            loading.IsVisible = false;
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetPickingDetails();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }

        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            clsWhsItem product = new clsWhsItem();
            product = ((clsWhsItem)e.RowData);

            if (product != null)
            {
                await Navigation.PushAsync(new PickingEntry(product,linkID));
            }
        }
    }
}