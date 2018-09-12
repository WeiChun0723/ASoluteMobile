using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.HaulageScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Registration : ContentPage
	{
        bool success = false;
        public string encryptedUserId, encryptedPassword;


        public Registration ()
		{
			InitializeComponent ();

            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }

            NavigationPage.SetHasNavigationBar(this, false);

            enterpriseEntry.Completed += (s, e) =>
            {
                userIDEntry.Focus();
            };

            userIDEntry.Completed += (s, e) =>
            {
                passwordEntry.Focus();
            };

            passwordEntry.Completed += (s, e) =>
            {
                icEntry.Focus();
            };

            icEntry.Completed += (s, e) =>
            {
                icEntry.Unfocus();
            };
        }

        public void enterTextChange(object sender, TextChangedEventArgs e)
        {
            string _enterprisetext = enterpriseEntry.Text.ToUpper();

            enterpriseEntry.Text = _enterprisetext;
        }

        public void userTextChange(object sender, TextChangedEventArgs e)
        {
            string _usertext = userIDEntry.Text.ToUpper();

            userIDEntry.Text = _usertext;
        }

        public void iCTextChange(object sender, TextChangedEventArgs e)
        {
            string _ICtext = icEntry.Text;

            //Get Current Text
            if (_ICtext.Length > 20)
            {
                _ICtext = _ICtext.Remove(_ICtext.Length - 1);
                icEntry.Text = _ICtext;
            }
                
        }


        public async void Register_Clicked(object sender, System.EventArgs e)
        {
            
            try
            {
                if (!(String.IsNullOrEmpty(enterpriseEntry.Text)) && !(String.IsNullOrEmpty(userIDEntry.Text)) && !(String.IsNullOrEmpty(passwordEntry.Text)) && !(String.IsNullOrEmpty(icEntry.Text)))
                {
                    activityIndicator.IsRunning = true;

                    encryptedUserId = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(userIDEntry.Text));
                    encryptedPassword = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(passwordEntry.Text));
                    GetBasedURL();
                       
                }
                else
                {
                    await DisplayAlert("Error", "Please fill in all the field.", "OK");
                }

               
            }
            catch(Exception exception)
            {
                await DisplayAlert("Exception", exception.Message, "OK");
            }
           
        }

        public async void GetBasedURL()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri("https://api.asolute.com/");
            var uri = ControllerUtil.getBaseURL(enterpriseEntry.Text.ToUpper());
            var response = await client.GetAsync(uri);
            var content = await response.Content.ReadAsStringAsync();
            Debug.WriteLine(content);

            clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (json_response.IsGood)
            {
               
                Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                Ultis.Settings.AppEnterpriseName = enterpriseEntry.Text;
                RegisterUser();
            }
            else
            {
                await DisplayAlert("Error", json_response.Message, "OK");
            }
        }

        public async void RegisterUser()
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
            var success_uri = ControllerUtil.getRegistrationURL(enterpriseEntry.Text, encryptedUserId, encryptedPassword, icEntry.Text);
            var success_response = await client.GetAsync(success_uri);
            var success_content = await success_response.Content.ReadAsStringAsync();
            Debug.WriteLine(success_content);
            clsResponse register_response = JsonConvert.DeserializeObject<clsResponse>(success_content);

            if (register_response.IsGood)
            {
                await DisplayAlert("Success", "Registration successfully", "OK");
                Ultis.Settings.AppEnterpriseName = enterpriseEntry.Text;
                DownloadLogo();

            }
            else
            {
                await DisplayAlert("Failed", register_response.Message, "OK");
                activityIndicator.IsRunning = false;
            }
        }

        public async void DownloadLogo()
        {
            try
            {

                encryptedUserId = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(userIDEntry.Text));
                encryptedPassword = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(passwordEntry.Text));

                // string uri;
                var client = new HttpClient();
                client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                //equipment.Text                              
                 var uri = ControllerUtil.getLoginURL(encryptedUserId, encryptedPassword);                
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood == true)
                {
                    activityIndicator.IsRunning = false;
                    //save user equipment into db and which use to display it on list for user to choose (similar to auto complete)

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
                    Ultis.Settings.SessionUserId = userIDEntry.Text;
                    Ultis.Settings.SessionSettingKey = System.Net.WebUtility.UrlEncode(login_user.SessionId);


                    if (login_user.Language == 0)
                    {
                        Ultis.Settings.Language = "English";
                    }
                    else
                    {
                        Ultis.Settings.Language = "Malay";
                    }

                    if (login_user.GetLogo || !File.Exists(Ultis.Settings.GetAppLogoFileLocation(userIDEntry.Text)))
                    {
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
                                File.WriteAllBytes(Ultis.Settings.GetAppLogoFileLocation(userIDEntry.Text), bytes);

                                try
                                {
                                    var downloadLogoAckURI = ControllerUtil.getDownloadLogoAcknowledgementURL();
                                    var logoAckResp = await client.GetAsync(downloadLogoAckURI);

                                }
                                catch (Exception exception)
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
                        if (File.Exists(Ultis.Settings.GetAppLogoFileLocation(userIDEntry.Text)))
                        {
                            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
                            {
                                File.Delete(Ultis.Settings.GetAppLogoFileLocation());
                            }
                            File.Copy(Ultis.Settings.GetAppLogoFileLocation(userIDEntry.Text), Ultis.Settings.GetAppLogoFileLocation());
                        }
                    }

                    if (userIDEntry.Text == passwordEntry.Text)
                    {
                        Application.Current.MainPage = new ChangePasswordPage();
                    }
                    else
                    {
                        Application.Current.MainPage = new MainPage();
                    }

                }
                else
                {
                    await DisplayAlert("Error", login_response.Message, "OK");
                    this.activityIndicator.IsRunning = false;
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again", "OK");
            }
        }

    }
}