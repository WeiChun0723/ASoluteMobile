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
            nameEntry.Text = Ultis.Settings.AppEnterpriseName;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        void EnterpriseName(object sender, TextChangedEventArgs e)
        {
            string _name = nameEntry.Text.ToUpper();

            if (_name.Length > 15)
            {
                _name = _name.Remove(_name.Length - 1);

                nameEntry.Unfocus();
            }

            nameEntry.Text = _name;
        }

        async void Url_Clicked(object sender, EventArgs e)
        {

                this.activityIndicator.IsRunning = true;

                var content = await CommonFunction.GetWebService("https://api.asolute.com/", ControllerUtil.getBaseURL(nameEntry.Text.ToUpper()));
                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood)
                {
                    string returnUri = json_response.Result;
                    if (!(returnUri.Contains("Unknown")))
                    {
                        Ultis.Settings.AppEnterpriseName = nameEntry.Text.ToUpper();
                        Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                        await DisplayAlert("Success", "Enterprise name updated.", "Ok");
                        await Navigation.PopAsync();
                    }
                    else
                    {
                        await DisplayAlert("Error", "Enterprise name unknown", "OK");
                    }
                   
                }
                else
                {
                await DisplayAlert("Error", json_response.Message, "OK");
                }   

                this.activityIndicator.IsRunning = false;
                
        }
    }
}
