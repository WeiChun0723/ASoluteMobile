using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace ASolute_Mobile.Planner
{
    public partial class AllTruckMap : ContentPage
    {
        public AllTruckMap(string title)
        {
            InitializeComponent();

            GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(4.2105, 101.9758), Distance.FromKilometers(300)));

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
                Text = Ultis.Settings.SessionUserItem.DriverId + " , " + Ultis.Settings.SessionUserItem.CompanyName,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await GetAllEquipmentList();
        }

        async Task GetAllEquipmentList()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getAllEq());
            clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (equipment_response.IsGood)
            {
                List<clsTruckingModel> equipmentList = JObject.Parse(content)["Result"].ToObject<List<clsTruckingModel>>();

                foreach(clsTruckingModel equipment in equipmentList)
                {
                    if(equipment.Latitude != "")
                    {
                        var pin = new Pin
                        {
                            Position = new Position(Convert.ToDouble(equipment.Latitude), Convert.ToDouble(equipment.Longitude)),
                            Label = equipment.TruckId

                        };

                        GoogleMap.Pins.Add(pin);
                    }

                }
            }

            loading.IsVisible = false;
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
}
