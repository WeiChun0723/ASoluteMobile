using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.jcTestFolder
{
    public partial class MyFirstPage : ContentPage
    {
        List<ListItems> TestingLists = new List<ListItems>();
        string TestingUri1;
        ListItems items = new ListItems();
        string selectedDate;


        public MyFirstPage(ListItems TestingItem, string TestingUri)
        {
            InitializeComponent();

            //StackLayout stk = new StackLayout();
            //stk.HorizontalOptions = LayoutOptions.Center;
            //SfButton sf = new SfButton();
            //stk.Children.Add(sf);
            //this.Content = stk;

            
            //sf.Text = "Testing Button";
            //sf.IsCheckable = true;


            items = TestingItem;
            TestingUri1 = TestingUri;

            testingCallWebService(TestingItem.Id,TestingUri1);

        }

        async void testingCallWebService(string CategoryID, string Uri)
        {


            noData.IsVisible = false;

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, Uri, this);
            
            if (content!=null)
            {
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
               
                if (response.IsGood)
                {
                    var items = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
                    App.Database.deleteRecords(CategoryID);
                    App.Database.deleteRecordSummary(CategoryID);

                    

                    foreach (clsDataRow item in items)
                    {
                        ListItems mainMenuItems = new ListItems();
                        mainMenuItems.Category = CategoryID;
                        mainMenuItems.Id = item.Id;
                        mainMenuItems.Background = (!(String.IsNullOrEmpty(item.BackColor))) ? item.BackColor : "#ffffff";

                        string testingSummary = "";

                        foreach (clsCaptionValue summary in item.Summary)
                        {

                            SummaryItems SI = new SummaryItems();
                            SI.Id = item.Id;
                            SI.Caption = summary.Caption;
                            SI.Value = summary.Value;
                            SI.Display = summary.Display;
                            SI.Type = CategoryID;

                            App.Database.SaveSummarysAsync(SI);

                            if(summary.Display)
                            {
                                testingSummary += summary.Caption + ": " + summary.Value + "\n" + "\n";
                            }
                        }

                        mainMenuItems.Summary = testingSummary;
                        App.Database.SaveItemAsync(mainMenuItems);
                    }

                    Ultis.Settings.List = CategoryID;
                    ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(CategoryID));

                    if (Item.Count == 0)
                    {
                        noData.IsVisible = true;
                        listView.ItemsSource = null;
                    }
                    else
                    {
                        listView.ItemsSource = Item;

                    }

                    listView.IsRefreshing = false;
                    
                }
            }
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string logId = ((ListItems)e.Item).Id;
            if(logId!="")
            {
                await Navigation.PushAsync(new LogEntry(logId, "Log Entry"));
            }
            else
            {
                return;
            }
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            try
            {
                testingCallWebService(items.Id, ControllerUtil.getLogHistoryURL(selectedDate));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        void DateSelectedEvent(object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            try
            {
                selectedDate = e.NewDate.ToString("yyyy-MM-dd");
                testingCallWebService(items.Id, ControllerUtil.getLogHistoryURL(e.NewDate.ToString("yyyy-MM-dd")));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }


        //IconTapped can be used for few images, with Switch case
        async void IconTapped(object sender, EventArgs e)
        {
            //Load sender into the image
            var image = sender as Image;


            //image.StyleId equal to icon name
            switch(image.StyleId)
            {
                case "tapIcon":
                    await Navigation.PushAsync(new LogEntry("", "LogBook"));
                break;


            }
        }

    }
}
