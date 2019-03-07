using System;
using System.Collections.Generic;
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

                foreach (clsTruckingModel equipment in equipmentList)
                {
                    if (equipment.Latitude != "")
                    {

                        var pin = new Pin
                        {
                            Position = new Position(Convert.ToDouble(equipment.Latitude), Convert.ToDouble(equipment.Longitude)),
                            Label = equipment.TruckId ,

                        };

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

                GoogleMap.MoveToRegion(MapSpan.FromCenterAndRadius(new Position(4.2105, 101.9758), Distance.FromKilometers(300)));
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

         void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetAllEquipmentList();
            //pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            //pullToRefresh.IsRefreshing = false;
        }


    }
}
