﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.HaulageScreen;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using ASolute_Mobile.WMS_Screen;
using ASolute_Mobile.Yard;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile
{
    public partial class ListViewTemplate : ContentPage
    {

        ListItems menuItems;
        ObservableCollection<ListItems> Item;
        List<clsHaulageModel> records;
        string uri;

        public ListViewTemplate(ListItems items, string callUri)
        {
            InitializeComponent();

            if (callUri.Contains("FuelCost"))
            {
                icon.Source = "refuel.png";
                icon.WidthRequest = 70;
                icon.HeightRequest = 70;
            }

            StackLayout main = new StackLayout();

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

        protected override void OnAppearing()
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

                    /*if (menuItems.Id == "Yard")
                    {
                        listView.ItemsSource = records;
                    }
                    else
                    {

                    }*/

                }
                else
                {
                    try
                    {
                        listView.ItemsSource = Item.Where(x => x.Summary.Contains(searchKey));

                        /*if (menuItems.Id == "Yard")
                        {
                            listView.ItemsSource = records.Where(x => x.Caption.Contains(searchKey));
                        }
                        else
                        {

                        }*/
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
                if (uri.Contains("FuelCost"))
                {
                    await Navigation.PushAsync(new RefuelEntry(menuItems.Name));
                }
                else
                {
                    var scanPage = new ZXingScannerPage();
                    await Navigation.PushAsync(scanPage);

                    scanPage.OnScanResult += (result) =>
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            if (menuItems.Id == "JobList")
                            {
                                scanPage.PauseAnalysis();

                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.addJobURL(result.Text), this);
                                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (response.IsGood)
                                {
                                    Ultis.Settings.RefreshListView = "Yes";
                                    GetListData();
                                    displayToast("Job added to job list.");

                                }

                                scanPage.ResumeAnalysis();
                            }
                            else
                            {
                                await Navigation.PopAsync();

                                searchBar.Text = result.Text;
                            }

                        });
                    };
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
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

            string type = menuItems.Id;

            switch (type)
            {
                case "TallyIn":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyInDetailURL(((ListItems)e.Item).Id), ((ListItems)e.Item)));
                    break;
                case "Packing":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPackingDetailURL(((ListItems)e.Item).Id), ((ListItems)e.Item)));
                    break;
                case "LoosePick":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetailURL(((ListItems)e.Item).Id, "LoosePick"), ((ListItems)e.Item)));
                    break;
                case "FullPick":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetailURL(((ListItems)e.Item).Id, "FullPick"), ((ListItems)e.Item)));
                    break;
                case "TallyOut":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadTallyOutDetailURL(((ListItems)e.Item).Id), ((ListItems)e.Item)));
                    break;
                case "JobList":
                    Ultis.Settings.Title = ((ListItems)e.Item).Title;
                    Ultis.Settings.SessionCurrentJobId = ((ListItems)e.Item).Id;
                    /*Ultis.Settings.Action = ((ListItems)e.Item).ActionId;
                    await Navigation.PushAsync(new JobDetails(((ListItems)e.Item).ActionId, ((ListItems)e.Item).ActionMessage));*/
                    await Navigation.PushAsync(new NewJobDetails());
                    break;
                case "PickingVerify":
                    await Navigation.PushAsync(new WMS_Screen.PalletMovement(((ListItems)e.Item)));
                    break;
                case "PendingStorage":
                    await PopupNavigation.Instance.PushAsync(new YardListPopUp(((ListItems)e.Item)));
                    break;
            }
        }

        async void GetListData()
        {
            try
            {
                //loading.IsVisible = true;

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, uri, this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    App.Database.deleteRecords(menuItems.Id);
                    App.Database.deleteRecordSummary(menuItems.Id);
                    App.Database.deleteRecordDetails();

                    //clsHaulageModel inherit clsDataRow
                    records = JObject.Parse(content)["Result"].ToObject<List<clsHaulageModel>>();

                    foreach (clsHaulageModel data in records)
                    {
                        Guid objectID = Guid.NewGuid();
                        ListItems record = new ListItems
                        {
                            Id = (menuItems.Id == "PendingCollection") ? objectID.ToString() : data.Id,
                            Background = data.BackColor,
                            Category = menuItems.Id,
                            Name = menuItems.Name
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
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        public void loadTallyInList()
        {
            try
            {
                Ultis.Settings.List = menuItems.Id;
                Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(menuItems.Id));
                listView.ItemsSource = Item;
                listView.HasUnevenRows = true;
                listView.Style = (Style)App.Current.Resources["recordListStyle"];
                listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
                noData.IsVisible = (Item.Count == 0) ? true : false;
            

               /* if (menuItems.Id == "Yard")
                {
                    listView.ItemsSource = records;
                    listView.HasUnevenRows = true;
                    listView.Style = (Style)App.Current.Resources["recordListStyle"];
                }
                else
                {
                    Ultis.Settings.List = menuItems.Id;
                    Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(menuItems.Id));
                    listView.ItemsSource = Item;
                    listView.HasUnevenRows = true;
                    listView.Style = (Style)App.Current.Resources["recordListStyle"];
                    listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
                    noData.IsVisible = (Item.Count == 0) ? true : false;
                }*/

                listView.IsRefreshing = false;
            }
            catch
            {

            }
        }
    }
}