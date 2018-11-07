using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class DriverRFC : ContentPage
    {
        public DriverRFC()
        {
            InitializeComponent();

            Title = "Collection Request";

         
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

        public void ContPreChange(object sender, TextChangedEventArgs e)
        {
            string _prefixtext = contPrefix.Text.ToUpper();
            contPrefix.Text = _prefixtext;
            //Get Current Text
           
             if (_prefixtext.Length == 4)
             {
                    contNum.Focus();
             }

        }

        public void ContNumChange(object sender, TextChangedEventArgs e)
        {
            string _numbertext = contNum.Text;

            if(contNum.Text.Contains("."))
            {
                string[] integer = contNum.Text.Split('.');
                contNum.Text = integer[0];
            }
            else
            {
                //Get Current Text
                if (_numbertext.Length > 7)
                {
                    _numbertext = _numbertext.Remove(_numbertext.Length - 1);
                    contNum.Text = _numbertext;
                }
                else if (_numbertext.Length == 7)
                {
                    contNum.Unfocus();
                }
            }
          
        }

        public async void SendContainerNumber(object sender, EventArgs e)
        {
            if(!(String.IsNullOrEmpty(contPrefix.Text)) && !(String.IsNullOrEmpty(contNum.Text)))
            {
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.postDriverRFCURL(contPrefix.Text + contNum.Text));
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(response.IsGood)
                {
                    await DisplayAlert("Sucess", "Container number uploaded.", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", response.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Missing field", "Please key in all field.", "OK");
            }

        }
    }
}
