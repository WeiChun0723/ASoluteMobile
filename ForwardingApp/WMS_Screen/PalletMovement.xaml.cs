using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PalletMovement : ContentPage
    {
        string fieldName;

        public PalletMovement(string screenTitle)
        {
            InitializeComponent();

            Title = screenTitle;
        }

        void PalletIDScan(object sender, EventArgs e)
        {
            fieldName = "PalletIDScan";
            BarCodeScan(fieldName);
        }

        void ConfirmScan(object sender, EventArgs e)
        {
            fieldName = "ConfirmScan";
            BarCodeScan(fieldName);
        }

        void ConfirmAddPallet(object sender, EventArgs e)
        {
           GetPalletTrx(pdEntry.Text);
        }

        async void BarCodeScan(string field)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                await Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync();

                        if (field == "PalletIDScan")
                        {
                            pdEntry.Text = result.Text;
                            GetPalletTrx(result.Text);
                        }
                        else if (field == "ConfirmScan")
                        {
                            confirmEntry.Text = result.Text;
                        }
                      
                    });
                };
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        async void GetPalletTrx(string palletID)
        {
            try
            {
                var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getNewPalletTrx(palletID));
                clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(newPallet_response.IsGood)
                {
                    clsPalletTrx pallet = JObject.Parse(content)["Result"].ToObject<clsPalletTrx>();

                    if(String.IsNullOrEmpty(pallet.NewLocation))
                    {
                        //palletDesc.IsVisible = false;
                        confirm_icon.IsVisible = false;
                        //reasonBox.IsVisible = false;
                        suggestView.IsVisible = false;
                        confirmView.IsVisible = false;
                        checkDigitView.IsVisible = false;
                    }


                    suggestedEntry.Text = pallet.OldLocation;

                    string palletDesciptions = "";
                    int count = 0;
                    foreach(clsCaptionValue desc in pallet.Summary)
                    {
                        count++;
                        if(desc.Caption != "")
                        {
                            if(count == pallet.Summary.Count)
                            {
                                palletDesciptions += "\r\n" + "      " + desc.Caption + " : " + desc.Value + "\r\n";
                            }
                            else
                            {
                                palletDesciptions += "\r\n" + "      " + desc.Caption + " : " + desc.Value;
                            }

                        }


                    }

                    palletDesc.Text = palletDesciptions;
                }
                else
                {
                    await DisplayAlert("Error", newPallet_response.Message, "OK");
                }

            }
            catch
            {

            }
           
        }
    }
}
