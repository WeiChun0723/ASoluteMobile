using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.HaulageScreen
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class CurrentLocation : ContentPage
	{
		public CurrentLocation ()
		{
			InitializeComponent ();
            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Send Current Location";
            }
            else
            {
                Title = "Hantar Lokasi Semasa";
            }
            Ultis.Settings.App = "Haulage";

  
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }


        public void editorTextChanged(object sender, TextChangedEventArgs e)
        {
            string _editortext = locationName.Text.ToUpper();

            //Get Current Text
            if (_editortext.Length > 50)
            {
                _editortext = _editortext.Remove(_editortext.Length - 1);

                locationName.Unfocus();
            }

            locationName.Text = _editortext;
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new MainPage();
            return true;
        }

        public async void updateLocation(object sender, EventArgs e)
        {
            if (!(String.IsNullOrEmpty(locationName.Text)))
            {
                
                try
                {
                    activityIndicator.IsRunning = true;
                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var uri = ControllerUtil.getLocationURL(locationName.Text);
                    var response = await client.GetAsync(uri);
                    var content = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine(content);
                    clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (json_response.IsGood)
                    {
                        await DisplayAlert("Success", "Message sent successfully.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", json_response.Message, "OK");
                    }
                    activityIndicator.IsRunning = false;
                }
                catch(Exception exception)
                {
                    await DisplayAlert("Error", exception.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please key in location name", "OK");
            }
        }

    }
}