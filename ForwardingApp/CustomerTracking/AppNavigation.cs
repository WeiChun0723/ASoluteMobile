using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Utils;
using Com.OneSignal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.DeviceInfo;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppNavigation: ContentPage
    {
        readonly Image splashImage;
        readonly CustomEntry entry;
        readonly CustomButton submit;
        string firebaseID = "business";
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


            string test = CrossDeviceInfo.Current.AppVersion;

            if (Ultis.Settings.DeviceUniqueID.Equals("testing"))
            {
                Ultis.Settings.DeviceUniqueID = Guid.NewGuid().ToString("N");
            }

            WebService(Ultis.Settings.DeviceUniqueID);


            submit.Clicked += async (sender, e) =>
            {
                if (!(String.IsNullOrEmpty(entry.Text)))
                {
                    Ultis.Settings.Email = entry.Text;

                    try
                    {



                        loading.IsRunning = true;
                        loading.IsVisible = true;
                        loading.IsEnabled = true;

                       /* clsRegister register = new clsRegister
                        {
                            DeviceId = Ultis.Settings.DeviceUniqueID,
                            UserName = "",
                            Email = entry.Text,
                            MobileNo = "",
                            RegNo = "",
                            CompanyName = "",
                            FirebaseId = firebaseID,
                            DeviceIdiom = CrossDeviceInfo.Current.Idiom.ToString(),
                            DeviceMfg = CrossDeviceInfo.Current.Manufacturer,
                            DeviceModel = CrossDeviceInfo.Current.Model,
                            OSPlatform = CrossDeviceInfo.Current.Platform.ToString(),
                            OSVer = CrossDeviceInfo.Current.VersionNumber.ToString(),
                            AppVer = CrossDeviceInfo.Current.AppVersion,
                            AppName = clsRegister.AppNameConst.Business
                        };

                        var content = await CommonFunction.PostRequest(register, Ultis.Settings.SessionBaseURI, ControllerUtil.postRegisterURL());
                        clsResponse register_response = JsonConvert.DeserializeObject<clsResponse>(content);*/
                        var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.emailVerifyURL(entry.Text));
                        clsResponse verify_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (verify_response.IsGood)
                        {
                            loading.IsRunning = false;
                            loading.IsVisible = false;
                            loading.IsEnabled = false;

                            await Navigation.PushAsync(new CustomerRegistration(content));
                            //Application.Current.MainPage = new AccountActivation();
                        }
                        else
                        {
                            await DisplayAlert("JsonError", verify_response.Message, "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Missing Field", "Please key in all field", "OK");
                }

            };
        }

        async void WebService(string id)
        {
            try
            {
                if (Ultis.Settings.FireID.Equals("firebase"))
                {
                    OneSignal.Current.IdsAvailable((playerID, pushToken) => firebaseID = playerID);

                    Ultis.Settings.FireID = firebaseID;
                }


                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getActionURL(id, firebaseID));
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
                                entry.IsVisible = true;
                                submit.IsVisible = true;

                                break;

                            case "Activate":
                                Application.Current.MainPage = new AccountActivation();
                                break;

                            case "Home":

                                Application.Current.MainPage = new MainPage();
                                break;
                            case "HaulageVolume":
                                //Application.Current.MainPage = new CustomNavigationPage(new LoadingScreen());
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
