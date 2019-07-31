using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class CycleCountDetailsEntry : ContentPage
    {
        string cycleCountId, cycleCountZone, cycleCountRack, cycleCountLevel, ascending = "true";
        int cycleCountVer;
        clsPalletTrx pallet = new clsPalletTrx();

        public CycleCountDetailsEntry(string id, string zone, string rack, string level, int countVer)
        {
            InitializeComponent();

            cycleCountId = id;
            cycleCountZone = zone;
            cycleCountRack = rack;
            cycleCountLevel = level;
            cycleCountVer = countVer;

            LoadCycleCount();

            Title = "Cycle Count Detail";
        }

        async void Handle_Clicked(object sender, EventArgs e)
        {
            loading.IsVisible = true;

            var button = sender as SfButton;
            switch (button.StyleId)
            {
                case "reverseBtn":
                    LoadCycleCount();
                    break;

                case "confirmBtn":
                    if (emptyLocation.IsChecked == true || fullPallet.IsChecked == true || losseQty.IsChecked == true && (!(String.IsNullOrEmpty(locationEntry.Text))))
                    {
                        if (losseQty.IsChecked == true)
                        {
                            if (!(String.IsNullOrEmpty(palletIdEntry.Text)))
                            {
                                if (!(String.IsNullOrEmpty(loseQuantityEntry.Text)) && Convert.ToInt32(loseQuantityEntry.Text) > 0)
                                {

                                    PostCycleCount("Q");
                                }
                                else
                                {
                                    await DisplayAlert("Error", "Quantity must more than 0", "OK");
                                }
                            }
                            else
                            {
                                await DisplayAlert("Error", "Please enter pallet id.", "OK");
                            }
                        }
                        else
                        {
                            if (emptyLocation.IsChecked == true)
                            {
                                PostCycleCount("E");
                            }
                            else if (fullPallet.IsChecked == true)
                            {
                                if (!(String.IsNullOrEmpty(palletIdEntry.Text)))
                                {
                                    PostCycleCount("");
                                }
                                else
                                {
                                    await DisplayAlert("Error", "Please enter pallet id.", "OK");
                                }

                            }
                        }
                    }
                    else
                    {
                        await DisplayAlert("Missing field", "Please key in all mandatory field and select at least 1 of the option.", "OK");
                    }
                    break;
            }

            loading.IsVisible = false;
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
                            Device.BeginInvokeOnMainThread( () =>
                            {
                                Navigation.PopAsync();

                                palletIdEntry.Text = result.Text;

                                GetPallet();
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
            GetPallet();
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

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            palletIdContainer.ContainerBackgroundColor = Color.WhiteSmoke;
                        });
                    }
                    break;

                case "fullPallet":
                    if (fullPallet.IsChecked == true)
                    {
                        emptyLocation.IsChecked = false;
                        losseQty.IsChecked = false;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            palletIdContainer.ContainerBackgroundColor = Color.LightYellow;
                        });
                    }

                    break;

                case "losseQty":
                    if (losseQty.IsChecked == true)
                    {
                        fullPallet.IsChecked = false;
                        emptyLocation.IsChecked = false;

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            palletIdContainer.ContainerBackgroundColor = Color.LightYellow;
                        });
                    }
                    break;

            }
        }

        async void GetPallet()
        {
            try
            {

                loading.IsVisible = true;

                desc.Children.Clear();

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getPalletInquiryURL(palletIdEntry.Text), this);
                if (content != null)
                {
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                    if (response.IsGood)
                    {
                        pallet = JObject.Parse(content)["Result"].ToObject<clsPalletTrx>();

                        Label topBlank = new Label();
                        desc.Children.Add(topBlank);
                        foreach (clsCaptionValue summary in pallet.Summary)
                        {
                            Label caption = new Label();
                            caption.FontSize = 13;
                            if (summary.Caption.Equals(""))
                            {
                                caption.Text = "    " + summary.Value;
                            }
                            else
                            {
                                caption.Text = "    " + summary.Caption + ": " + summary.Value;
                            }

                            desc.Children.Add(caption);
                        }
                        Label bottomBlank = new Label();
                        desc.Children.Add(bottomBlank);
                    }

                    loading.IsVisible = false;
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }

        async void LoadCycleCount()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountLoadURL(cycleCountId, cycleCountZone, cycleCountRack, cycleCountLevel, ascending, cycleCountVer), this);
                if (content != null)
                {
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                    if (response.IsGood)
                    {
                        ascending = (ascending == "false") ? "true" : "false";

                        List<clsKeyValue> location = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                        if (location.Count != 0)
                        {
                            if (!(String.IsNullOrEmpty(location[0].Value)))
                            {
                                locationEntry.Text = location[0].Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        async void PostCycleCount(string countResult)
        {
            try
            {
                loading.IsVisible = true;

                clsPalletTrx confirmedPallet = new clsPalletTrx();
                confirmedPallet.LinkId = cycleCountId;
                confirmedPallet.NewLocation = locationEntry.Text;

                if (!(String.IsNullOrEmpty(palletIdEntry.Text)))
                {
                    confirmedPallet.Id = palletIdEntry.Text;
                    confirmedPallet.Quantity = (!(String.IsNullOrEmpty(loseQuantityEntry.Text))) ? Convert.ToInt32(loseQuantityEntry.Text) : 0;
                    confirmedPallet.CargoId = pallet.CargoId;
                }

                var content = await CommonFunction.CallWebService(1, confirmedPallet, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountSaveURL(countResult, cycleCountVer), this);
                if (content != null)
                {
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                    if (response.IsGood)
                    {
                        await DisplayAlert("Success", "Cycle count succeed.", "OK");

                        locationEntry.Text = "";
                        palletIdEntry.Text = "";
                        loseQuantityEntry.Text = "";
                        palletEntryCancel.IsVisible = false;
                        losseQuantityCancel.IsVisible = false;

                        desc.Children.Clear();

                        emptyLocation.IsChecked = false;
                        fullPallet.IsChecked = false;
                        losseQty.IsChecked = false;
                    }
                }

                loading.IsVisible = false;
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }

        }
    }
}
