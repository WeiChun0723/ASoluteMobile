using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android;
using Android.Support.V4.App;
using Android.Telephony;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
//using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class AppNavigation: ContentPage
    {
        static string firebaseID = "qwert-qwer45-asfafaf";
        readonly Image splashImage, splashDeviceImage;

        public AppNavigation()
        {

            NavigationPage.SetHasNavigationBar(this, false);

            var sub = new AbsoluteLayout();

            splashImage = new Image
            {
                Source = "appIcon.png",
                HeightRequest = 100,
                WidthRequest = 100
            };

            Label welcome = new Label
            {
                HorizontalOptions = LayoutOptions.CenterAndExpand,
                Text = "Loading",
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,
                IsVisible = false
            };

            AbsoluteLayout.SetLayoutFlags(splashImage, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage, new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            sub.Children.Add(splashImage);

            ScrollView scroll = new ScrollView();

            StackLayout layout = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 10),
                //HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            layout.Children.Add(sub);
            layout.Children.Add(splashDeviceImage);
            layout.Children.Add(welcome);


            scroll.Content = layout;
            this.BackgroundColor = Color.FromHex("#ffffff");
            this.Content = scroll;
        }



        protected override async void OnAppearing()
        {
            base.OnAppearing();

            uint duration = 5 * 1000;

            await Task.WhenAll(

              splashImage.RotateYTo(13 * 360, duration)
            );

            //OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            /*if(firebaseID.Equals(""))
            {
                firebaseID = "testing";
            }*/
            if (Ultis.Settings.DeviceUniqueID.Equals(""))
            {
                Application.Current.MainPage = new CustomerRegistration();
            }
            else
            {
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
                          
                            case "Activate":
                                Application.Current.MainPage = new AccountActivation();
                                break;

                            case "Home":
                                Application.Current.MainPage = new MainPage();
                                break;
                        }

                    }
                }

                else
                {
                    await DisplayAlert("JsonError", login_response.Message, "OK");
                }

            }

        }

    }
}
