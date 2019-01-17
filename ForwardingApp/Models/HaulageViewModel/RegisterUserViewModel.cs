using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.Models.HaulageViewModel
{
    public class RegisterUserViewModel : PropertyChange
    {
        string enterpriseName, userID, password, IC, encryptedUserId, encryptedPassword;
        bool isBusy = false;
        public Command Register { get; }

        public RegisterUserViewModel()
        {
            Register = new Command(async () => await RegisterUser(),
                                            () => !IsBusy);
        }

        public string EnterpriseName
        {
            set 
            {
                enterpriseName = value.ToUpper();
                SetProperty(ref enterpriseName, value); 
            }
            get 
            {
               
                    return enterpriseName;

            }
        }

        public string UserID
        {
            set { SetProperty(ref userID, value); }
            get { return userID; }
        }

        public string Password
        {
            set { SetProperty(ref password, value); }
            get { return password; }
        }

        public string ICNumber
        {
            set { SetProperty(ref IC, value); }
            get { return IC; }
        }

        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;

                SetProperty(ref isBusy, value);

                OnPropertyChanged(nameof(IsBusy));
                Register.ChangeCanExecute();

            }
        }

        async Task RegisterUser()
        {
            IsBusy = true;

            if (!(String.IsNullOrEmpty(EnterpriseName)) && !(String.IsNullOrEmpty(UserID)) && !(String.IsNullOrEmpty(Password)) && !(String.IsNullOrEmpty(ICNumber)))
            {
                encryptedUserId = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(UserID));
                encryptedPassword = System.Net.WebUtility.UrlEncode(clsCommonFunc.AES_Encrypt(Password));

                var content = await CommonFunction.GetWebService("https://api.asolute.com/", ControllerUtil.getBaseURL(EnterpriseName));
                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood)
                {
                    Ultis.Settings.RefreshMenuItem = "Yes";
                    Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                    Ultis.Settings.AppEnterpriseName = EnterpriseName;
                    RegisterUserUrl();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Web Service Error", json_response.Message, "OK");
                }
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Missing field", "Please fill in all field.", "Ok");
            }


           IsBusy = false;
        }

        public async void RegisterUserUrl()
        {
            try
            {
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getRegistrationURL(EnterpriseName, encryptedUserId, encryptedPassword, IC));
                clsResponse register_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (register_response.IsGood)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Registration successfully", "OK");
                    Ultis.Settings.AppEnterpriseName = EnterpriseName;
                    AutoLogin();
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Failed", register_response.Message, "OK");

                }
            }
            catch
            {
                await Application.Current.MainPage.DisplayAlert("Error", "The baseURL for enterprise " + EnterpriseName + " ("
                                   + Ultis.Settings.SessionBaseURI + ") " + "cannot be use.", "OK");
            }


        }


        public async void AutoLogin()
        {
            try
            {
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI,
                                                                                       ControllerUtil.getLoginURL(encryptedUserId, encryptedPassword)));


                if (login_response.IsGood == true)
                {
                  
                    //save user equipment into db and which use to display it on list for user to choose (similar to auto complete)

                    var login_user = JObject.Parse(login_response.Result).ToObject<clsLogin>();

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

                    var userObject = JObject.Parse(login_response.Result).ToObject<UserItem>();
                    //Save user info to constant
                    Ultis.Settings.SessionUserItem = userObject;
                    Ultis.Settings.SessionUserId = UserID;
                    Ultis.Settings.SessionSettingKey = System.Net.WebUtility.UrlEncode(login_user.SessionId);


                    if (login_user.Language == 0)
                    {
                        Ultis.Settings.Language = "English";
                    }
                    else
                    {
                        Ultis.Settings.Language = "Malay";
                    }

                    if (login_user.GetLogo || !File.Exists(Ultis.Settings.GetAppLogoFileLocation(UserID)))
                    {
                        clsResponse logo_response = JsonConvert.DeserializeObject<clsResponse>(await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI,
                                                                                       ControllerUtil.getDownloadLogoURL()));


                       
                        if (logo_response.IsGood)
                        {
                            if (logo_response.Result != null)
                            {
                                byte[] bytes = Convert.FromBase64String(logo_response.Result);
                                File.WriteAllBytes(Ultis.Settings.GetAppLogoFileLocation(), bytes);
                                File.WriteAllBytes(Ultis.Settings.GetAppLogoFileLocation(UserID), bytes);

                                try
                                {
                                    var client = new HttpClient();
                                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                                    var downloadLogoAckURI = ControllerUtil.getDownloadLogoAcknowledgementURL();
                                    var logoAckResp = await client.GetAsync(downloadLogoAckURI);

                                }
                                catch (Exception exception)
                                {
                                    await Application.Current.MainPage.DisplayAlert("Error", exception.Message, "OK");
                                }
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Download Logo Error", "No image.", "Ok");
                            }
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Download Logo Error", "No image.", "Ok");
                        }
                    }
                    else
                    {
                        if (File.Exists(Ultis.Settings.GetAppLogoFileLocation(UserID)))
                        {
                            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
                            {
                                File.Delete(Ultis.Settings.GetAppLogoFileLocation());
                            }
                            File.Copy(Ultis.Settings.GetAppLogoFileLocation(UserID), Ultis.Settings.GetAppLogoFileLocation());
                        }
                    }

                    /*if (userIDEntry.Text == passwordEntry.Text)
                    {
                        Application.Current.MainPage = new ChangePasswordPage();
                    }
                    else
                    {
                        Application.Current.MainPage = new MainPage();
                    }*/

                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", login_response.Message, "OK");

                }
            }
            catch
            {
                //await Application.Current.MainPage.DisplayAlert("Error", "Please try again", "OK");
            }
        }

    }
}
