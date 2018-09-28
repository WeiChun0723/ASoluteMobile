using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class ContainerDetails : ContentPage
    {
        string provider_code, provider_container, latitude, longtitude;
        

        public ContainerDetails(string code, string container)
        {
            InitializeComponent();
            provider_code = code;
            provider_container = container;
            GetContainerDetail();

        }

        void ShowOnMap()
        {
            if(!(String.IsNullOrEmpty(latitude)) && !(String.IsNullOrEmpty(longtitude)))
            {
                double loc_latitude = Convert.ToDouble(latitude);
                double loc_longtitude = Convert.ToDouble(longtitude);

                GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(loc_latitude, loc_longtitude), Distance.FromMeters(100)));

                var pin = new Pin
                {
                    Position = new Position(loc_latitude, loc_longtitude),
                    Label = "Container Location"
                };

                GoogleMap.Pins.Add(pin);
            }
        
        }

        public async void GetContainerDetail()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerDetail(provider_code, provider_container));
            clsResponse container_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(container_response.IsGood)
            {
                var containers = JObject.Parse(content)["Result"].ToObject<clsDataRow>();

                foreach (clsCaptionValue details in containers.Details)
                {
                    if(details.Display == true)
                    {
                        Label label = new Label();
                        label.Text = details.Caption + ":  " + details.Value;
                        label.FontAttributes = FontAttributes.Bold;
                                     
                        containerDetails.Children.Add(label);
                    }

                    if(details.Caption.Equals("Container No."))
                    {
                        Title = details.Value;
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
                await DisplayAlert("JsonError", container_response.Message, "OK");
            }
        }
    }
}
