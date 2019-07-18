using System;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Plugin.Geolocator;
using Rg.Plugins.Popup.Services;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class ContainerDetails : ContentPage
    {
        string provider_code, provider_container, latitude, longtitude, rfcValue, rfcHours,category;
        
        public ProviderDetails previousPage;

        public ContainerDetails(string code, ListItems container)
        {
            InitializeComponent();
            provider_code = code;
            provider_container = container.Id;
            category = container.Category;

            MessagingCenter.Subscribe<App>((App)Application.Current, "RefreshDetail",async  (sender)  => {
                await GetContainerDetail();
            });

            if(category == "Pending Acknowledgement")
            {
                confirmBtn.IsVisible = true;
            }
        }

        protected override async void OnAppearing()
        {
            await StartListening();
            await GetContainerDetail();
        }

        public async Task StartListening()
        {
            if (CrossGeolocator.Current.IsListening)
                return;

            await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(60), 0, true, new Plugin.Geolocator.Abstractions.ListenerSettings
            {
                ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                AllowBackgroundUpdates = true,
                DeferLocationUpdates = true,
                DeferralDistanceMeters = 1,
                ListenForSignificantChanges = true,
                PauseLocationUpdatesAutomatically = true
            });
        }


        void ShowOnMap()
        {
            if(!(String.IsNullOrEmpty(latitude)) && !(String.IsNullOrEmpty(longtitude)))
            {
                GoogleMap.IsVisible = true;
                switchChange.IsVisible = true;
                GoogleMap.IsShowingUser = false;
            
                double loc_latitude = Convert.ToDouble(latitude);
                double loc_longtitude = Convert.ToDouble(longtitude);

                if(loc_latitude == 0.00 || loc_longtitude == 0.00)
                {
                    GoogleMap.IsVisible = false;
                    switchChange.IsVisible = false;
                }
                else
                {
                    GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(loc_latitude, loc_longtitude), Distance.FromMeters(100)));

                    var pin = new Pin
                    {
                        Position = new Position(loc_latitude, loc_longtitude),
                        Label = ""
                    };

                    GoogleMap.Pins.Add(pin);
                }
              
            }
            else
            {
                GoogleMap.IsVisible = false;
                switchChange.IsVisible = false;
            }
        }

        public async void Handle_Clicked(object sender, EventArgs e)
        {
            var button = sender as SfButton;

            switch(button.StyleId)
            {
                case "rfcBtn":
                    PopUp up = new PopUp(provider_code, rfcValue, rfcHours, provider_container,"containerDetail");
                    await PopupNavigation.Instance.PushAsync(up);
                    break;

                case "confirmBtn":
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.updatePODURL(provider_code, provider_container), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if(response.IsGood)
                    {
                        await Navigation.PopAsync();

                        MessagingCenter.Send<App,string>((App)Application.Current, "RefreshContainers",provider_container);

                        MessagingCenter.Send<App>((App)Application.Current, "RefreshCategory");
                    }
                    break;
            }

            
        }

        public void switchMap(object sender, ToggledEventArgs e)
        {
            if(changeMap.IsToggled)
            {
                GoogleMap.MapType = MapType.Satellite;
            }
            else
            {
                GoogleMap.MapType = MapType.Street;
            }
        }

        public async Task GetContainerDetail()
        {
            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerDetailURL(provider_code, provider_container),this);
            if(content != null)
            {
                clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (container_response.IsGood)
                {
                    rfcBtn.IsVisible = false;
                    var containers = JObject.Parse(content)["Result"].ToObject<clsDataRow>();
                    containerDetails.Children.Clear();

                    foreach (clsCaptionValue details in containers.Details)
                    {
                        StackLayout stackLayout = new StackLayout { Orientation = StackOrientation.Horizontal, VerticalOptions = LayoutOptions.Center, HorizontalOptions = LayoutOptions.Start, Padding = new Thickness(0, 0, 0, 10) };

                        if (details.Display == true)
                        {
                            Label captionLabel = new Label()
                            {
                                FontAttributes = FontAttributes.Bold,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                Text = details.Caption + ":  ",
                                WidthRequest = 120
                            };
                            Label valueLabel = new Label()
                            {
                                FontAttributes = FontAttributes.Bold,
                                Text = details.Value
                            };

                            stackLayout.Children.Add(captionLabel);
                            stackLayout.Children.Add(valueLabel);
                            containerDetails.Children.Add(stackLayout);
                        }

                        if (details.Caption.Equals("Container No."))
                        {
                            Title = details.Value;
                        }
                        else if (String.IsNullOrEmpty(Title))
                        {
                            Title = provider_container;
                        }

                        if (details.Caption.Equals("Latitude"))
                        {
                            latitude = details.Value;
                        }
                        if (details.Caption.Equals("Longitude"))
                        {
                            longtitude = details.Value;
                        }

                        if (details.Caption.Equals("RFC"))
                        {
                            rfcValue = details.Value;
                            rfcBtn.IsVisible = true;
                        }
                        else if (details.Caption.Equals("RFC Hour"))
                        {
                            rfcHours = details.Value;
                            rfcBtn.IsVisible = true;
                        }
                    }

                    ShowOnMap();
                }
                
            }
            
        }
    }
}
