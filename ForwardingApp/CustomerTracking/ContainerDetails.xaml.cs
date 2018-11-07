using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace ASolute_Mobile.CustomerTracking
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ContainerDetails : ContentPage
    {
        string provider_code, provider_container, latitude, longtitude, rfcValue, rfcHours;
        public ProviderDetails previousPage;

        public ContainerDetails(string code, string container)
        {
            InitializeComponent();
            provider_code = code;
            provider_container = container;

            MessagingCenter.Subscribe<App>((App)Application.Current, "OnCategoryCreated",async  (sender)  => {
                await GetContainerDetail();
            });

        }

        protected async override void OnAppearing()
        {
            await GetContainerDetail();
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

                GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(loc_latitude, loc_longtitude), Distance.FromMeters(100)));

                var pin = new Pin
                {
                    Position = new Position(loc_latitude, loc_longtitude),
                    Label = ""
                };

                GoogleMap.Pins.Add(pin);
            }
            else
            {
                GoogleMap.IsVisible = false;
                switchChange.IsVisible = false;
            }
        
        }

        public async void updateRFC(object sender, EventArgs e)
        {
            PopUp up = new PopUp(provider_code, rfcValue, rfcHours, provider_container);
            
            await PopupNavigation.Instance.PushAsync(up);
           
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
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerDetail(provider_code, provider_container));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(container_response.IsGood)
            {
                rfc.IsVisible = false;
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

                    if(details.Caption.Equals("Container No."))
                    {
                        Title = details.Value;
                    }
                    else if(String.IsNullOrEmpty(Title))
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

                    if(details.Caption.Equals("RFC"))
                    {
                        rfcValue = details.Value;
                        rfc.IsVisible = true;
                    }
                    else if (details.Caption.Equals("RFC Hour"))
                    {
                        rfcHours = details.Value;
                        rfc.IsVisible = true;
                    }
                }

                ShowOnMap();
            }
            else
            {
                await DisplayAlert("JsonError", container_response.Message, "OK");
            }
        }
    }
}
