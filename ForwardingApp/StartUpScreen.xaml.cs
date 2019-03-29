using System;
using System.Net.Http;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Plugin.Geolocator;
using Plugin.Media;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class StartUpScreen : ContentPage
    {
        public StartUpScreen()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            uint duration = 3 * 1000;

            await Task.WhenAll(

              splashImage.RotateYTo(13 * 360, duration)
            );

            if (Ultis.Settings.AppFirstInstall == "First")
            {
                title.IsVisible = true;
                enterpriseView.IsVisible = true;
                submit.IsVisible = true;
                splashDeviceImage.IsVisible = true;
                splashImage.IsVisible = false;
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }
        }

        public async void GetBaseURL(object s, EventArgs e)
        {

            try
            {
                loading.IsVisible = true;

                if (!String.IsNullOrEmpty(enterpriseEntry.Text))
                {
                    Ultis.Settings.AppFirstInstall = "Second";

                    clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(await CommonFunction.CallWebService(0, null, "https://api.asolute.com/", ControllerUtil.getBaseURL(enterpriseEntry.Text),this));

                    if (json_response.IsGood)
                    {
                        Ultis.Settings.EnterpriseName = enterpriseEntry.Text.ToUpper();
                        Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                    }
                    else
                    {
                        await DisplayAlert("Json Error", json_response.Message, "OK");
                    }

                    Application.Current.MainPage = new NavigationPage(new LoginPage());

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
