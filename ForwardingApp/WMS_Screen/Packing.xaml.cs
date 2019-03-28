﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class Packing : ContentPage
    {
        List<clsDataRow> packing;
        ObservableCollection<ListItems> Item;
        bool tapped = true;

        public Packing(string screenTitle)
        {
            InitializeComponent();

            Title = screenTitle;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetPackingList();
        }

        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {

                    pickingList.ItemsSource = Item;
                }

                else
                {
                    try
                    {
                        pickingList.ItemsSource = Item.Where(x => x.Summary.Contains(searchKey) || x.Name.Contains(searchKey));
                    }
                    catch (Exception error)
                    {
                        await DisplayAlert("Error", error.Message, "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }

        }

        public async void GetPackingList()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getPackingList());
            clsResponse tallyInList_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyInList_response.IsGood)
            {
                packing = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deleteRecords("PackingList");
                App.Database.deleteRecordSummary("PackingList");
                foreach (clsDataRow data in packing)
                {
                    ListItems record = new ListItems
                    {
                        Id = data.Id,
                        Background = data.BackColor,
                        Category = "PackingList"
                    };

                    string summary = "";

                    int count = 0;
                    foreach (clsCaptionValue summaryItem in data.Summary)
                    {

                        count++;

                        if (!(String.IsNullOrEmpty(summaryItem.Caption)))
                        {
                            if (count == data.Summary.Count)
                            {
                                summary += summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine;
                            }
                            else
                            {
                                summary += summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine + System.Environment.NewLine;
                            }
                        }
                        if (summaryItem.Caption.Equals(""))
                        {
                            record.Name = summaryItem.Value;
                        }

                    }

                    record.Summary = summary;

                    App.Database.SaveMenuAsync(record);

                    foreach (clsCaptionValue summaryList in data.Summary)
                    {
                        SummaryItems summaryItem = new SummaryItems();

                        summaryItem.Id = data.Id;
                        summaryItem.Caption = summaryList.Caption;
                        summaryItem.Value = summaryList.Value;
                        summaryItem.Display = summaryList.Display;
                        summaryItem.Type = "PackingList";
                        summaryItem.BackColor = data.BackColor;
                        App.Database.SaveSummarysAsync(summaryItem);

                    }

                }

                loadPackingList();
            }
            else
            {
                await DisplayAlert("Error", tallyInList_response.Message, "OK");
            }

        }

        public async void BarCodeScan(object sender, EventArgs e)
        {

            if(tapped)
            {
                tapped = false;
                try
                {
                    var scanPage = new ZXingScannerPage();
                    await Navigation.PushAsync(scanPage);

                    scanPage.OnScanResult += (result) =>
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await Navigation.PopAsync();

                            filterText.Text = result.Text;

                        });
                    };
                }
                catch
                {

                }

                tapped = true;
            }

        }

        void loadPackingList()
        {
            Ultis.Settings.List = "Packing_List";
            Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu("PackingList"));
            pickingList.ItemsSource = Item;
            pickingList.HasUnevenRows = true;
            pickingList.Style = (Style)App.Current.Resources["recordListStyle"];
            pickingList.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (Item.Count == 0)
            {

                noData.IsVisible = true;

            }
            else
            {

                noData.IsVisible = false;
            }
            loading.IsVisible = false;
        }

        public async void SelectPicking(object sender, ItemTappedEventArgs e)
        {

             await Navigation.PushAsync(new PackingDetail(((ListItems)e.Item).Id));
            //await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPackingDetail(((AppMenu)e.Item).menuId)));

        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetPackingList();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
