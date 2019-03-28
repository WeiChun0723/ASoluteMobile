using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LogHistory : ContentPage
	{

        public string date, screenTitle;
        

        public LogHistory (string title)
		{
			InitializeComponent ();

            screenTitle = title;

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
                Title = "Log History";
            }
            else 
            {
                Title = "Buku Log";
            }
        }

        protected override async void OnAppearing()
        {
            if (NetworkCheck.IsInternet())
            {
                downloadLogHistory(logDate.Date.ToString("yyyy-MM-dd"));
            }
            else
            {
                refreshLogHistory();
                await DisplayAlert("Offline Mode", "Cant connect to server", "Ok");
            }
        }


        public void addNewLog(object sender, EventArgs e)
        {
            LogEntry addNewLog = new LogEntry("" ,screenTitle);
            addNewLog.previousPage = this;
            Navigation.PushAsync(addNewLog);
        }

        public void recordDate(object sender, DateChangedEventArgs e)
        {
            date = e.NewDate.ToString("yyyy-MM-dd");

            downloadLogHistory(date);
        }


        public async void downloadLogHistory(string date)
        {
            loading.IsVisible = true;
            if (Ultis.Settings.SessionSettingKey != null && Ultis.Settings.SessionSettingKey != "")
            {             
                try
                {
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);

                    var uri = "";
                    if (date == "")
                    {
                        uri = ControllerUtil.getLogHistoryURL(DateTime.Now.ToString("yyyy-MM-dd"));
                    }
                    else
                    {
                        uri = ControllerUtil.getLogHistoryURL(date);
                    }
                              
                    var response = await client.GetAsync(uri);
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(content);

                    clsResponse logHistory_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (logHistory_response.IsGood == true)
                    {
                        var logHistoryList = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();
                                          
                        App.Database.deleteLogHistory();
                        //App.Database.deleteLogSummary();
                        App.Database.deleteRecordSummary("Log");

                        foreach (clsDataRow log in logHistoryList)
                        {
                            Log existingRecord = App.Database.GetLogRecordAsync(log.Id);

                            if (existingRecord == null || (!(existingRecord != null )))
                            {
                                if (existingRecord == null)
                                {
                                    existingRecord = new Log();
                                }
                               
                                existingRecord.logId = log.Id;
                                App.Database.SaveLogAsync(existingRecord);

                                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(log.Id, "Log");

                                int index = 0;
                                foreach (clsCaptionValue summaryList in log.Summary)
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

                                    summaryItem.Id = log.Id;
                                    summaryItem.Caption = summaryList.Caption;
                                    summaryItem.Value = summaryList.Value;
                                    summaryItem.Display = summaryList.Display;
                                    summaryItem.Type = "Log";
                                    App.Database.SaveSummarysAsync(summaryItem);
                                    index++;
                                }

                            }
                          
                        }

                        refreshLogHistory();
                    }
                    else
                    {
                        await DisplayAlert("Error", logHistory_response.Message, "OK");
                    }

                }
                catch (HttpRequestException)
                {
                    await DisplayAlert("Unable to connect", "Please try again later", "Ok");
                }
                    catch (Exception exception)
                {
                    await DisplayAlert("Error", exception.Message, "Ok");
                }

            }

        }

        protected void logRefresh(object sender, EventArgs e)
        {
            downloadLogHistory(logDate.Date.ToString("yyyy-MM-dd"));    
            logHistory.IsRefreshing = false;
        }

        public async void selectLog(object sender, ItemTappedEventArgs e)
        {
            try
            {
                string logId = ((Log)e.Item).logId;
                //&& logDate.Date.ToString("dd-MMMM-yyyy") == DateTime.Now.Date.ToString("dd-MMMM-yyyy")
                if (logId != "")
                {
                    try
                    {
                        LogEntry existLog = new LogEntry(logId, screenTitle);
                        existLog.previousPage = this;
                        await Navigation.PushAsync(existLog);
                    }
                    catch (Exception exception)
                    {
                        await DisplayAlert("Error", exception.Message, "Ok");
                    }
                }
                else
                {
                    return;
                }
            }
            catch
            {

            }
           
        }

        public void refreshLogHistory()
        {

            Ultis.Settings.List = "Log_History";
            ObservableCollection<Log> Item = new ObservableCollection<Log>(App.Database.GetLogItems());           
            logHistory.ItemsSource = Item;
            logHistory.HasUnevenRows = true;
            if (Device.RuntimePlatform == Device.iOS)
            {
                logHistory.RowHeight = 150;
            }
            logHistory.Style = (Style)App.Current.Resources["recordListStyle"];         
            logHistory.ItemTemplate = new DataTemplate(typeof(CustomListViewCell));
            loading.IsVisible = false;

        }

    }
}