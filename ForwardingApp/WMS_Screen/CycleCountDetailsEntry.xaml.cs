using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
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

        public CycleCountDetailsEntry(string id, string zone, string rack, string level)
        {
            InitializeComponent();

            cycleCountId = id;
            cycleCountZone = zone;
            cycleCountRack = rack;

            Task.Run(async () => { await LoadCycleCount(); });

            Title = "Cycle Count Detail";
        }

        async void Handle_Clicked(object sender, EventArgs e)
        {
            var button = sender as SfButton;
            switch (button.StyleId)
            {
                case "reverseBtn":
                    await LoadCycleCount();
                    break;

                case "confirmBtn":
                    if(emptyLocation.IsChecked == true || fullPallet.IsChecked == true || losseQty.IsChecked == true)
                    {
                        if(losseQty.IsChecked == true )
                        {
                            if(!(String.IsNullOrEmpty(loseQuantityEntry.Text)) && Convert.ToInt32(loseQuantityEntry.Text) > 0)
                            {
                                await PostCycleCount("Q");
                            }
                        }
                        else
                        {
                            if(emptyLocation.IsChecked == true)
                            {
                               await PostCycleCount("E");
                            }
                            else if (fullPallet.IsChecked == true)
                            {
                               await PostCycleCount("");
                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Missing field", "Please select at least 1 of the option.", "OK");
                    }
                    break;
            }
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            switch (entry.StyleId)
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
            switch (image.StyleId)
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

        async void Handle_Completed(object sender, System.EventArgs e)
        {
            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getPalletInquiryURL(cycleCountId), this);
            if (content != null)
            {
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                if (response.IsGood)
                {

                }
            }
        }

        private void CheckBox_StateChanged(object sender, StateChangedEventArgs e)
        {
            var checkBox = sender as SfCheckBox;

            switch (checkBox.StyleId)
            {
                case "emptyLocation":

                    if (emptyLocation.IsChecked == true)
                    {
                        fullPallet.IsChecked = false;
                        losseQty.IsChecked = false;
                    }
                    break;

                case "fullPallet":
                    if (fullPallet.IsChecked == true)
                    {
                        emptyLocation.IsChecked = false;
                        losseQty.IsChecked = false;
                    }
                    break;

                case "losseQty":
                    if (losseQty.IsChecked == true)
                    {
                        fullPallet.IsChecked = false;
                        emptyLocation.IsChecked = false;
                    }
                    break;

            }
        }

        async Task LoadCycleCount()
        {
            var reverse_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountLoadURL(cycleCountId, cycleCountZone, cycleCountRack, ascending), this);
            if (reverse_content != null)
            {
                clsResponse reverse_response = JsonConvert.DeserializeObject<clsResponse>(reverse_content);
                if (reverse_response.IsGood)
                {
                    ascending = (ascending == "false") ? "true" : "false";
                }
            }
        }

        async Task PostCycleCount(string countResult)
        {
            clsPalletTrx pallet = new clsPalletTrx
            {
                LinkId = cycleCountId
            };

            var content = await CommonFunction.CallWebService(1, pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountSaveURL(countResult), this);
            if (content != null)
            {
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                if (response.IsGood)
                {

                }
            }
        }
    }
}
