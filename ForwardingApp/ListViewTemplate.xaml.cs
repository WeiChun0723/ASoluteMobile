using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.HaulageScreen;
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

        ListItems menuItems;
        ObservableCollection<ListItems> Item;
        string uri;

        public ListViewTemplate(ListItems items, string callUri)
        {
            InitializeComponent();

            StackLayout main = new StackLayout();

            if (callUri.Contains("FuelCost"))
            {
                icon.Source = "refuel.png";
                icon.WidthRequest = 70;
                icon.HeightRequest = 70;
            }

            Label title1 = new Label
            {
                FontSize = 15,
                Text = items.Name,
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

            menuItems = items;
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
                        listView.ItemsSource = Item.Where(x => x.Summary.Contains(searchKey));
                    }
                    catch 
                    {
                        //await DisplayAlert("Error", error.Message, "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }
        }

        public async void IconTapped(object sender, EventArgs e)
        {
            try
            {
                if(uri.Contains("FuelCost"))
                {
                    await Navigation.PushAsync(new RefuelEntry(menuItems.Name));
                }
                else
                {
                    var scanPage = new ZXingScannerPage();
                    await Navigation.PushAsync(scanPage);

                    scanPage.OnScanResult += (result) =>
                    {
                        if (menuItems.Id == "JobList")
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                scanPage.PauseAnalysis();

                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.addJobURL(result.Text), this);
                                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (response.IsGood)
                                {
                                    Ultis.Settings.RefreshMenuItem = "Yes";
                                    Ultis.Settings.UpdatedRecord = "RefreshJobList";
                                    GetListData();
                                    displayToast("Job added to job list.");
                                    scanPage.ResumeAnalysis();
                                }
                                else
                                {
                                    var answer = await DisplayAlert("Error", response.Message, "OK", "Cancel");
                                    if (answer.Equals(true))
                                    {
                                        scanPage.ResumeAnalysis();
                                    }
                                    else
                                    {
                                        await Navigation.PopAsync();
                                    }
                                }
                            });
                        }
                        else
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await Navigation.PopAsync();

                                searchBar.Text = result.Text;

                            });

                        }

                    };
                }

            }
            catch
            {

            }
        }

        public void displayToast(string message)
        {
            var toastConfig = new ToastConfig(message);
            toastConfig.SetDuration(2000);
            toastConfig.Position = 0;
            toastConfig.SetMessageTextColor(System.Drawing.Color.FromArgb(0, 0, 0));
            if (message == "Job added to job list.")
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(50, 205, 50));
            }

            UserDialogs.Instance.Toast(toastConfig);
        }

        public async void SelectList(object sender, ItemTappedEventArgs e)
        {
            try
            {
                string type = menuItems.Id;

                switch (type)
                {
                    case "TallyIn":
                        //await Navigation.PushAsync(new TallyInDetail(((AppMenu)e.Item).menuId));
                        await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyInDetail(((ListItems)e.Item).Id), ((ListItems)e.Item).Id, type));
                        break;
                    case "Packing":
                        //await Navigation.PushAsync(new PackingDetail(((AppMenu)e.Item).menuId));
                        await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPackingDetail(((ListItems)e.Item).Id), ((ListItems)e.Item).Id, type));
                        break;
                    case "LoosePick":
                        //await Navigation.PushAsync(new PickingDetail(((AppMenu)e.Item).menuId, "LoosePick", name));
                        await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetail(((ListItems)e.Item).Id, "LoosePick"), ((ListItems)e.Item).Id, type));
                        break;
                    case "FullPick":
                        //await Navigation.PushAsync(new PickingDetail(((AppMenu)e.Item).menuId, "FullPick", name));
                        await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetail(((ListItems)e.Item).Id, "FullPick"), ((ListItems)e.Item).Id, type));
                        break;
                    case "TallyOut":
                        //await Navigation.PushAsync(new TallyOutDetail(((AppMenu)e.Item).menuId));
                        await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyOutDetail(((ListItems)e.Item).Id), ((ListItems)e.Item).Id, type));
                        break;
                    case "JobList":
                        Ultis.Settings.SessionCurrentJobId = ((ListItems)e.Item).Id;
                        /*Ultis.Settings.Action = ((ListItems)e.Item).ActionId;
                        await Navigation.PushAsync(new JobDetails(((ListItems)e.Item).ActionId, ((ListItems)e.Item).ActionMessage));*/
                        await Navigation.PushAsync(new NewJobDetails());
                        break;
                }
            }
            catch
            {

            }

        }

        async void GetListData()
        {
            try
            {
                loading.IsVisible = true;

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, uri, this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    App.Database.deleteRecords(menuItems.Id);
                    App.Database.deleteRecordSummary(menuItems.Id);
                    App.Database.deleteRecordDetails();

                    //clsHaulageModel inherit clsDataRow
                    var records = JObject.Parse(content)["Result"].ToObject<List<clsHaulageModel>>();

                    foreach (clsHaulageModel data in records)
                    {
                        Guid objectID = Guid.NewGuid();
                        ListItems record = new ListItems
                        {
                            Id = (menuItems.Id == "PendingCollection") ? objectID.ToString() : data.Id,
                            Background = data.BackColor,
                            Category = menuItems.Id,
                        };

                        if (menuItems.Id == "JobList")
                        {
                            record.TruckId = data.TruckId;
                            record.ReqSign = data.ReqSign;
                            record.Latitude = data.Latitude;
                            record.Longitude = data.Longitude;
                            record.TelNo = data.TelNo;
                            record.EventRecordId = data.EventRecordId;
                            record.TrailerId = data.TrailerId;
                            record.ContainerNo = data.ContainerNo;
                            record.MaxGrossWeight = data.MaxGrossWeight;
                            record.TareWeight = data.TareWeight;
                            record.CollectSeal = data.CollectSeal;
                            record.SealNo = data.SealNo;
                            record.ActionId = data.ActionId.ToString();
                            record.ActionMessage = data.ActionMessage;
                            record.Title = data.Title;
                            record.SealMode = data.SealMode;
                        }

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
                                summary += summaryItem.Value;
                            }
                        }

                        record.Summary = summary;

                        App.Database.SaveMenuAsync(record);

                        foreach (clsCaptionValue summaryList in data.Summary)
                        {
                            SummaryItems summaryItem = new SummaryItems();

                            summaryItem.Id = (menuItems.Id == "PendingCollection") ? objectID.ToString() : data.Id;
                            summaryItem.Caption = summaryList.Caption;
                            summaryItem.Value = summaryList.Value;
                            summaryItem.Display = summaryList.Display;
                            summaryItem.Type = menuItems.Id;
                            summaryItem.BackColor = data.BackColor;
                            App.Database.SaveSummarysAsync(summaryItem);

                        }

                        foreach (clsCaptionValue detailList in data.Details)
                        {
                            DetailItems detailItem = new DetailItems();

                            detailItem.Id = data.Id;
                            detailItem.Caption = detailList.Caption;
                            detailItem.Value = detailList.Value;
                            detailItem.Display = detailList.Display;
                            App.Database.SaveDetailsAsync(detailItem);
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
            catch
            {

            }
           
        }

      
        public void loadTallyInList()
        {
            Ultis.Settings.List = menuItems.Id;
            Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(menuItems.Id));
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

            listView.IsRefreshing = false;
        }
    }
}
