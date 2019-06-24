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

	public partial class CurrentLocation : ContentPage
	{
		public CurrentLocation ()
		{
			InitializeComponent ();
            Title = (Ultis.Settings.Language.Equals("English")) ? "Send Current Location": "Hantar Lokasi Semasa";
        }

      
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }


        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new MainPage();
            return true;
        }

        public async void UpdateLocation(object sender, EventArgs e)
        {
            if (!(String.IsNullOrEmpty(locationName.Text)))
            {
                try
                {
                    activityIndicator.IsRunning = true;
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getLocationURL(locationName.Text), this);
                    clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (json_response.IsGood)
                    {
                        await DisplayAlert("Success", "Message sent successfully.", "OK");
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