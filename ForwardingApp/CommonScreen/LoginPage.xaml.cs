using System;
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
using Plugin.DeviceInfo;
using Plugin.Geolocator;
using Plugin.Media;

namespace ASolute_Mobile
{
    public partial class LoginPage : ContentPage
    {
        public static string crash = "Yes";

        public LoginPage()
        {
            InitializeComponent();
           
            //hide the navigation bar
            NavigationPage.SetHasNavigationBar(this, false);

            //search for any company icon that stored in file and display it
            if(File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());    
            }

            AppLabel.Text = "AILS Haulage";


            //set username entry maximum to 10 chars
            usernameEntry.TextChanged += (sender, args) =>
            {
                string _text = usernameEntry.Text.ToUpper(); 

                usernameEntry.Text = _text;
            };

            usernameEntry.Focus();
                
            usernameEntry.Completed += (s, e) =>
            {                
                passwordEntry.Focus();
            };

           
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            organizationEntry.Text = Ultis.Settings.AppEnterpriseName;
            usernameEntry.Text = Ultis.Settings.SessionUserId;
        }
      
        async void Update_URL_Clicked(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new SettingPage());            
        }

        async void NewUser(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new HaulageScreen.Registration());
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
                    var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getLoginURL(encryptedUserId, encryptedPassword));
                   //var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getFleetLoginURL(encryptedUserId, encryptedPassword,equipmentEntry.Text));
                   clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (login_response.IsGood == true)
                    {
                        //save user equipment into db and which use to display it on list for user to choose (similar to auto complete)
                        Ultis.Settings.UpdatedRecord = "RefreshJobList";
                        Ultis.Settings.RefreshMenuItem = "Yes";
                        Ultis.Settings.SessionUserId = usernameEntry.Text;
                        Ultis.Settings.SessionPassword = passwordEntry.Text;

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
                        Ultis.Settings.SessionSettingKey = System.Net.WebUtility.UrlEncode(login_user.SessionId);

                        if (login_user.Language == 0)
                        {
                            Ultis.Settings.Language = "English";
                        }
                        else
                        {
                            Ultis.Settings.Language = "Malay";
                        }

                        if(Ultis.Settings.Language.Equals("Malay"))
                        {
                            await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getLanguageURL(0));
                            Ultis.Settings.Language = "English";
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
               
            }
            this.activityIndicator.IsRunning = false;
        }

    }   
}
