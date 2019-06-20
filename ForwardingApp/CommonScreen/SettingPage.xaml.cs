using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.IO;

using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class SettingPage : ContentPage
    {
        public SettingPage()
        {
            InitializeComponent();
            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }
            nameEntry.Text = Ultis.Settings.EnterpriseName;

            if(!(Device.RuntimePlatform == Device.iOS))
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }
            else
            {
                NavigationPage.SetHasNavigationBar(this, true);
            }
        }

        async void Url_Clicked(object sender, EventArgs e)
        {
            submitButton.IsEnabled = false;
            this.activityIndicator.IsRunning = true;
            try
            {
                var content = await CommonFunction.CallWebService(0, null, "https://api.asolute.com/", ControllerUtil.getBaseURL(nameEntry.Text), this);
                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood)
                {
                    string returnUri = json_response.Result;
                    if (!(returnUri.Contains("Unknown")))
                    {
                        Ultis.Settings.EnterpriseName = nameEntry.Text.ToUpper();
                        Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                        await DisplayAlert("Success", "Enterprise name updated.", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Enterprise name unknown", "OK");
                    }

                }


            }
            catch
            {

            }
            this.activityIndicator.IsRunning = false;
            submitButton.IsEnabled = true;
        }
    }
}
