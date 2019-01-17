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
    public partial class Picking : ContentPage
    {
        List<clsDataRow> picking;
        ObservableCollection<AppMenu> records = new ObservableCollection<AppMenu>();

        public Picking(string screenTitle)
        {
            InitializeComponent();

            Title = screenTitle;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetPickingList();
        }

        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                   
                    pickingList.ItemsSource = records;
                }

                else
                {
                    try
                    {
                      
                        pickingList.ItemsSource = records.Where(x => x.summary.Contains(searchKey) || x.name.Contains(searchKey));
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

        public async void GetPickingList()
        {
            loading.IsVisible = true;
            records.Clear();

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getPickingList());
            clsResponse tallyInList_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyInList_response.IsGood)
            {
                picking = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                foreach (clsDataRow data in picking)
                {
                    string summary = "";
                    AppMenu record = new AppMenu
                    {
                        menuId = data.Id

                    };

                    if (!(String.IsNullOrEmpty(data.BackColor)))
                    {
                        record.background = data.BackColor;
                    }
                    else
                    {
                        record.background = "#ffffff";
                    }

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
                                summary +=  summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine + System.Environment.NewLine;
                            }

                        }

                        if (summaryItem.Caption.Equals(""))
                        {
                            record.name =  summaryItem.Value;
                        }

                    }

                    record.summary = summary;
                    records.Add(record);
                }

                loadPickingList();
            }
            else
            {
                await DisplayAlert("Error", tallyInList_response.Message, "OK");
            }


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
                        await Navigation.PopAsync();

                        filterText.Text = result.Text;

                    });
                };
            }
            catch
            {

            }
        }

        void loadPickingList()
        {
            pickingList.ItemsSource = records;
            pickingList.HasUnevenRows = true;

            if (records.Count == 0)
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

            await Navigation.PushAsync(new PickingDetail(((AppMenu)e.Item).menuId, ((AppMenu)e.Item).summary, ((AppMenu)e.Item).name));

        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetPickingList();
            pullToRefresh.IsRefreshing = false;
        }



        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
