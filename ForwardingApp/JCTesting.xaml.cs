using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class JCTesting : ContentPage
    {
        List<ListItems> JCLists = new List<ListItems>();
        string callUri;
        ListItems items = new ListItems();

        public JCTesting(ListItems JCitems, string JCcallUri)
        {
            InitializeComponent();

            items = JCitems;
            callUri = JCcallUri;

            CallWebServiceJC(items.Id,callUri);


        }

        //Category is for ListItems, Type is for summaryItem and detailItem

        async void CallWebServiceJC (string CategoryID ,string callUri)
        {

            //callUri is for retrieve data, Uri stands for Uniform Resource Identifier, Uri is more general than Url, Uri is like the name, Url is the address which can use to find you directly
            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, callUri, this);

            //Check whether the callWebService got return any data or not
            if (content!=null)
            {

            //Put the content into clsResponse
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);


            //Check if the response is storing the content retrieved above
                if(response.IsGood)
                {

             //Put the retrieved content into clsDataRow
                    var items= JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
                    int count = items.Count;


             //Delete databse everytime to avoid duplication
                    App.Database.deleteRecords(CategoryID);
                    App.Database.deleteRecordSummary(CategoryID);


             //Loop each row
             //Each item will have its own summary
                    foreach (clsDataRow item in items)
                    {

             //Initialize listItems
                        ListItems mainMenuItems = new ListItems();

                        mainMenuItems.Category = CategoryID;
                        mainMenuItems.Id = item.Id;
                        mainMenuItems.Background = (!(String.IsNullOrEmpty(item.BackColor))) ? item.BackColor : "#ffffff";
                        

                        string testSummary = "";
                        foreach (clsCaptionValue summary in item.Summary)
                        {
                            //Summary needed to be transform to SummaryItems then only can use
                            SummaryItems SI = new SummaryItems();
                            
                            SI.Id = item.Id;
                            SI.Caption = summary.Caption;
                            SI.Value = summary.Value;
                            SI.Display = summary.Display;
                            SI.Type = CategoryID;

                            App.Database.SaveSummarysAsync(SI);

                            if(summary.Display)
                            {
                                testSummary += summary.Caption + " :  " + summary.Value + "\r\n" + "\r\n";
                            }
                            

                        }

                        mainMenuItems.Summary = testSummary;
                        App.Database.SaveItemAsync(mainMenuItems);

                    }

                    //Set list with CategoryID as primary key
                    Ultis.Settings.List = CategoryID;

                    //Obeservable Collection is like the list, but it can change accordingly if the original data has been changed
                    //If use normal list, the changes of orginal data will not be chnaged and show to the user.
                    // To check how many item has been saved to my local database
                    ObservableCollection<ListItems> Item = new ObservableCollection<ListItems>(App.Database.GetMainMenu(CategoryID));

                    //ItemSource has been binded with Item.
                    listView.ItemsSource = Item;
                   
                    
                    //listView.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
                }
            }
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string logId = ((ListItems)e.Item).Id;
            if (logId != "")
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
            CallWebServiceJC(items.Id, callUri);
        }
    }

}
