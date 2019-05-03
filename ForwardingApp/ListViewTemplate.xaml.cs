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

        //use for AILS Yard container inquiry
        List<string> blocksID = new List<string>();
        List<string> dateOption = new List<string>
        {
            "All",
            "Closing Today",
            "Closing Tomorrow",
            "Closing After Tomorrow"
        };
        ListItems menuItems;
        ObservableCollection<ListItems> Item;
        List<clsHaulageModel> records;
        string uri;
        //record more than 100 store in this and popuplate to list view
        ObservableCollection<ListItems> overloadRecord = new ObservableCollection<ListItems>();

        public ListViewTemplate(ListItems items, string callUri)
        {
            InitializeComponent();

            //some of the list view require special control for filtering purpose
            switch (items.Id)
            {
                case "FuelCost":
                    icon.Source = "refuel.png";
                    break;

                //ASolute Fleet funtion control
                case "LogBook":
                    icon.Source = "truck.png";
                    logBookDate.IsVisible = true;
                    searchBar.IsVisible = false;
                    break;

                //AILS Yard function control
                case "PendingStorage":
                    icon.IsVisible = false;
                    break;
                case "ContainerInquiry":
                    icon.IsVisible = false;
                    searchBar.IsVisible = false;
                    blockComboBox.IsVisible = true;
                    dateComboBox.IsVisible = true;
                    break;
            }

            //define title for and subtitle for the screen
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

            //if the combo box is visible get value from webservice and populate using the control
            if (blockComboBox.IsVisible == true)
            {
                GetComboBoxData(ControllerUtil.getBlockList());
            }
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;
            GetListData();
        }

        //filter list by using search bar
        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                    listView.ItemsSource = (overloadRecord.Count == 0) ? Item : overloadRecord;
                }
                else
                {
                    listView.ItemsSource = (overloadRecord.Count == 0) ? Item.Where(x => x.Summary.Contains(searchKey)) : overloadRecord.Where(x => x.Summary.Contains(searchKey));
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }
        }

        //icon in screen and decide what to do by check the call uri
        async void IconTapped(object sender, EventArgs e)
        {
            loading.IsVisible = true;
            try
            {
                if (menuItems.Id == "FuelCost")
                {
                    await Navigation.PushAsync(new RefuelEntry(menuItems.Name));
                }
                else if (menuItems.Id == "LogBook")
                {
                    await Navigation.PushAsync(new LogEntry("", menuItems.Name));
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
                loading.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        //display scan result (success / error) in scan page
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

        //for fleet app log history only
        void Handle_DateSelected(object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            uri = ControllerUtil.getLogHistoryURL(e.NewDate.ToString("yyyy-MM-dd"));
            GetListData();
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
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
                case "Picking":
                    await Navigation.PushAsync(new WMS_DetailsPage(ControllerUtil.loadPickingDetailURL(((ListItems)e.Item).Id,"Picking"), ((ListItems)e.Item)));
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
                    await Navigation.PushAsync(new NewJobDetails());
                    break;
                case "PickingVerify":
                    await Navigation.PushAsync(new WMS_Screen.PalletMovement(((ListItems)e.Item)));
                    break;
                case "PendingStorage":
                    await PopupNavigation.Instance.PushAsync(new YardListPopUp(((ListItems)e.Item)));
                    break;
                case "LogBook":
                    string logId = ((ListItems)e.Item).Id;
                    if (logId != "")
                    {
                        await Navigation.PushAsync(new LogEntry(logId, menuItems.Name));
                    }
                    else
                    {
                        return;
                    }
                    break;
            }
        }

        async void GetListData()
        {
            try
            {
                loading.IsVisible = true;

                overloadRecord.Clear();
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
                            Background = (!(String.IsNullOrEmpty(data.BackColor))) ? data.BackColor : "#ffffff",
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

                        //store summary of the record for search 
                        string summary = "", closingTime = "";
                        int count = 0;
                        foreach (clsCaptionValue summaryItem in data.Summary)
                        {
                            count++;

                            if (!(String.IsNullOrEmpty(summaryItem.Caption)))
                            {
                                if (count == data.Summary.Count)
                                {
                                    summary += summaryItem.Caption + " :  " + summaryItem.Value;
                                }
                                else
                                {
                                    summary += summaryItem.Caption + " :  " + summaryItem.Value + System.Environment.NewLine + System.Environment.NewLine;
                                }
                            }

                            if (summaryItem.Caption == "")
                            {
                                summary += summaryItem.Value;
                            }
                            else if (summaryItem.Caption == "Closing Date")
                            {
                                closingTime = summaryItem.Value;
                            }
                        }
                        record.Summary = summary;
                        record.ClosingDate = (String.IsNullOrEmpty(closingTime)) ? DateTime.Now : Convert.ToDateTime(closingTime);

                        if (records.Count < 100)
                        {
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
                        else
                        {
                            overloadRecord.Add(record);
                        }
                    }

                    if (records.Count < 100)
                    {
                        LoadListData();
                    }
                    else
                    {
                        listView.ItemsSource = overloadRecord;
                        listView.RowHeight = 130;

                    }

                    listView.IsRefreshing = false;
                    loading.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        void LoadListData()
        {
            Ultis.Settings.List = menuItems.Id;
            Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(menuItems.Id));
            listView.ItemsSource = Item;
            listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
            noData.IsVisible = (Item.Count == 0) ? true : false;
        }

        //AILS Yrad container inquiry combo box data
        async void GetComboBoxData(string getBlockIdUri)
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, getBlockIdUri, this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    blocksID.Clear();

                    List<clsYardBlock> yardBlocks = JObject.Parse(content)["Result"].ToObject<List<clsYardBlock>>();

                    blocksID.Add("All");

                    foreach (clsYardBlock yardBlock in yardBlocks)
                    {
                        blocksID.Add(yardBlock.Id);
                    }

                    blockComboBox.ComboBoxSource = blocksID;
                    dateComboBox.ComboBoxSource = dateOption;
                }

                string test = "";

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        //AILS Yard filtering list by block id and closing date
        async void Handle_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {
            try
            {
                var filterOption = sender as Syncfusion.XForms.ComboBox.SfComboBox;

                switch (filterOption.StyleId)
                {
                    case "blockComboBox":
                        if (blockComboBox.Text == "All")
                        {
                            listView.ItemsSource = overloadRecord;
                        }
                        else
                        {
                            listView.ItemsSource = overloadRecord.Where(x => x.Summary.Contains(blockComboBox.Text + "-"));
                        }
                        break;

                    case "dateComboBox":
                        if (dateComboBox.Text == "All")
                        {
                            listView.ItemsSource = overloadRecord;
                        }
                        else
                        {
                            switch(dateComboBox.Text)
                            {
                                case "Closing Today":
                                    listView.ItemsSource = overloadRecord.Where(x => x.ClosingDate <= DateTime.Now.AddDays(-1) || x.ClosingDate.Value.Day == DateTime.Now.Day);
                                    break;

                                case "Closing Tomorrow":
                                    listView.ItemsSource = overloadRecord.Where(x => x.ClosingDate.Value.Day == DateTime.Now.AddDays(1).Day);
                                    break;

                                case "Closing After Tomorrow":
                                    listView.ItemsSource = overloadRecord.Where(x => x.ClosingDate >= DateTime.Now.AddDays(2));
                                    break;
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
