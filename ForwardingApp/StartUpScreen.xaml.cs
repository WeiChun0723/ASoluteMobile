using System;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.DeviceInfo;
using Plugin.Geolocator;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class StartUpScreen : ContentPage
    {

        string firebaseID = "tracking";

        public StartUpScreen()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);

            if (Ultis.Settings.App.Contains("Tracking") || Ultis.Settings.App.Contains("Business"))
            {

                title.Text = "Please enter email.";


                if (Ultis.Settings.DeviceUniqueID.Equals("DeviceID"))
                {
                    Ultis.Settings.DeviceUniqueID = Guid.NewGuid().ToString("N");
                }

                GetAction(Ultis.Settings.DeviceUniqueID);
            }
            else
            {
                title.Text = "Please enter enterprise.";
            }
        }

        protected override async void OnAppearing()
        {

            uint duration = 3 * 1000;

            await Task.WhenAll(

              splashImage.RotateYTo(13 * 360, duration)
            );


            splashImage.IsVisible = false;
            entryStack.IsVisible = true;

            if (Ultis.Settings.App.Contains("Tracking") || Ultis.Settings.App.Contains("Business"))
            {
                emailEntryView.IsVisible = true;
            }
            else
            {
                enterpriseEntryView.IsVisible = true;
            }
        }

        async void GetAction(string deviceID)
        {
            try
            {
                CommonFunction.GetFireBaseID();

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getActionURL(deviceID, firebaseID), this);
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood)
                {

                    var result = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    Ultis.Settings.SessionSettingKey = result.SessionId;
                    Ultis.Settings.SessionUserId = result.UserName;


                    if (result.MainMenu.Count == 1)
                    {
                        loading.IsVisible = false;
                        loading.IsEnabled = false;

                        string action = result.MainMenu[0].Id;

                        switch (action)
                        {
                            case "Register":
                                emailEntryView.IsVisible = true;
                                submit.IsVisible = true;
                                break;

                            case "Activate":
                                Application.Current.MainPage = new AccountActivation();
                                break;

                            case "Home":
                                Application.Current.MainPage = new MainPage();
                                break;

                            case "HaulageVolume":
                                Application.Current.MainPage = new MainPage();
                                break;
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }

        async void GetBaseURL(object s, EventArgs e)
        {

            try
            {
                //var position = await Geolocation.GetLastKnownLocationAsync();

                loading.IsVisible = true;

                if (!String.IsNullOrEmpty(startUpEnterprise.Text) || !String.IsNullOrEmpty(startUpEmail.Text))
                {

                    Ultis.Settings.Email = startUpEmail.Text;

                    switch (Ultis.Settings.App)
                    {
                        case "asolute.Mobile.AILSTracking":
                        case "com.asolute.AILSTracking":
                            var tracking_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.emailVerifyURL(startUpEmail.Text), this);
                            clsResponse verify_response = JsonConvert.DeserializeObject<clsResponse>(tracking_content);

                            if (verify_response.IsGood)
                            {
                                loading.IsVisible = false;
                                loading.IsEnabled = false;

                                await Navigation.PushAsync(new CustomerRegistration(tracking_content));
                            }
                            break;

                        case "asolute.Mobile.AILSBusiness":
                        case "com.asolute.AILSBusiness":
                            clsRegister register = new clsRegister
                            {
                                DeviceId = Ultis.Settings.DeviceUniqueID,
                                UserName = "",
                                Email = startUpEmail.Text,
                                MobileNo = "",
                                RegNo = "",
                                CompanyName = "",
                                FirebaseId = firebaseID,
                                DeviceIdiom = CrossDeviceInfo.Current.Idiom.ToString(),
                                DeviceMfg = CrossDeviceInfo.Current.Manufacturer,
                                DeviceModel = CrossDeviceInfo.Current.Model,
                                OSPlatform = CrossDeviceInfo.Current.Platform.ToString(),
                                OSVer = CrossDeviceInfo.Current.VersionNumber.ToString(),
                                AppVer = CrossDeviceInfo.Current.AppVersion,
                                AppName = clsRegister.AppNameConst.Business
                            };

                            var business_content = await CommonFunction.CallWebService(1, register, Ultis.Settings.SessionBaseURI, ControllerUtil.postBusinessRegisterURL(), this);
                            clsResponse register_response = JsonConvert.DeserializeObject<clsResponse>(business_content);
                            if (register_response.IsGood)
                            {

                                loading.IsVisible = false;
                                loading.IsEnabled = false;

                                Application.Current.MainPage = new AccountActivation();
                            }
                            break;

                        default:
                            Ultis.Settings.AppFirstInstall = "Second";

                            clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(await CommonFunction.CallWebService(0, null, "https://api.asolute.com/", ControllerUtil.getBaseURL(startUpEnterprise.Text), this));

                            if (json_response.IsGood)
                            {
                                Ultis.Settings.EnterpriseName = startUpEnterprise.Text.ToUpper();
                                Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                            }

                            Application.Current.MainPage = new NavigationPage(new LoginPage());
                            break;
                    }
                }
                else
                {
                    await DisplayAlert("Missing field", "Please fill in all field.", "OK");
                }

                loading.IsVisible = false;
            }
            catch
            {
                await DisplayAlert("Unable to connect", "Please connect to internet", "OK");
            }


        }

    }
}
