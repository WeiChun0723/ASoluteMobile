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
        public DriverRFC(string title)
        {
            InitializeComponent();

            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text = title,
                TextColor = Color.White
            };

            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);
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
                var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.postDriverRFCURL(contPrefix.Text + contNum.Text),this);
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
