using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class NewCategoryPage : ContentPage
    {
        ObservableCollection<string> categories = new ObservableCollection<string>();
        ObservableCollection<string> filterCategories = new ObservableCollection<string>();
        List<ListItems> items;
        List<clsDataRow> records;
        ListItems haulier;
        string callURL;
        bool multipleFilterCheck = false, savedCheck = true;

        public NewCategoryPage(ListItems item, string URL)
        {
            try
            {
                InitializeComponent();
                haulier = item;
                callURL = URL;

                GetContainerHeader(URL, item);

                Title = item.Name + " Summary";

                MessagingCenter.Subscribe<App>((App)Application.Current, "RefreshCategory", (sender) =>
                {
                    GetContainerHeader(URL, item);
                });

                
            }
            catch (Exception ex)
            {

            }
        }

        void ClearEntry(object sender, System.EventArgs e)
        {
            shipperConsignee.Text = "";
            cancel.IsVisible = false;
        }

        private void CheckBox_StateChanged(object sender, StateChangedEventArgs e)
        {
            string checkedAction = "";

            var checkBox = sender as SfCheckBox;

            if (savedCheck == false)
            {

                switch (checkBox.StyleId)
                {
                    case "Export":
                        checkedAction = "E";
                        if (Export.IsChecked == true)
                        {
                            Ultis.Settings.ExportCheck = "E";
                        }
                        else
                        {
                            Ultis.Settings.ExportCheck = "false";
                        }
                        break;

                    case "Import":
                        checkedAction = "I";
                        if (Import.IsChecked == true)
                        {
                            Ultis.Settings.ImportCheck = "I";
                        }
                        else
                        {
                            Ultis.Settings.ImportCheck = "false";
                        }
                        break;

                    case "Local":
                        checkedAction = "L";
                        if (Local.IsChecked == true)
                        {
                            Ultis.Settings.LocalCheck = "L";
                        }
                        else
                        {
                            Ultis.Settings.LocalCheck = "false";
                        }
                        break;
                }

                if ((Export.IsChecked == false || Import.IsChecked == false || Local.IsChecked == false) && multipleFilterCheck == true)
                {
                    //items = null;
                    foreach (ListItems item in items.ToList())
                    {
                        if (item.Action == checkedAction)
                        {
                            items.Remove(item);
                        }
                    }

                    if (items.Count == 0)
                    {
                        items.Clear();
                    }

                    multipleFilterCheck = false;
                }

                if ((Export.IsChecked == false && Import.IsChecked == false && Local.IsChecked == false) ||
                    (Export.IsChecked == true && Import.IsChecked == true && Local.IsChecked == true))
                {

                    listView.ItemsSource = categories;
                    items.Clear();
                }
                else
                {
                    filterCategories.Clear();

                    List<ListItems> all = new List<ListItems>(App.Database.GetAllContainerDetail());

                    if (items == null || items.Count == 0)
                    {
                        items = all.Where(x => x.Action == checkedAction).ToList();
                    }
                    else
                    {
                        if (checkBox.IsChecked != false)
                        {
                            List<ListItems> secondFilter = all.Where(x => x.Action == checkedAction).ToList();

                            foreach (ListItems listItem in secondFilter)
                            {
                                items.Add(listItem);
                            }
                        }
                        multipleFilterCheck = true;
                    }

                    foreach (string category in categories)
                    {
                        string[] splitCategory = category.Split(" (");
                        int count = 0;
                        foreach (ListItems item in items)
                        {
                            if (item.Category == splitCategory[0])
                            {
                                count++;
                            }
                        }

                        filterCategories.Add(splitCategory[0] + " (" + count + ")");
                    }

                    listView.ItemsSource = filterCategories;
                }
            }
        }

        async void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            cancel.IsVisible = true;

            try
            {
                string searchKey = shipperConsignee.Text;

                if (string.IsNullOrEmpty(searchKey))
                {
                    items.Clear();

                    if ((Export.IsChecked == false && Import.IsChecked == false && Local.IsChecked == false) ||
                    (Export.IsChecked == true && Import.IsChecked == true && Local.IsChecked == true))
                    {
                        listView.ItemsSource = categories;
                    }
                    else
                    {
                        if (Export.IsChecked == true)
                        {
                            checkContainerList("E", Export);
                        }
                        if (Import.IsChecked == true)
                        {
                            checkContainerList("I", Import);
                        }
                        if (Local.IsChecked == true)
                        {
                            checkContainerList("L", Local);
                        }
                    }
                }
                else
                {
                    filterCategories.Clear();

                    List<ListItems> all = new List<ListItems>(App.Database.GetAllContainerDetail());

                    items = all.Where(x => x.Summary.Contains(searchKey)).ToList();

                    foreach (string category in categories)
                    {
                        string[] splitCategory = category.Split(" (");
                        int count = 0;
                        foreach (ListItems item in items)
                        {

                            if (item.Category == splitCategory[0])
                            {
                                count++;
                            }
                        }

                        filterCategories.Add(splitCategory[0] + " (" + count + ")");
                    }

                    listView.ItemsSource = filterCategories;
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }
        }

        async void GetContainerHeader(string url, ListItems item)
        {
            loading.IsVisible = true;

            try
            {
                categories.Clear();
                #region GetFullContainerList
                var container_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerFullListURL(item.Id), this);

                if (container_content != null)
                {
                    clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(container_content);

                    App.Database.DeleteRecords();

                    if (container_response.IsGood)
                    {
                        //clsHaulageModel inherit clsDataRow
                        records = JObject.Parse(container_content)["Result"].ToObject<List<clsDataRow>>();

                        foreach (clsDataRow data in records)
                        {
                            ListItems record = new ListItems
                            {
                                Id = data.Id,
                                Background = (!(String.IsNullOrEmpty(data.BackColor))) ? data.BackColor : "#ffffff",
                                Category = data.Caption,
                                Name = item.Name,
                                Action = data.Action,
                                IsVisible = (data.Caption == "Pending Acknowledgement" || data.Caption == "Pending RFC") ? true : false
                            };

                            if (data.Caption == "Pending Acknowledgement")
                            {
                                record.ButtonName = "Confirm Receive";
                            }
                            else if (data.Caption == "Pending RFC")
                            {
                                record.ButtonName = "RFC";
                            }

                            //store summary of the record for search 
                            string summary = "";
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
                                        summary += summaryItem.Caption + " :  " + summaryItem.Value + "\r\n" + "\r\n";
                                    }
                                }
                                else if (summaryItem.Caption == "")
                                {
                                    if (!(String.IsNullOrEmpty(summaryItem.Value)))
                                    {
                                        summary += summaryItem.Value + "\r\n" + "\r\n";
                                    }
                                }
                            }
                            record.Summary = summary;
                            App.Database.SaveItemAsync(record);
                        }
                        #endregion

                        var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, url, this);
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                        foreach (string category in response.Result)
                        {
                            int count = 0;
                            foreach (clsDataRow record in records)
                            {
                                if (record.Caption == category)
                                {
                                    count++;
                                }
                            }

                            categories.Add(category + " (" + count + ")");
                        }

                        listView.ItemsSource = categories;

                        if (Ultis.Settings.ExportCheck == "E")
                        {
                            Export.IsChecked = true;
                            checkContainerList("E", Export);
                        }
                        if (Ultis.Settings.ImportCheck == "I")
                        {
                            Import.IsChecked = true;
                            checkContainerList("I", Import);
                        }
                        if (Ultis.Settings.LocalCheck == "L")
                        {
                            Local.IsChecked = true;
                            checkContainerList("L", Local);
                        }

                        savedCheck = false;
                    }
                }
            }
            catch (Exception ex)
            {

            }

            loading.IsVisible = false;
        }

        void checkContainerList(string checkedAction, SfCheckBox checkBox)
        {
            try
            {
                if ((Export.IsChecked == false && Import.IsChecked == false && Local.IsChecked == false) ||
                    (Export.IsChecked == true && Import.IsChecked == true && Local.IsChecked == true))
                {

                    listView.ItemsSource = categories;
                    items.Clear();
                }
                else
                {
                    filterCategories.Clear();

                    List<ListItems> all = new List<ListItems>(App.Database.GetAllContainerDetail());

                    if (items == null || items.Count == 0)
                    {
                        items = all.Where(x => x.Action == checkedAction).ToList();
                    }
                    else
                    {
                        if (checkBox.IsChecked != false)
                        {
                            List<ListItems> secondFilter = all.Where(x => x.Action == checkedAction).ToList();

                            foreach (ListItems listItem in secondFilter)
                            {
                                items.Add(listItem);
                            }
                        }
                        multipleFilterCheck = true;
                    }

                    foreach (string category in categories)
                    {
                        string[] splitCategory = category.Split(" (");
                        int count = 0;
                        foreach (ListItems item in items)
                        {
                            if (item.Category == splitCategory[0])
                            {
                                count++;
                            }
                        }

                        filterCategories.Add(splitCategory[0] + " (" + count + ")");
                    }

                    listView.ItemsSource = filterCategories;
                }
            }
            catch
            {

            }
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string[] split = e.Item.ToString().Split(" (");

            string selectedCategory = split[0];

            List<ListItems> categoryItem = null;

            if (items == null || items.Count == 0)
            {
                categoryItem = new List<ListItems>(App.Database.GetContainerDetail(selectedCategory));
            }
            else
            {
                categoryItem = items.Where(x => x.Category.Contains(selectedCategory)).ToList();
            }

            if (categoryItem.Count != 0)
            {
                await Navigation.PushAsync(new NewCategoryDetail(categoryItem, selectedCategory, haulier.Id));
            }
        }

        protected void Handle_Refreshing(object sender, EventArgs e)
        {
            GetContainerHeader(callURL, haulier);

            listView.IsRefreshing = false;
        }
    }
}
