using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.LGC
{
    public partial class OutFromChina : ContentPage
    {
        ObservableCollection<Shipment> shipments = new ObservableCollection<Shipment>(); 

        public OutFromChina()
        {
            InitializeComponent();

            Title = "Out From China";
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var scanPage = new ZXingScannerPage();
            await Navigation.PushAsync(scanPage);

            scanPage.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    Navigation.PopAsync();

                    cartonBox.Text = result.Text;
                });
            };
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;
            try
            {
                if((!String.IsNullOrEmpty(shipmentRef.Text)) && (!String.IsNullOrEmpty(cartonBox.Text)))
                {
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.updateShipment(shipmentRef.Text, cartonBox.Text), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if(response.IsGood == true)
                    {
                        await DisplayAlert("Success", "Successfully update shipment.", "OK");

                        Shipment shipment = new Shipment
                        {
                            ShipmentNo = "Shipment Ref: " + shipmentRef.Text,
                            CartonBox = "Carton Box: " + cartonBox.Text
                        };

                        shipments.Add(shipment);

                        shipmentList.RowHeight = 100;
                        shipmentList.ItemsSource = shipments;

                        cartonBox.Text = String.Empty;

                    }
                }
                else
                {
                    await DisplayAlert("Missing field", "Please fill in all mandatory field", "OK");
                }
            }
            catch
            {

            }

            loading.IsVisible = false;
        }
    }

    class Shipment
    {
        public string ShipmentNo { get; set; }
        public string CartonBox { get; set; }
    }
}
