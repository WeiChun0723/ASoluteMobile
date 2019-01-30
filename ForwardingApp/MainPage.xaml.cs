using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
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
        //MasterPage masterPage = new MasterPage();

        public MainPage()
        {
            InitializeComponent();

        /* Master = masterPage;
            Detail = new NavigationPage(new MyProviders());*/

            masterPage.ListView.ItemSelected += OnItemSelected;
            MessagingCenter.Subscribe<object, string>(this, "JobSync", (s, e) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                   //Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(null); }).Wait();
                   // Task.Run(async () => { await BackgroundTask.UploadLatestRecord(); }).Wait();                  
                });
            });

          
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
                               
                                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getAutoScan());
                                clsResponse autoScan_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (autoScan_response.IsGood)
                                {
                                    await DisplayAlert("Success", autoScan_response.Result, "OK");
                                    Ultis.Settings.AppFirstInstall = "Refresh";
                                    Application.Current.MainPage = new MainPage();

                                }
                                else
                                {
                                    await DisplayAlert("Success", autoScan_response.Message, "OK");
                                }

                            }
                            catch
                            {
                                await DisplayAlert("Error", "Problem occur", "OK");
                            }
                        }
                       
                    }
                    else if(item.Id.Equals("Panic"))
                    {
                        string panic = "";
                        if (Ultis.Settings.Language.Equals("English"))
                        {
                            panic = "Send panic message ?";
                        }
                        else
                        {
                            panic = "Pasti ?";
                        }
                        var answer = await DisplayAlert("", panic, "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {
                                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getPanicURL());
                                clsResponse panic_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (panic_response.IsGood == true)
                                {
                                    string reply = "";
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        reply = "Message sent successfully.";
                                    }
                                    else
                                    {
                                        reply = "Permintaan anda telah dihantar.";
                                    }
                                    await DisplayAlert("",reply , "Okay");
                                }
                                else
                                {
                                    await DisplayAlert("Json Error", panic_response.Message, "Okay");
                                }
                            }
                            catch(Exception exception)
                            {
                                await DisplayAlert("Error", exception.Message, "OK");
                            }
                   
                        }
                    }
                    else if (item.Id.Equals("CallOffice"))
                    {
                        try
                        {
                            Device.OpenUri(new Uri(String.Format("tel:{0}", Ultis.Settings.SessionUserItem.OperationPhone)));
                        }
                        catch(Exception exception)
                        {
                            await DisplayAlert("Error", exception.Message, "OK");
                        }
                    }
                    else if (item.Id.Equals("CallMe"))
                    {
                        string callMe = "";
                        if (Ultis.Settings.Language.Equals("English"))
                        {
                            callMe = "Request controller to call you ?";
                        }
                        else
                        {
                            callMe = "Pasti ?";
                        }
                        var answer = await DisplayAlert("",callMe , "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {
                              
                                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getCallOperatorURL());
                                clsResponse callMe_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (callMe_response.IsGood == true)
                                {
                                    string reply = "";
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        reply = "Your request has been attended";
                                    }
                                    else
                                    {
                                        reply = "Permintaan anda telah dihantar.";
                                    }
                                    await DisplayAlert("", reply, "Okay");
                                }
                                else
                                {
                                    await DisplayAlert("Json_Error", callMe_response.Message, "Okay");
                                }

                            }
                            catch (Exception exception)
                            {
                                await DisplayAlert("Error", exception.Message, "OK");
                            }
                           
                        }
                    }
                    else if (item.Id.Equals("Language"))
                    {
                        string language = "";
                        if (Ultis.Settings.Language.Equals("English"))
                        {
                            language = "Please choose prefer language";
                        }
                        else
                        {
                            language = "Sila pilih bahasa ";
                        }
                        var answer = await DisplayActionSheet(language, "",null,"English", "Malay");
                        string uri = "";
                        if(answer != null)
                        {
                            if(answer.Equals("English"))
                            {
                                uri = ControllerUtil.getLanguageURL(0);
                                Ultis.Settings.Language = "English";
                            }
                            else if(answer.Equals("Malay") )
                            {
                                uri = ControllerUtil.getLanguageURL(1);
                                Ultis.Settings.Language = "Malay";
                            }

                            try
                            {

                                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, uri);
                                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (json_response.IsGood == true)
                                {
                                    Ultis.Settings.RefreshMenuItem = "Yes";
                                    string reply = "";
                                    if (Ultis.Settings.Language.Equals("English"))
                                    {
                                        reply = "Language had been changed.";
                                    }
                                    else
                                    {
                                        reply = "Bahasa telah diubah.";
                                    }
                               
                                    await DisplayAlert("", reply, "Okay");

                                    refreshMainPage();

                                }
                                else
                                {
                                    await DisplayAlert("Failed", json_response.Message, "Okay");
                                }

                            }
                            catch (Exception exception)
                            {
                                //await DisplayAlert("Error", exception.Message, "OK");
                            }
                        }
                        else
                        {

                        }
                    }                   
                    else if(item.Id.Equals("LogOff"))
                    {
                        string logoff = "";
                        if (Ultis.Settings.Language.Equals("English"))
                        {
                            logoff = "Are you sure?";
                        }
                        else
                        {
                            logoff = "Pasti ?";
                        }
                        var answer = await DisplayAlert("", logoff, "Yes", "No");
                        if (answer.Equals(true))
                        {
                            try
                            {
                              
                                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getLogOutURL());
                                clsResponse logoutResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (logoutResponse.IsGood == true)
                                {
                                    App.DropDatabase();
                                    App.Database.deleteLocationAutoComplete("Location");
                                    BackgroundTask.Logout(this);
                                }
                            }
                            catch (Exception)
                            {
                               // await DisplayAlert("Error", exception.Message, "OK");
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
            var context_content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(Ultis.Settings.FireID));
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
