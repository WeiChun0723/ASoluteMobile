using ASolute.Mobile.Models;
using ASolute_Mobile.Data;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace ASolute_Mobile
{
	
	public partial class RefuelHistory : ContentPage
	{
        string screen_title;

        public RefuelHistory (string title )
		{
			InitializeComponent ();

            screen_title = title;

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

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Refuel";
            }
            else
            {
                Title = "Isi Minyak";
            }


        }

        protected override async void OnAppearing()
        {
            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });

            if (NetworkCheck.IsInternet()  )
            {
                getRefuelHistory();               
            }
            else
            {
                loadRefuelHistory();
                await DisplayAlert("Offline Mode", "Cant connect to server", "Ok");
            }
                
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        //download record and display at each row of list
        public async void getRefuelHistory()
        {
           try
          {
            activityIndicator.IsRunning = true;
            activityIndicator.IsVisible = true;
            var client = new HttpClient();
            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
            var uri = ControllerUtil.getDownloadRefuelHistoryURL();
            var response = await client.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);           
            clsResponse refuelHistory_response = JsonConvert.DeserializeObject<clsResponse>(content);
            //List<clsDataRow> refuelList = new List<clsDataRow>();
              
                if (refuelHistory_response.IsGood == true )
                {
                    var jsonDataList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
                 

                    App.Database.deleteRecordSummary("Refuel");
                    App.Database.deleteHistory();
                   
                    foreach (clsDataRow history in jsonDataList)
                    {
                        RefuelHistoryData existingRecord = App.Database.GetRecordAsync(history.Id);

                        if (existingRecord == null || (!(existingRecord != null && existingRecord.Done > 0)))
                        {
                            if (existingRecord == null)
                            {
                                existingRecord = new RefuelHistoryData();
                            }

                            existingRecord.recordId = history.Id;
                            App.Database.SaveHistoryAsync(existingRecord);

                            List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(history.Id,"Refuel");

                            int index = 0;
                            foreach (clsCaptionValue summaryList in history.Summary)
                            {
                                SummaryItems summaryItem = null;
                                if (index < existingSummaryItems.Capacity)
                                {
                                    summaryItem = existingSummaryItems.ElementAt(index);
                                }

                                if (summaryItem == null)
                                {
                                    summaryItem = new SummaryItems();
                                }

                                summaryItem.Id = history.Id;
                                summaryItem.Caption = summaryList.Caption;
                                summaryItem.Value = summaryList.Value;
                                summaryItem.Display = summaryList.Display;
                                summaryItem.Type = "Refuel";
                                App.Database.SaveSummarysAsync(summaryItem);
                                index++;
                            }

                            if (existingSummaryItems != null)
                            {
                                for (; index < existingSummaryItems.Count; index++)
                                {
                                    App.Database.DeleteSummaryItem(existingSummaryItems.ElementAt(index));
                                }
                            }

                        }                        
                    }

                    loadRefuelHistory();
                    activityIndicator.IsRunning = false;
                    activityIndicator.IsVisible = false;
                }
                else
                 {
                await DisplayAlert("Error", refuelHistory_response.Message, "OK");
                 }
            } 
          catch(HttpRequestException )
            {
                await DisplayAlert("Offline Mode", "Cant connect to server", "Ok");
            }
            catch(Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "OK");
            }

                                  
        } 

        public void addNewRecord(object sender, EventArgs e)
        {
            RefuelEntry addNewRecord = new RefuelEntry(screen_title);

            Navigation.PushAsync(addNewRecord);

        }

        protected void refreshRefuelHistory (object sender, EventArgs e)
        {
            getRefuelHistory();  
        }

        public void loadRefuelHistory()
        {
            ObservableCollection<RefuelHistoryData> recordItems = new ObservableCollection<RefuelHistoryData>(App.Database.GetRecordItems());
            Ultis.Settings.List = "refuel_List";
            refuel_history.ItemsSource = recordItems;
            refuel_history.HasUnevenRows = true;
            if (Device.RuntimePlatform == Device.iOS)
            {
                refuel_history.RowHeight = 180;
            }
            refuel_history.Style = (Style)App.Current.Resources["recordListStyle"];
            refuel_history.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));

            if (recordItems.Count == 0)
            {
                refuel_history.IsVisible = true;
                noData.IsVisible = true;
            }
            else
            {
                refuel_history.IsVisible = true;
                noData.IsVisible = false;
            }

            refuel_history.IsRefreshing = false;
        }
    }
}