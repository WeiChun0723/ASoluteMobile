using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ASolute_Mobile.Planner
{
    public partial class EquipmentDetails : ContentPage
    {
        string equipmentNo, latitude, longtitude;

        public EquipmentDetails(string eq)
        {
            InitializeComponent();

            equipmentNo = eq;

            GetEquipmentDetails();
        }

        void ShowOnMap()
        {
            if (!(String.IsNullOrEmpty(latitude)) && !(String.IsNullOrEmpty(longtitude)))
            {
                GoogleMap.IsVisible = true;
                switchChange.IsVisible = true;
                GoogleMap.IsShowingUser = false;

                double loc_latitude = Convert.ToDouble(latitude);
                double loc_longtitude = Convert.ToDouble(longtitude);

                if (loc_latitude == 0.00 || loc_longtitude == 0.00)
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

        void switchMap(object sender, ToggledEventArgs e)
        {
            if (changeMap.IsToggled)
            {
                GoogleMap.MapType = MapType.Satellite;
            }
            else
            {
                GoogleMap.MapType = MapType.Street;
            }
        }

        async void GetEquipmentDetails()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getEqDetail(equipmentNo));
            clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);


            if (equipment_response.IsGood)
            {
                var equipment = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                foreach (clsCaptionValue details in equipment)
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
                        equipmentDetails.Children.Add(stackLayout);
                    }

                    if (details.Caption.Equals("Reg No."))
                    {
                        Title = details.Value + " Details";
                    }

                    if (details.Caption.Equals("Latitude"))
                    {
                        latitude = details.Value;
                    }
                    if (details.Caption.Equals("Longitude"))
                    {
                        longtitude = details.Value;
                    }
                }

                ShowOnMap();
            }
            else
            {
                await DisplayAlert("JsonError", equipment_response.Message, "OK");
            }


        }
       
    }
}
