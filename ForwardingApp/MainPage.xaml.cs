using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.Forwarding;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class MainPage : MasterDetailPage
    {
        public static int selection;
        MasterPage masterPage = new MasterPage();
        public MainPage()
        {
            InitializeComponent();

            Master = masterPage;

            switch(Ultis.Settings.App)
            {
                case "asolute.Mobile.AILSTracking":
                case "com.asolute.AILSTracking":
                    Detail = new CustomNavigationPage(new MyProviders());
                    break;

                case "asolute.Mobile.AILSBusiness":
                case "com.asolute.AILSBusiness":
                    Detail = new CustomNavigationPage(new BusinessChartTable());
                    break;

                case "asolute.Mobile.Forwarding":
                case "com.asolute.Forwarding":
                    Detail = new JobListTabbedPage();
                    MessagingCenter.Subscribe<object, string>(this, "JobSync", (s, e) =>
                    {
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Task.Run(async () => { await BackgroundTask.DownloadLatestJobs(null); }).Wait();
                            Task.Run(async () => { await BackgroundTask.UploadLatestJobs(); }).Wait();
                        });
                    });
                    break;


                default:
                    Detail = new CustomNavigationPage(new NewMainMenu());
                    break;
            }

            masterPage.ListView.ItemSelected += OnItemSelected;
        }

        async void OnItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            if (Ultis.Settings.SessionSettingKey == "")
            {
                BackgroundTask.Logout(this);
            }
            else
            {
                var item = e.SelectedItem as MasterPageItem;

                if (item != null)
                {
                    if (item.Id.Equals("AddProvider"))
                    {
                        var answer = await DisplayAlert("", "Refresh provider list?", "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {

                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getAutoScanURL(), this);
                                clsResponse autoScan_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (autoScan_response.IsGood)
                                {
                                    await DisplayAlert("Success", autoScan_response.Result, "OK");
                                    Ultis.Settings.AppFirstInstall = "Refresh";
                                    Application.Current.MainPage = new MainPage();
                                }
                            }
                            catch
                            {
                              
                            }
                        }

                    }
                    else if (item.Id.Equals("Panic"))
                    {
                        string panic = (Ultis.Settings.Language.Equals("English")) ? "Send panic message ?" : "Pasti ?";
                        var answer = await DisplayAlert("", panic, "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {
                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getPanicURL(), this);
                                clsResponse panic_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (panic_response.IsGood)
                                {
                                    string reply = (Ultis.Settings.Language.Equals("English")) ? "Message sent successfully." : "Permintaan anda telah dihantar.";
                                    await DisplayAlert("", reply, "Okay");
                                }
                            }
                            catch
                            {

                            }

                        }
                    }
                    else if (item.Id.Equals("CallOffice"))
                    {
                        try
                        {
                            Device.OpenUri(new Uri(String.Format("tel:{0}", Ultis.Settings.SessionUserItem.OperationPhone)));
                        }
                        catch 
                        {
                           
                        }
                    }
                    else if (item.Id.Equals("CallMe"))
                    {
                        string callMe = (Ultis.Settings.Language.Equals("English")) ? "Request controller to call you ?" : "Pasti ?";
                        var answer = await DisplayAlert("", callMe, "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {

                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCallOperatorURL(), this);
                                clsResponse callMe_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (callMe_response.IsGood == true)
                                {
                                    string reply = (Ultis.Settings.Language.Equals("English")) ? "Your request has been attended" : "Permintaan anda telah dihantar.";
                                    await DisplayAlert("", reply, "Okay");
                                }
                            }
                            catch 
                            {
                               
                            }

                        }
                    }
                    else if (item.Id.Equals("Language"))
                    {
                        string language = (Ultis.Settings.Language.Equals("English")) ? "Please choose prefer language" : "Sila pilih bahasa ";
                        var answer = await DisplayActionSheet(language, "", null, "English", "Malay");
                        string uri = "";
                        if (answer != null)
                        {
                            if (answer.Equals("English"))
                            {
                                uri = ControllerUtil.getLanguageURL(0);
                                Ultis.Settings.Language = "English";
                            }
                            else if (answer.Equals("Malay"))
                            {
                                uri = ControllerUtil.getLanguageURL(1);
                                Ultis.Settings.Language = "Malay";
                            }

                            try
                            {
                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, uri, this);
                                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (json_response.IsGood)
                                {
                                    Ultis.Settings.RefreshListView = "Yes";
                                    string reply = (Ultis.Settings.Language.Equals("English")) ? "Language had been changed." : "Bahasa telah diubah.";
                                    await DisplayAlert("", reply, "Okay");

                                    refreshMainPage();
                                }
                            }
                            catch 
                            {

                            }
                        }
                        else
                        {

                        }
                    }
                    else if (item.Id.Equals("LogOff"))
                    {
                        string logoff = (Ultis.Settings.Language.Equals("English")) ? "Are you sure?" : "Pasti ?";
                        var answer = await DisplayAlert("", logoff, "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {
                                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getLogOutURL(), this);
                                clsResponse logoutResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (logoutResponse.IsGood)
                                {

                                    //App.DropDatabase(); the app will crash
                                    BackgroundTask.Logout(this);
                                }
                            }
                            catch 
                            {

                            }
                        }
                    }
                    else
                    {
                        Detail = new CustomNavigationPage((Page)Activator.CreateInstance(item.TargetType));
                        masterPage.ListView.SelectedItem = null;
                        IsPresented = false;
                    }
                }
            }
        }

        public static explicit operator ContentPage(MainPage v)
        {
            throw new NotImplementedException();
        }

        public async void refreshMainPage()
        {
            var context_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(), this);
            clsResponse context_return = JsonConvert.DeserializeObject<clsResponse>(context_content);

            var contextMenuItems = JObject.Parse(context_content)["Result"].ToObject<clsLogin>();
            foreach (clsDataRow contextMenu in contextMenuItems.ContextMenu)
            {
                List<SummaryItems> existingSummaryItems = App.Database.GetSummarysAsync(contextMenu.Id, "ContextMenu");

                int index = 0;
                foreach (clsCaptionValue summaryList in contextMenu.Summary)
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

                    summaryItem.Id = contextMenu.Id;
                    summaryItem.Caption = summaryList.Caption;
                    summaryItem.Value = summaryList.Value;
                    summaryItem.Display = summaryList.Display;
                    summaryItem.Type = "ContextMenu";
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

            Application.Current.MainPage = new MainPage();
        }

    }
}
