﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Models;
using System.IO;
using ASolute_Mobile.Utils;
using System.Diagnostics;
using ASolute.Mobile.Models;
using Newtonsoft.Json.Linq;
using System.Linq;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.CommonScreen;

namespace ASolute_Mobile
{
    public partial class LoginPage : ContentPage
    {
        public static string crash = "Yes";
        List<AutoComplete> equipmentID;

        public LoginPage()
        {
            InitializeComponent();

            //hide the navigation bar
            NavigationPage.SetHasNavigationBar(this, false);

            //search for any company icon that stored in file and display it
            if(File.Exists(Ultis.Settings.GetAppLogoFileLocation())){
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());    
            }

            //decide the app login screen name
            Ultis.Settings.App = "Haulage";
            if (Ultis.Settings.App.Equals("Transport"))
            {
                AppLabel.Text = "ASolute Transport";
                //equipment.IsVisible = false;
            }
            else if(Ultis.Settings.App.Equals("Fleet"))
            {
                AppLabel.Text = "ASolute Fleet";
            }
            else if (Ultis.Settings.App.Equals("Fowarding"))
            {
                AppLabel.Text = "ASolute Fowarding";
            }
            else if (Ultis.Settings.App.Equals("Haulage"))
            {
                AppLabel.Text = "AILS Haulage V29";
                equipmentEntry.IsVisible = false;
                eqPicker.IsVisible = false;
            }

            usernameEntry.Text = Ultis.Settings.SessionUserId;

            // display the equipment that input by user before an load into list for user to choose when they type similar eq id (autocomplete)
            equipmentID = new List<AutoComplete>(App.Database.GetAutoCompleteValues("Equipment"));

            foreach(AutoComplete equipment in equipmentID)
            {
                eqPicker.Items.Add(equipment.Value);
            }

            List<string> Eqsuggestion = new List<string>();
            for (int i = 0; i < equipmentID.Count; i++)
            {
                Eqsuggestion.Add(equipmentID[i].Value);
            }

            //set username entry maximum to 10 chars
          usernameEntry.TextChanged += (sender, args) =>
            {
                string _text = usernameEntry.Text.ToUpper(); 

                usernameEntry.Text = _text;
            };

            usernameEntry.Completed += (s, e) =>
            {                
                passwordEntry.Focus();
            };

            passwordEntry.Completed += (s, e) =>
            {
                if(equipmentEntry.IsVisible && eqPicker.IsVisible)
                {
                    equipmentEntry.Text = "";
                    equipmentEntry.Focus();
                }
            };

            if (Ultis.Settings.GeneratedAppID == "")
            {
                GetAppID();
            }

            AppID.Text = "App ID : " + Ultis.Settings.GeneratedAppID;
            Ultis.Settings.CrashInLoginPage = crash;                     
        }

        protected override void OnAppearing()
        {
            organizationEntry.Text = Ultis.Settings.AppEnterpriseName;
        }
        

        public  void eqSelected(object sender, SelectedPositionChangedEventArgs e)
        {                        
            if(eqPicker.SelectedIndex != -1)
            {             
                equipmentEntry.Text = equipmentID[eqPicker.SelectedIndex].Value;
                eqPicker.SelectedIndex = -1;               
            }          
        }

