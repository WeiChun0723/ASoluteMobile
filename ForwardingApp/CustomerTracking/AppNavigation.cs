using System;
using System.Diagnostics;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class AppNavigation: ContentPage
    {
        static string firebaseID;

        public AppNavigation()
        {
            NavigationPage.SetHasNavigationBar(this, false);
          
        }

        protected override async void OnAppearing()
        {
            string test2 = Ultis.Settings.DeviceUniqueID;

            OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getActionURL(Ultis.Settings.DeviceUniqueID, firebaseID));
            clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (login_response.IsGood)
            {

                var result = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                Ultis.Settings.SessionSettingKey = result.SessionId;
                Ultis.Settings.SessionUserId = result.UserName;

                if (result.MainMenu.Count == 1)
                {
                    string action = result.MainMenu[0].Id;

                    switch (action)
                    {
                        case "Register":
                            Application.Current.MainPage = new CustomerRegistration();
                            break;

                        case "Activate":
                            Application.Current.MainPage = new AccountActivation();
                            break;

                        case "MyProviders":
                            Application.Current.MainPage = new MainPage();
                            break;
                        
                        case "Home":
                            Application.Current.MainPage = new MainPage();
                            break;
                    }

                }
                else
                {
                    Application.Current.MainPage = new MainMenu();
                }
            }
            else
            {

            }

        }


    }
}
