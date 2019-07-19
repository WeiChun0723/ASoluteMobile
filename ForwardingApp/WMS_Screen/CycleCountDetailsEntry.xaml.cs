using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class CycleCountDetailsEntry : ContentPage
    {
        string cycleCountId, cycleCountZone, cycleCountRack, ascending = "true";
       
        public CycleCountDetailsEntry(string id, string zone, string rack , string level)
        {
            InitializeComponent();
            
            cycleCountId = id;
            cycleCountZone = zone;
            cycleCountRack = rack;

            Title = "Cycle Count Detail";

            locationEntry.Text = zone + "-" + rack + "-" + level;
        }

        async void Handle_Clicked(object sender, EventArgs e)
        {
            var button = sender as SfButton;
            switch(button.StyleId)
            {
                case "reverseBtn":
                    var reverse_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountLoadURL(cycleCountId, cycleCountZone, cycleCountRack,ascending), this);
                    if (reverse_content != null)
                    {
                        clsResponse reverse_response = JsonConvert.DeserializeObject<clsResponse>(reverse_content);
                        if(reverse_response.IsGood)
                        {
                            ascending = (ascending == "false") ? "true" : "false";
                        }
                    }
                    break;

                case "confirmBtn":
                    
                    break;
            }
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            switch(entry.StyleId)
            {
                case "palletIdEntry":
                    palletEntryCancel.IsVisible = true;
                    break;

                case "loseQuantityEntry":
                    losseQuantityCancel.IsVisible = true;
                    break;
                    
            }
        }

        async void Handle_Tapped(object sender, EventArgs e)
        {
            var image = sender as Image;
            switch(image.StyleId)
            {
                case "palletScan":
                    try
                    {
                        var scanPage = new ZXingScannerPage();
                        await Navigation.PushAsync(scanPage);

                        scanPage.OnScanResult += (result) =>
                        {
                            Device.BeginInvokeOnMainThread(async () =>
                            {
                                await Navigation.PopAsync();

                                palletIdEntry.Text = result.Text;
                            });
                        };
                      
                    }
                    catch (Exception ex)
                    {
                        await DisplayAlert("Error", ex.Message, "OK");
                    }
                    break;

                case "palletEntryCancel":
                    palletIdEntry.Text = "";
                    palletEntryCancel.IsVisible = false;
                    break;

                case "losseQuantityCancel":
                    loseQuantityEntry.Text = "";
                    losseQuantityCancel.IsVisible = false;
                    break;
            }
        }

        void Handle_Completed(object sender, System.EventArgs e)
        {

        }
    }
}
