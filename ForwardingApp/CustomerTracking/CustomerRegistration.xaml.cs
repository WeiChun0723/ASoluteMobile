using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.DeviceInfo;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class CustomerRegistration : ContentPage
    {
       string firebaseID = "";
        clsRegister result;

        public CustomerRegistration(string content)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this,false);

            if(!(String.IsNullOrEmpty(Ultis.Settings.Email)))
            {
                emailAddressEntry.Text = Ultis.Settings.Email;
            }

            if(content!="")
            {
                clsResponse verify_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (verify_response.Result != null)
                {
                    result = JObject.Parse(content)["Result"].ToObject<clsRegister>();

                    userNameEntry.Text = result.UserName;
                    phoneEntry.Text = result.MobileNo;
                    businessRegEntry.Text = result.RegNo;
                    companyEntry.Text = result.CompanyName;
                }
            }

        }

        public async void Register_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (RegisterButton.Text.Equals("Next"))
                {
                
                    if (emailAddressEntry.Text.Contains("@"))
                    {

                        string[] emailFormat = emailAddressEntry.Text.Split('@');

                        if(emailFormat[1].Contains("."))
                        {
                            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCompanyNameURL(businessRegEntry.Text), this);
                            clsResponse company_response = JsonConvert.DeserializeObject<clsResponse>(content);

                            if (company_response.IsGood)
                            {
                                companyEntryView.IsVisible = true;
                                term.IsVisible = true;

                                companyEntry.Text = company_response.Result;
                                RegisterButton.Text = "Confirm";
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Email address must in example@mail.com format.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Email address must in example@mail.com format.", "OK");
                    }
                }
                else
                {

                        if (!(String.IsNullOrEmpty(emailAddressEntry.Text)) && !(String.IsNullOrEmpty(userNameEntry.Text)) && !(String.IsNullOrEmpty(phoneEntry.Text))
                          && !(String.IsNullOrEmpty(businessRegEntry.Text)) && !(String.IsNullOrEmpty(companyEntry.Text)))
                        {
                            try
                            {
                                OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

                                Ultis.Settings.FireID = firebaseID;
                            }
                            catch
                            {

                            }

                            clsRegister register = new clsRegister();

                            register.DeviceId = Ultis.Settings.DeviceUniqueID;
                            register.UserName = userNameEntry.Text;
                            register.Email = emailAddressEntry.Text;
                            register.MobileNo = phoneEntry.Text;
                            register.RegNo = businessRegEntry.Text;
                            register.CompanyName = companyEntry.Text;
                            register.FirebaseId = firebaseID;
                            register.DeviceIdiom = CrossDeviceInfo.Current.Idiom.ToString();
                            register.DeviceMfg = CrossDeviceInfo.Current.Manufacturer;
                            register.DeviceModel = CrossDeviceInfo.Current.Model;
                            register.OSPlatform = CrossDeviceInfo.Current.Platform.ToString();
                            register.OSVer = CrossDeviceInfo.Current.VersionNumber.ToString();
                            register.AppVer = CrossDeviceInfo.Current.AppVersion;

                            var content = await CommonFunction.PostRequestAsync(register, Ultis.Settings.SessionBaseURI, ControllerUtil.postBusinessRegisterURL());
                            clsResponse register_response = JsonConvert.DeserializeObject<clsResponse>(content);

                            if (register_response.IsGood)
                            {

                                Application.Current.MainPage = new AccountActivation();
                            }
                            else
                            {
                                await DisplayAlert("Json Error", register_response.Message, "OK");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Please enter all the field .", "OK");
                        }

                    /*if (term.Checked)
                    {
                    }
                    else
                    {
                        await DisplayAlert("Reminder", "Please tick the check box to continue", "OK");
                    }*/

                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

    }
}
