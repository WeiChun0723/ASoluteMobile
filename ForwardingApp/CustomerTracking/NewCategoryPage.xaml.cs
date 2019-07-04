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
        bool multipleFilterCheck = false;
        

        public NewCategoryPage(ListItems item, string URL)
        {
            InitializeComponent();
            haulier = item;
            callURL = URL;

            GetContainerHeader(URL, item);
            Title = item.Name + " Summary";
        }

        private void CheckBox_StateChanged(object sender, StateChangedEventArgs e)
        {
            string checkedAction = "";

            var checkBox = sender as SfCheckBox;

            switch (checkBox.StyleId)
            {
                case "Export":
                    checkedAction = "E";
                    break;

                case "Import":
                    checkedAction = "I";
                    break;

                case "Local":
                    checkedAction = "L";
                    break;
            }

            if((Export.IsChecked == false || Import.IsChecked == false || Local.IsChecked == false) && multipleFilterCheck == true)
            {
                items = null;
                multipleFilterCheck = false;
            }

            if ((Export.IsChecked == false && Import.IsChecked == false && Local.IsChecked == false) ||
                (Export.IsChecked == true && Import.IsChecked == true && Local.IsChecked == true))
            {
                listView.ItemsSource = categories;
                items = null;
            }
            else
            {
                filterCategories.Clear();

                List<ListItems> all = new List<ListItems>(App.Database.GetAllContainerDetail());

                if(items == null)
                {
                    items = all.Where(x => x.Action == checkedAction).ToList();
                }
                else
                {
                    List<ListItems> secondFilter = all.Where(x => x.Action == checkedAction).ToList();

                    foreach(ListItems listItem in secondFilter)
                    {
                        items.Add(listItem);
                    }

                    multipleFilterCheck = true;
                }

                foreach (string category in categories)
                {
                    string[] splitCategory = category.Split('(');
                    int count = 0;
                    foreach (ListItems item in items)
                    {
                        if (item.Category == splitCategory[0])
                        {
                            count++;
                        }
                    }

                    filterCategories.Add(splitCategory[0] + "( " + count + " )");
                }

                listView.ItemsSource = filterCategories;
            }
        }

        async void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            try
            {
                string searchKey = shipperConsignee.Text;

                if (string.IsNullOrEmpty(searchKey))
                {
                    listView.ItemsSource = categories;
                    items = null;
                }
                else
                {
                    filterCategories.Clear();

                    List<ListItems> all = new List<ListItems>(App.Database.GetAllContainerDetail());

                    items = all.Where(x => x.Summary.Contains(searchKey)).ToList();

                    foreach (string category in categories)
                    {
                        string[] splitCategory = category.Split('(');
                        int count = 0;
                        foreach (ListItems item in items)
                        {

                            if (item.Category == splitCategory[0])
                            {
                                count++;
                            }
                        }

                        filterCategories.Add(splitCategory[0] + "( " + count + " )");
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
                clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(container_content);

                App.Database.DeleteRecords();

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
                        Action = data.Action
                    };

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
                    App.Database.SaveMenuAsync(record);
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

                    categories.Add(category + "( " + count + " )");
                }

                listView.ItemsSource = categories;
            }
            catch
            {

            }

            loading.IsVisible = false;
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string[] split = e.Item.ToString().Split('(');

            string selectedCategory = split[0];

            List<ListItems> categoryItem = null;

            if (items == null)
            {
                categoryItem = new List<ListItems>(App.Database.GetContainerDetail(selectedCategory));
            }
            else
            {
                categoryItem = items.Where(x => x.Category.Contains(selectedCategory)).ToList();
            }

            if (categoryItem.Count != 0)
            {
                await Navigation.PushAsync(new NewCategoryDetail(categoryItem, selectedCategory));
            }
        }

        protected void Handle_Refreshing(object sender, EventArgs e)
        {
            GetContainerHeader(callURL, haulier);

            listView.IsRefreshing = false;


        }
    }
}
