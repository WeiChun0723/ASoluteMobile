using System;
using System.Collections.Generic;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class TallyInList : ContentPage
    {

        List<clsWhsCommon> tallyInList;

        public TallyInList(string title)
        {
            InitializeComponent();

            Title = title;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            loading.IsVisible = true;

            GetTallyInList();
        }

        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                    dataGrid.AutoExpandGroups = false;
                    dataGrid.ItemsSource = tallyInList;
                }

                else
                {
                    try
                    {
                        dataGrid.AutoExpandGroups = true;
                        dataGrid.ItemsSource = tallyInList.Where(x => x.DocumentNo.Contains(searchKey) || x.ContainerNo.Contains(searchKey) || x.Principal.Contains(searchKey));
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Please try again", "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }

        }

        public async void GetTallyInList()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getTallyInList());
            clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (equipment_response.IsGood)
            {
                tallyInList = JObject.Parse(content)["Result"].ToObject<List<clsWhsCommon>>();

                dataGrid.ItemsSource = tallyInList;
            }

            loading.IsVisible = false;
        }


        public async void BarCodeScan(object sender, EventArgs e)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                await Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PushAsync(new TallyInDetail(result.Text));

                    });
                };
            }
            catch
            {

            }
        }

        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            clsWhsCommon tallyIn = new clsWhsCommon();
            tallyIn = ((clsWhsCommon)e.RowData);

            if (tallyIn != null)
            {
                await Navigation.PushAsync(new TallyInDetail(tallyIn.Id));
            }
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetTallyInList();
            pullToRefresh.IsRefreshing = false;
        }

       

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
