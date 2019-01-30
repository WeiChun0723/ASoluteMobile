using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class TallyInList : ContentPage
    {

        List<clsDataRow> tallyInList;
        ObservableCollection<AppMenu> Item;

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
                    //dataGrid.AutoExpandGroups = false;
                    //dataGrid.ItemsSource = tallyInList;
                    tallyList.ItemsSource = Item;
                }

                else
                {
                    try
                    {
                        // dataGrid.AutoExpandGroups = true;

                        // dataGrid.ItemsSource = tallyInList.Where(x => x.DocumentNo.Contains(searchKey) || x.ContainerNo.Contains(searchKey) || x.Principal.Contains(searchKey));

                        tallyList.ItemsSource = Item.Where(x => x.name.Contains(searchKey) || x.booking.Contains(searchKey) || x.customerRef.Contains(searchKey));
                    }
                    catch(Exception error)
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

        public async void GetTallyInList()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getTallyInList());
            clsResponse tallyInList_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyInList_response.IsGood)
            {
                tallyInList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deleteMainMenuItem("TallyInList");
                App.Database.deleteMenuItems("TallyInList");
                foreach (clsDataRow data in tallyInList)
                {
                        AppMenu record = new AppMenu
                        {
                            menuId = data.Id,
                            background = data.BackColor,
                            category = "TallyInList"
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
                            summaryItem.Type = "TallyInList";
                            summaryItem.BackColor = data.BackColor;
                            App.Database.SaveSummarysAsync(summaryItem);

                        }
                }
                loading.IsVisible = false;
                loadTallyInList();
            }
            else
            {
                await DisplayAlert("Error", tallyInList_response.Message, "OK");
            }


        }

        public void loadTallyInList()
        {
            Ultis.Settings.List = "TallyIn_List";
            Item = new ObservableCollection<AppMenu>(App.Database.GetMainMenu("TallyInList"));
            tallyList.ItemsSource = Item;
            tallyList.HasUnevenRows = true;
            tallyList.Style = (Style)App.Current.Resources["recordListStyle"];
            tallyList.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (Item.Count == 0)
            {

                noData.IsVisible = true;

            }
            else
            {

                noData.IsVisible = false;
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

                        /*string recordID = "";
                        foreach(clsWhsCommon tallyIn in tallyInList)
                        {
                            if(tallyIn.DocumentNo.Equals(result.Text))
                            {
                                recordID = tallyIn.Id;
                            }
                        }

                        if(recordID != "")
                        {
                            await Navigation.PushAsync(new TallyInDetail(recordID));
                        }
                        else
                        {
                            await DisplayAlert("Not found", "No such id.", "OK");
                        }*/

                    });
                };
            }
            catch
            {

            }
        }



        public async void selectTallyIn(object sender, ItemTappedEventArgs e)
        {

            await Navigation.PushAsync(new TallyInDetail(((AppMenu)e.Item).menuId));
 
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
