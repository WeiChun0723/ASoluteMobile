using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using ASolute_Mobile.CustomRenderer;
using System.Net.Http;
using System.Diagnostics;
using ASolute_Mobile.Utils;
using ASolute.Mobile.Models;
using Newtonsoft.Json;

namespace ASolute_Mobile
{
    public class SplashScreen : ContentPage
    {
        readonly Image splashImage,splashDeviceImage;
        readonly CustomEntry entry;
        readonly CustomButton submit;
        readonly Label title;
        readonly StackLayout layout;
        ScrollView ScrollView;

        public SplashScreen()
        {
            NavigationPage.SetHasNavigationBar(this, false);

            var sub = new AbsoluteLayout();

            splashImage = new Image
            {
                Source = "appIcon.png",
                HeightRequest = 100,
                WidthRequest = 100
            };

            splashDeviceImage = new Image
            {
                Source = "splashScreenlogo.png",
                HeightRequest = 300,
                WidthRequest = 150,
                IsVisible= false
            };

            Label welcome = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Welcome",
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,
                IsVisible = false
            };

            title = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text="Please enter your Enterprise name",              
                IsVisible = false,
                FontAttributes = FontAttributes.Bold,    
                FontSize = 15
            };

            entry = new CustomEntry
            {
                Style = (Style)App.Current.Resources["entryStyle"],
                TextColor = Color.White,
                LineColor = Color.CornflowerBlue,
                Placeholder = "Enterprise name",
                HorizontalTextAlignment = TextAlignment.Center,
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Fill,
                Image = "organization"
                
            };

            submit = new CustomButton
            {
                TextColor = Color.White,
                Text = "Submit",
                IsVisible = false,
                HorizontalOptions = LayoutOptions.Fill,
            };


            AbsoluteLayout.SetLayoutFlags(splashImage, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage, new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
          
            sub.Children.Add(splashImage);
           

            layout = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 10),
                //HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            ScrollView = new ScrollView();

            StackLayout gap = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 15),               
                VerticalOptions = LayoutOptions.Center
            };

            layout.Children.Add(sub);
            layout.Children.Add(splashDeviceImage);
            layout.Children.Add(welcome);
            layout.Children.Add(title);
            layout.Children.Add(entry);
            layout.Children.Add(gap);
            layout.Children.Add(submit);

            ScrollView.Content = layout;
            this.BackgroundColor = Color.FromHex("#ffffff");
            this.Content = ScrollView;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
          
            uint duration = 5 * 1000;

            await Task.WhenAll(
              
              splashImage.RotateYTo(13 * 360, duration)
            );

            if(Ultis.Settings.AppFirstInstall == "First")
            {
                title.IsVisible = true;
                entry.IsVisible = true;
                submit.IsVisible = true;
                splashDeviceImage.IsVisible = true;
                splashImage.IsVisible = false;
                layout.Children.RemoveAt(0);
            }
            else
            {
                Application.Current.MainPage = new NavigationPage(new LoginPage());
            }

            submit.Clicked += async (sender, e)  => 
            {
                Ultis.Settings.AppFirstInstall = "Second";
               
                var client = new HttpClient();
                client.BaseAddress = new Uri("https://api.asolute.com/");
                var uri = ControllerUtil.getBaseURL(entry.Text.ToUpper());
                var response = await client.GetAsync(uri);
                var content = await response.Content.ReadAsStringAsync();
                Debug.WriteLine(content);

                clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (json_response.IsGood)
                {
                    Ultis.Settings.AppEnterpriseName = entry.Text.ToUpper();
                    Ultis.Settings.SessionBaseURI = json_response.Result + "api/";
                }
                else
                {
                    await DisplayAlert("Json Error", json_response.Message, "OK");
                }

                Application.Current.MainPage = new NavigationPage(new LoginPage());
            };
        }

    }
}