        public async void Update_URL_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new ChangeEnterpriseName());            
        }

        public async void NewUser(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new HaulageScreen.Registration());
        }

        //get app id for crash report by auto generate
        public static void GetAppID()
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var stringChars = new char[4];
            var random = new Random();
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var randomString = new String(stringChars);
            Ultis.Settings.GeneratedAppID = randomString;                    
        }
       
        public async void Login_Clicked(object sender, System.EventArgs e)
        {
            this.activityIndicator.IsRunning = true;
            
            if (!string.IsNullOrEmpty(usernameEntry.Text) &&
                !string.IsNullOrEmpty(passwordEntry.Text))
            {            
                string encryptedUserId = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(usernameEntry.Text));
                string encryptedPassword = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(passwordEntry.Text));
               
                try
                {

                    var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getLoginURL(encryptedUserId, encryptedPassword));
                    clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (login_response.IsGood == true)
                    {
                        //save user equipment into db and which use to display it on list for user to choose (similar to auto complete)
                        Ultis.Settings.UpdatedRecord = "RefreshJobList";
                        Ultis.Settings.RefreshMenuItem = "Yes";
                        Ultis.Settings.SessionUserId = usernameEntry.Text;
                        Ultis.Settings.SessionPassword = passwordEntry.Text;

                        AutoComplete existingEquipment = App.Database.GetAutoCompleteValue(equipmentEntry.Text);

                          if(existingEquipment == null)
                          {
                              existingEquipment = new AutoComplete();
                          }

                          existingEquipment.Value = equipmentEntry.Text;
                          existingEquipment.Type = "Equipment";
                          App.Database.SaveAutoCompleteAsync(existingEquipment);

                        var login_user = JObject.Parse(content)["Result"].ToObject<clsLogin>(); 
                 
                        foreach (clsDataRow contextMenu in login_user.ContextMenu)
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

                        var userObject = JObject.Parse(content)["Result"].ToObject<UserItem>();
                        //Save user info to constant
                        Ultis.Settings.SessionUserItem = userObject;                       
                        Ultis.Settings.SessionUserId = usernameEntry.Text;
                        Ultis.Settings.SessionSettingKey = System.Net.WebUtility.UrlEncode(login_user.SessionId);

                        if (login_user.Language == 0)
                        {
                            Ultis.Settings.Language = "English";
                        }
                        else
                        {
                            Ultis.Settings.Language = "Malay";
                        }                        

                        if (login_user.GetLogo || !File.Exists(Ultis.Settings.GetAppLogoFileLocation(usernameEntry.Text)))
                        {
                            var client = new HttpClient();
                            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                            var downloadLogoURI = ControllerUtil.getDownloadLogoURL();
                            var logo = await client.GetAsync(downloadLogoURI);
                            var logo_byte = await logo.Content.ReadAsStringAsync();
                            clsResponse logo_response = JsonConvert.DeserializeObject<clsResponse>(logo_byte);

                            if (logo_response.IsGood)
                            {
                                if (logo_response.Result != null)
                                {
                                    byte[] bytes = Convert.FromBase64String(logo_response.Result);
                                    File.WriteAllBytes(Ultis.Settings.GetAppLogoFileLocation(), bytes);
                                    File.WriteAllBytes(Ultis.Settings.GetAppLogoFileLocation(usernameEntry.Text), bytes);

                                    try
                                    {
                                        var downloadLogoAckURI = ControllerUtil.getDownloadLogoAcknowledgementURL();
                                        var logoAckResp = await client.GetAsync(downloadLogoAckURI);
                                       
                                    }
                                    catch(Exception exception)
                                    {
                                        await DisplayAlert("Error", exception.Message, "OK");
                                    }                                                                       
                                }
                                else
                                {
                                    await DisplayAlert("Download Logo Error", "No image.", "Ok");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Download Logo Error", "No image.", "Ok");
                            }
                        }
                        else
                        {
                            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation(usernameEntry.Text)))
                            {
                                if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
                                {
                                    File.Delete(Ultis.Settings.GetAppLogoFileLocation());
                                }
                                File.Copy(Ultis.Settings.GetAppLogoFileLocation(usernameEntry.Text), Ultis.Settings.GetAppLogoFileLocation());
                            }
                        }

                        Application.Current.MainPage = new MainPage();
                       
                    }                                  
                    else
                    {
                        await DisplayAlert("Error", login_response.Message, "OK");
                        this.activityIndicator.IsRunning = false;
                    }                              
                }
                catch (HttpRequestException exception)
                {
                    await DisplayAlert("Unable to connect", exception.Message, "Ok");
                    Application.Current.MainPage = new LoginPage();

                }
                catch(Exception exception)
                {
                    await DisplayAlert("Unable to connect", exception.Message, "Ok");
                }
            }
            else
            {
                await DisplayAlert("Login Fail", "Please make sure all fields are complete", "Ok");
                this.activityIndicator.IsRunning = false;
            }

        }

    }   
}