using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Android;
using Android.Support.V4.App;
using Android.Telephony;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Utils;
//using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppNavigation: ContentPage
    {
        static string firebaseID = "qwert-qwer45-asfafaf";
        readonly Image splashImage;
        readonly CustomEntry entry;
        readonly CustomButton submit;
        ActivityIndicator loading;

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
                Text = "Welcome",
                FontAttributes = FontAttributes.Bold,
                FontSize = 20,

            };

            loading = new ActivityIndicator();


            AbsoluteLayout.SetLayoutFlags(splashImage, AbsoluteLayoutFlags.PositionProportional);
            AbsoluteLayout.SetLayoutBounds(splashImage, new Rectangle(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));

            sub.Children.Add(splashImage);

            ScrollView scroll = new ScrollView();

            StackLayout layout = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 10),
                VerticalOptions = LayoutOptions.Center
            };

            entry = new CustomEntry
            {
                Style = (Style)App.Current.Resources["entryStyle"],
                TextColor = Color.White,
                LineColor = Color.CornflowerBlue,
                Placeholder = "Email address",
                HorizontalTextAlignment = TextAlignment.Center,
                HorizontalOptions = LayoutOptions.Fill,
                Image = "organization",
                IsVisible = false

            };

            submit = new CustomButton
            {
                TextColor = Color.White,
                Text = "Submit",
                HorizontalOptions = LayoutOptions.Fill,
                IsVisible = false
            };

            StackLayout gap = new StackLayout
            {
                Spacing = 20,
                Padding = new Thickness(15, 15),
                VerticalOptions = LayoutOptions.Center
            };

            layout.Children.Add(sub);
            layout.Children.Add(welcome);
            layout.Children.Add(gap);
            layout.Children.Add(entry);
            layout.Children.Add(submit);
            layout.Children.Add(loading);


            scroll.Content = layout;
            this.BackgroundColor = Color.FromHex("#ffffff");
            this.Content = scroll;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if(Ultis.Settings.DeviceUniqueID.Equals(""))
            {
                entry.IsVisible = true;
                submit.IsVisible = true;
            }
            else
            {
                WebService(Ultis.Settings.DeviceUniqueID);
            }


            submit.Clicked += async (sender, e) =>
            {
                try
                {
                    loading.IsRunning = true;
                    loading.IsVisible = true;
                    loading.IsEnabled = true;

                    var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.emailVerify(entry.Text));
                    clsResponse verify_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (verify_response.IsGood)
                    {
                        WebService(entry.Text);
                    }
                    else
                    {
                        await DisplayAlert("JsonError", verify_response.Message, "OK");
                    }
                }
                catch(Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
        
            };

            //OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

            /*if(firebaseID.Equals(""))
            {
                firebaseID = "testing";
            }*/
        

        }

        async void WebService(string id)
        {
            try
            {
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getActionURL(id, firebaseID));
                clsResponse login_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (login_response.IsGood)
                {

                    var result = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    Ultis.Settings.SessionSettingKey = result.SessionId;
                    Ultis.Settings.SessionUserId = result.UserName;


                    if (result.MainMenu.Count == 1)
                    {
                        loading.IsRunning = false;
                        loading.IsVisible = false;
                        loading.IsEnabled = false;

                        string action = result.MainMenu[0].Id;

                        switch (action)
                        {
                            case "Register":
                                if (!(String.IsNullOrEmpty(entry.Text)))
                                {
                                    Ultis.Settings.Email = entry.Text;
                                }
                                Application.Current.MainPage = new CustomerRegistration();
                                break;

                            case "Activate":
                                Application.Current.MainPage = new AccountActivation();
                                break;

                            case "Home":
                                if (!(String.IsNullOrEmpty(entry.Text)))
                                {
                                    Ultis.Settings.DeviceUniqueID = entry.Text;
                                }
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
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }


        }

    }
}
