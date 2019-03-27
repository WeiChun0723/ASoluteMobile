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
        clsPalletTrx pallet;
        bool tapped = true;

        public PalletMovement(string screenTitle)
        {
            InitializeComponent();

            Title = screenTitle;

            pdEntry.Completed += PdEntry_Completed;
        }

        void PdEntry_Completed(object sender, EventArgs e)
        {
            GetPalletTrx(pdEntry.Text);
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

        void PickUpPallet(object sender, System.EventArgs e)
        {
            PalletUpdate();
        }

        void DropUpPallet(object sender, EventArgs e)
        {

            PalletUpdate();
        }

        void ClearPdEntry(object sender, System.EventArgs e)
        {
            pdEntry.Text = "";
        }

        void PdEntry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!(String.IsNullOrEmpty(pdEntry.Text)))
            {
                pdCancel.IsVisible = true;
            }
            else
            {
                pdCancel.IsVisible = false;
            }
        }

        void ClearConfirmEntry(object sender, System.EventArgs e)
        {
            confirmEntry.Text = "";
        }

        void ConfirmEntry_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if (!(String.IsNullOrEmpty(pdEntry.Text)))
            {
                confirmCancel.IsVisible = true;
            }
            else
            {
                confirmCancel.IsVisible = false;
            }
        }

        async void PalletUpdate()
        {
            loading.IsVisible = true;
            try
            {
                if (!(String.IsNullOrEmpty(pdEntry.Text)))
                {
                    pallet.NewLocation = !(String.IsNullOrEmpty(confirmEntry.Text)) ? confirmEntry.Text : "";
                    if (!(String.IsNullOrEmpty(checkDigitEntry.Text)))
                    {
                        pallet.CheckDigit = Convert.ToInt32(checkDigitEntry.Text);
                    }

                    var content = await CommonFunction.PostRequestAsync(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPalletTrx());
                    clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (update_response.IsGood)
                    {

                        if (pallet.Action.Equals("Pickup"))
                        {
                            GetPalletTrx(pdEntry.Text);
                        }
                        else if (pallet.Action.Equals("Dropoff"))
                        {
                            await DisplayAlert("Success", "Putaway updated.", "OK");

                            btnPickUp.IsVisible = false;
                            btnDropOff.IsVisible = false;
                            suggestView.IsVisible = false;
                            confirmView.IsVisible = false;
                            checkDigitView.IsVisible = false;
                            palletDesc.IsVisible = false;

                            pdEntry.Text = String.Empty;
                            confirmEntry.Text = String.Empty;
                            checkDigitEntry.Text = String.Empty;
                        }

                    }
                    else
                    {
                        await DisplayAlert("Error", update_response.Message, "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Missing field", "Please key in all mandatory field.", "OK");
                }

                loading.IsVisible = false;
            }
            catch(Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        async void BarCodeScan(string field)
        {

            if(tapped)
            {
                tapped = false;

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

                    tapped = true;
                }
                catch (Exception e)
                {
                    await DisplayAlert("Error", e.Message, "OK");
                }

            }

        }

        async void GetPalletTrx(string palletID)
        {
            loading.IsVisible = true;
            try
            {
                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getPalletInquiry(palletID));
                clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(newPallet_response.IsGood)
                {
                    pallet = JObject.Parse(content)["Result"].ToObject<clsPalletTrx>();

                    palletDesc.IsVisible = true;
                    Title = "Putaway";

                    if(pallet.Action.Equals("Pickup"))
                    {
                        btnPickUp.IsVisible = true;
                        btnDropOff.IsVisible = false;
                        suggestView.IsVisible = false;
                        confirmView.IsVisible = false;
                        checkDigitView.IsVisible = false;
                    }
                    else if (pallet.Action.Equals("Dropoff"))
                    {
                        if(!String.IsNullOrEmpty(pallet.NewLocation))
                        {
                            btnPickUp.IsVisible = false;
                            btnDropOff.IsVisible = true;
                            suggestView.IsVisible = true;
                            confirmView.IsVisible = true;
                            checkDigitView.IsVisible = true;
                            suggestedEntry.Text = pallet.NewLocation;
                        } 
                    }

                    palletDesc.Children.Clear();

                    Label blank = new Label
                    {
                        Text = ""
                    };
                    palletDesc.Children.Add(blank);
                    foreach (clsCaptionValue desc in pallet.Summary)
                    {
                        if(desc.Caption != "")
                        {
                            Label caption = new Label
                            {
                                FontSize = 13,
                                                            
                            };

                            caption.Text =  "    " + desc.Caption + ": " + desc.Value;
                        
                            palletDesc.Children.Add(caption);
                        }
                    }
                    Label btmblank = new Label
                    {
                        Text = ""
                    };
                    palletDesc.Children.Add(btmblank);

                }
                else
                {
                    await DisplayAlert("Error", newPallet_response.Message, "OK");
                }

                loading.IsVisible = false;
            }
            catch
            {

            }
           
        }
    }
}
