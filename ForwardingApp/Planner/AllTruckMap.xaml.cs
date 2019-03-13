using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Maps;


namespace ASolute_Mobile.Planner
{
    public partial class AllTruckMap : ContentPage
    {
        List<clsTruckingModel> equipmentList;
        List<Pin> pins = new List<Pin>();

        public AllTruckMap(string title)
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

            Task.Run(async () => {await GetAllEquipmentList(); }).Wait();
        }

        async Task GetAllEquipmentList()
        {
            loading.IsVisible = true;

             var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getAllEq());
             clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);

             if (equipment_response.IsGood)
             {
                 equipmentList = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();

                var latitudes = new List<double>();
                var longitudes = new List<double>();

                foreach (clsTruckingModel equipment in equipmentList)
                {
                    if (equipment.Latitude != "")
                    {

                        var pin = new Pin
                        {
                            Position = new Position(Convert.ToDouble(equipment.Latitude), Convert.ToDouble(equipment.Longitude)),
                            Label = equipment.TruckId ,

                        };

                        latitudes.Add(Convert.ToDouble(equipment.Latitude));
                        longitudes.Add(Convert.ToDouble(equipment.Longitude));


                        string detail = "";
  
                        foreach (clsCaptionValue details in equipment.Details)
                        {
                            if(details.Caption.Equals("Engine") || details.Caption.Equals("Speed (Km)") )
                           {
                                    detail += details.Caption + ": " + details.Value + "\r\n";
                           }
                            else if( details.Caption.Equals("Odometer"))
                            {
                                detail += details.Caption + ": " + details.Value;
                            }

                        }

                        pin.Address  = detail;
                        pin.Clicked += Pin_Clicked2;
                        pins.Add(pin);

                    }
                }

                GoogleMap.MapPins = pins;

                foreach(Pin pin in pins)
                {
                    GoogleMap.Pins.Add(pin);
                }

                double lowestLat = latitudes.Min();
                double highestLat = latitudes.Max();
                double lowestLong = longitudes.Min();
                double highestLong = longitudes.Max();
                double finalLat = (lowestLat + highestLat) / 2;
                double finalLong = (lowestLong + highestLong) / 2;
                double distance = DistanceCalculation.GeoCodeCalc.CalcDistance(lowestLat, lowestLong, highestLat, highestLong, DistanceCalculation.GeoCodeCalcMeasurement.Kilometers);

                GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(finalLat, finalLong), Distance.FromKilometers(distance)));
            }

            loading.IsVisible = false;
        }

       async void Pin_Clicked2(object sender, EventArgs e)
        {
            var p = sender as Pin;

            await Navigation.PushAsync(new EquipmentDetails("", equipmentList.Find(item => item.TruckId == p.Label))); 

        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        async void Handle_Refreshing(object sender, System.EventArgs e)
        {
            await GetAllEquipmentList();
            //pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            //pullToRefresh.IsRefreshing = false;
        }
    }

    public class DistanceCalculation
    {
        public static class GeoCodeCalc
        {
            public const double EarthRadiusInMiles = 3956.0;
            public const double EarthRadiusInKilometers = 6367.0;

            public static double ToRadian(double val) { return val * (Math.PI / 180); }
            public static double DiffRadian(double val1, double val2) { return ToRadian(val2) - ToRadian(val1); }

            public static double CalcDistance(double lat1, double lng1, double lat2, double lng2)
            {
                return CalcDistance(lat1, lng1, lat2, lng2, GeoCodeCalcMeasurement.Miles);
            }

            public static double CalcDistance(double lat1, double lng1, double lat2, double lng2, GeoCodeCalcMeasurement m)
            {
                double radius = GeoCodeCalc.EarthRadiusInMiles;

                if (m == GeoCodeCalcMeasurement.Kilometers) { radius = GeoCodeCalc.EarthRadiusInKilometers; }
                return radius * 2 * Math.Asin(Math.Min(1, Math.Sqrt((Math.Pow(Math.Sin((DiffRadian(lat1, lat2)) / 2.0), 2.0) + Math.Cos(ToRadian(lat1)) * Math.Cos(ToRadian(lat2)) * Math.Pow(Math.Sin((DiffRadian(lng1, lng2)) / 2.0), 2.0)))));
            }
        }

        public enum GeoCodeCalcMeasurement 
        {
            Miles = 0,
            Kilometers = 1
        }
    }
}
