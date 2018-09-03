﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.HaulageScreen;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class MainPage : MasterDetailPage
    {
        public static int selection;
        public MainPage()
        {
            InitializeComponent();
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
                   
                    if (item.Id.Equals("Panic"))
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
                                var client = new HttpClient();
                                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                                var uri = ControllerUtil.getPanicURL();
                                var response = await client.GetAsync(uri);                           
                                var content = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine(content);
                                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (json_response.IsGood == true)
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
                                    await DisplayAlert("",reply , "Okay");
                                }
                                else
                                {
                                    await DisplayAlert("", json_response.Message, "Okay");
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
                                var client = new HttpClient();
                                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                                var uri = ControllerUtil.getCallOperatorURL();
                                var response = await client.GetAsync(uri);
                                var content = await response.Content.ReadAsStringAsync();
                                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (json_response.IsGood == true)
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
                                    await DisplayAlert("", json_response.Message, "Okay");
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
                                var client = new HttpClient();
                                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);                                
                                var response = await client.GetAsync(uri);
                                var content = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine(content);
                                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);
                                if (json_response.IsGood == true)
                                {

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
                                    

                                    var context_client = new HttpClient();
                                    context_client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                                    var context_uri = ControllerUtil.getDownloadMenuURL();
                                    var context_response = await context_client.GetAsync(context_uri);
                                    var context_content = await context_response.Content.ReadAsStringAsync();

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
                                else
                                {
                                    await DisplayAlert("Failed", json_response.Message, "Okay");
                                }

                            }
                            catch (Exception exception)
                            {
                                await DisplayAlert("Error", exception.Message, "OK");
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
                                var client = new HttpClient();
                                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                                var uri = ControllerUtil.getLogOutURL();
                                var response = await client.GetAsync(uri);
                                var content = await response.Content.ReadAsStringAsync();
                                Debug.WriteLine(content);

                                clsResponse logoutResponse = JsonConvert.DeserializeObject<clsResponse>(content);

                                if (logoutResponse.IsGood == true)
                                {
                                    App.DropDatabase();
                                    App.Database.deleteLocationAutoComplete("Location");
                                    BackgroundTask.Logout(this);
                                    if (Device.RuntimePlatform == Device.Android || Device.RuntimePlatform == Device.iOS)
                                    {
                                        DependencyService.Get<CloseApp>().close_app();
                                    }
                                }
                            }
                            catch (Exception exception)
                            {
                                await DisplayAlert("Error", exception.Message, "OK");
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

      
    }
}
