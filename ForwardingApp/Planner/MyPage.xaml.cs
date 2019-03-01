using System;
using System.Collections.Generic;

using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ASolute_Mobile.Planner
{
    public partial class MyPage : ContentPage
    {
        public MyPage()
        {
            InitializeComponent();

            GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(4.2105, 101.9758), Distance.FromKilometers(300)));

            var pin = new Pin
            {
                Position = new Position(2.9222, 101.6510),
                Label = "dpulze"

            };

            var pin1 = new Pin
            {
                Position = new Position(3.1179, 101.6778),
                Label = "midvalley"
            };

            var pin2 = new Pin
            {
                Position = new Position(3.1466, 101.6958),
                Label = "klcc"
            };
            var pin3 = new Pin
            {
                Position = new Position(3.1489, 101.7135),
                Label = "pavilion"
            };

            GoogleMap.Pins.Add(pin);
            GoogleMap.Pins.Add(pin1);
            GoogleMap.Pins.Add(pin2);
            GoogleMap.Pins.Add(pin3);

            GoogleMap.SelectedPin = pin;
            GoogleMap.SelectedPin = pin1;
            GoogleMap.SelectedPin = pin2;
            GoogleMap.SelectedPin = pin3;
        }
    }
}
