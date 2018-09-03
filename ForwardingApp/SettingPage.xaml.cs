using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
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
            urlEntry.Text = Ultis.Settings.AppEnterpriseName;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        async  void Url_Clicked(object sender, EventArgs e)
        {
            if(urlEntry.Text.Length > 0){
                this.activityIndicator.IsRunning = true;
                activityIndicator.IsVisible = true;
                Ultis.Settings.SessionBaseURI = urlEntry.Text;
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://api.asolute.com/");
                var uri = ControllerUtil.getBaseURL(urlEntry.Text.ToUpper());
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);

                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood)
                {
                    Ultis.Settings.AppEnterpriseName = urlEntry.Text.ToUpper();
                    Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                    await DisplayAlert("Success", "Enterprise name updated.", "Ok");
                    await Navigation.PopAsync();
                }
            } else {
                await DisplayAlert("Error", "Base URL must not be empty.", "Ok");
            }

            this.activityIndicator.IsRunning = false;
            activityIndicator.IsVisible = false;
        }
    }
}
