using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ASolute_Mobile.WMS_Screen;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile
{
    public partial class ListViewTemplate : ContentPage
    {

        List<clsDataRow> dataList;
        ObservableCollection<ListItems> Item;
        string uri,name;

        public ListViewTemplate(string title, string callUri)
        {
            InitializeComponent();

            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text = title,
                TextColor = Color.White
            };

            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

            name = title;
            uri = callUri;
             
        }

        protected override  void OnAppearing()
        {
            base.OnAppearing();

            GetListData();
        }


         void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetListData();

        }


        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {

                    listView.ItemsSource = Item;
                }
                else
                {
                    try
                    {
                        listView.ItemsSource = Item.Where(x => x.summary.Contains(searchKey) || x.name.Contains(searchKey));
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

                        searchBar.Text = result.Text;

                    });
                };
            }
            catch
            {

            }
        }

        public async void SelectList(object sender, ItemTappedEventArgs e)
        {
            string type = name;

            switch(type)
            {
                case "Tally In":
                    //await Navigation.PushAsync(new TallyInDetail(((AppMenu)e.Item).menuId));
                   await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyInDetail(((ListItems)e.Item).menuId), ((ListItems)e.Item).menuId,type));
                    break;
                case "Packing":
                    //await Navigation.PushAsync(new PackingDetail(((AppMenu)e.Item).menuId));
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPackingDetail(((ListItems)e.Item).menuId), ((ListItems)e.Item).menuId,type));
                    break;
                case "Loose Pick":
                    //await Navigation.PushAsync(new PickingDetail(((AppMenu)e.Item).menuId, "LoosePick", name));
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetail(((ListItems)e.Item).menuId,"LoosePick") , ((ListItems)e.Item).menuId, type));
                    break;
                case "Full Pick":
                    //await Navigation.PushAsync(new PickingDetail(((AppMenu)e.Item).menuId, "FullPick", name));
                     await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetail(((ListItems)e.Item).menuId, "FullPick"), ((ListItems)e.Item).menuId, type));
                    break;
                case "Tally Out":
                    //await Navigation.PushAsync(new TallyOutDetail(((AppMenu)e.Item).menuId));
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyOutDetail(((ListItems)e.Item).menuId), ((ListItems)e.Item).menuId, type));
                    break;
            }
        }

        async void GetListData()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, uri,this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (response.IsGood)
            {
                dataList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                App.Database.deleteMainMenuItem(name);
                App.Database.deleteMenuItems(name);
                foreach (clsDataRow data in dataList)
                {
                    ListItems record = new ListItems
                    {
                        menuId = data.Id,
                        background = data.BackColor,
                        category = name,
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
                        summaryItem.Type = name;
                        summaryItem.BackColor = data.BackColor;
                        App.Database.SaveSummarysAsync(summaryItem);

                    }
                }
                loading.IsVisible = false;
                loadTallyInList();
            }
            else
            {
                await DisplayAlert("Error", response.Message, "OK");
            }
        }

        public void loadTallyInList()
        {
            Ultis.Settings.List = name;
            Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(name));
            listView.ItemsSource = Item;
            listView.HasUnevenRows = true;
            listView.Style = (Style)App.Current.Resources["recordListStyle"];
            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (Item.Count == 0)
            {
                noData.IsVisible = true;
            }
            else
            {
                noData.IsVisible = false;
            }
        }
    }
}
