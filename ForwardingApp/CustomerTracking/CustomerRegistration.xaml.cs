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

        public void userNameTextChange(object sender, TextChangedEventArgs e)
        {
            string _name = userNameEntry.Text.ToUpper(); ;
            if (_name.Length > 50)
            {
                _name = _name.Remove(_name.Length - 1);

            }
            userNameEntry.Text = _name;
        }

        public void emailTextChange(object sender, TextChangedEventArgs e)
        {
            string _email = emailAddressEntry.Text; 
            if (_email.Length > 50)
            {
                _email = _email.Remove(_email.Length - 1);

            }
            emailAddressEntry.Text = _email;
        }

        public void mobileTextChange(object sender, TextChangedEventArgs e)
        {
            string _phone = phoneEntry.Text;
            if (_phone.Length > 20)
            {
                _phone = _phone.Remove(_phone.Length - 1);

            }
            phoneEntry.Text = _phone;
        }


        public void businessTextChange(object sender, TextChangedEventArgs e)
        {
            string _business = businessRegEntry.Text.ToUpper();
            if (_business.Length > 20)
            {
                _business = _business.Remove(_business.Length - 1);

            }
            businessRegEntry.Text = _business;
        }

        public void companyTextChange(object sender, TextChangedEventArgs e)
        {
            string _company = companyEntry.Text.ToUpper();
            if (_company.Length > 100)
            {
                _company = _company.Remove(_company.Length - 1);

            }
            companyEntry.Text = _company;
        }

        public async void Register_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (RegisterButton.Text.Equals("Next"))
                {
                    if (emailAddressEntry.Text.Contains("@") && emailAddressEntry.Text.Contains("."))
                    {
                        var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getCompanyNameURL(businessRegEntry.Text));
                        clsResponse company_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (company_response.IsGood)
                        {
                            companyEntry.IsVisible = true;
                            term.IsVisible = true;

                            companyEntry.Text = company_response.Result;
                            RegisterButton.Text = "Confirm";
                        }
                        else
                        {
                            await DisplayAlert("Json Error", company_response.Message, "OK");
                        }

                    }
                    else
                    {
                        await DisplayAlert("Error", "Email address must in example@mail.com format", "OK");
                    }

                }
                else
                {
                    if (term.Checked)
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

                            var content = await CommonFunction.PostRequestAsync(register, Ultis.Settings.SessionBaseURI, ControllerUtil.postRegisterURL());
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

                    }
                    else
                    {
                        await DisplayAlert("Reminder", "Please tick the check box to continue", "OK");
                    }

                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

    }
}
