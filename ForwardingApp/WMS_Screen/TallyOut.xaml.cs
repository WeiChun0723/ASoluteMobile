using System;
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
    public partial class TallyOut : ContentPage
    {

        List<clsDataRow> tallyOutList;
        ObservableCollection<ListItems> Item;
        bool tapped = true;

        public TallyOut(string screenTitle)
        {
            InitializeComponent();

            Title = screenTitle;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetTallyOutList();
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
                        pickingList.ItemsSource = Item.Where(x => x.summary.Contains(searchKey) || x.name.Contains(searchKey));
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

        public async void GetTallyOutList()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getTallyOutList());
            clsResponse tallyInList_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyInList_response.IsGood)
            {
                tallyOutList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deleteMainMenuItem("TallyOutList");
                App.Database.deleteMenuItems("TallyOutList");
                foreach (clsDataRow data in tallyOutList)
                {
                    ListItems record = new ListItems
                    {
                        menuId = data.Id,
                        background = data.BackColor,
                        category = "TallyOutList"
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
                            record.name = summaryItem.Value;
                        }

                    }

                    record.summary = summary;

                    App.Database.SaveMenuAsync(record);

                    foreach (clsCaptionValue summaryList in data.Summary)
                    {
                        SummaryItems summaryItem = new SummaryItems();

                        summaryItem.Id = data.Id;
                        summaryItem.Caption = summaryList.Caption;
                        summaryItem.Value = summaryList.Value;
                        summaryItem.Display = summaryList.Display;
                        summaryItem.Type = "TallyOutList";
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
            Ultis.Settings.List = "TallyOut_List";
            Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu("TallyOutList"));
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

            await Navigation.PushAsync(new TallyOutDetail(((ListItems)e.Item).menuId));

        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetTallyOutList();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
