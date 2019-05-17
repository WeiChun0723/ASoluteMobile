using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using XLabs.Forms.Controls;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PalletMovement : ContentPage
    {
        string fieldName;
        clsPalletTrx pallet;
        bool tapped = true;
        ListItems record = new ListItems();

        public PalletMovement(ListItems item)
        {
            InitializeComponent();

            record = item;

            Title = record.Name;

            chkScan.Checked = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            palletIdEntry.Focus();
        }

        public void Handle_Tapped(object sender, EventArgs e)
        {
            var image = sender as Image;

            switch(image.StyleId)
            {
                case "palletScan":
                    fieldName = "PalletIDScan";
                    BarCodeScan(fieldName);
                    break;

                case "confirmScan":
                    fieldName = "ConfirmScan";
                    BarCodeScan(fieldName);
                    break;

                case "palletEntryCancel":
                    palletIdEntry.Text = "";
                    palletIdEntry.Focus();
                    break;

                case "confirmEntryCancel":
                    confirmEntry.Text = "";
                    confirmEntry.Focus();
                    break;
            }
        }

        void Handle_Completed(object sender, System.EventArgs e)
        {
            var entry = sender as Entry;

            switch(entry.StyleId)
            {
                case "palletIdEntry":
                    GetPalletTrx(palletIdEntry.Text);
                    break;

                case "confirmEntry":
                    checkDigitEntry.Focus();
                    break;
            }

        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            var entry = sender as Entry;

            switch(entry.StyleId)
            {
                case "palletIdEntry":
                    if (!(String.IsNullOrEmpty(palletIdEntry.Text)))
                    {
                        palletEntryCancel.IsVisible = true;
                    }
                    else
                    {
                        palletEntryCancel.IsVisible = false;
                    }

                    if(chkScan.Checked)
                    {
                        if(!(String.IsNullOrEmpty(palletIdEntry.Text)))
                        {

                            GetPalletTrx(palletIdEntry.Text);
                        }

                    }

                    break;

                case "confirmEntry":
                    if (!(String.IsNullOrEmpty(confirmEntry.Text)))
                    {
                        confirmEntryCancel.IsVisible = true;
                    }
                    else
                    {
                        confirmEntryCancel.IsVisible = false;
                    }

                    break;
            }
        }

        void Handle_CheckedChanged(object sender, XLabs.EventArgs<bool> e)
        {
            var chkBox = sender as CheckBox;

            switch (chkBox.StyleId)
            {
                case "chkScan":
                    if (chkManual.Checked && chkScan.Checked)
                    {
                        chkManual.Checked = false;
                        palletIdEntry.Focus();
                    }

                    break;

                case "chkManual":
                    if (chkScan.Checked && chkManual.Checked)
                    {
                        chkScan.Checked = false;
                        palletIdEntry.Focus();
                    }
                    break;
            }
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as SfButton;

            switch(button.StyleId)
            {
                case "btnVerify":
                    var content = await CommonFunction.CallWebService(1, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getPalletVerificationURL(record.Id, palletIdEntry.Text), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if(response.IsGood)
                    {
                        // see whether web service got return stuff
                        await DisplayAlert("Success", "Pallet verified", "OK");

                        palletIdEntry.Text = "";
                        palletDesc.Children.Clear();
                    }
                    break;

                case "btnDropOff":
                case "btnPickUp":
                    PalletUpdate();
                    break;
            }
        }

        async void PalletUpdate()
        {
            loading.IsVisible = true;
            try
            {
                if (!(String.IsNullOrEmpty(palletIdEntry.Text)))
                {
                    pallet.NewLocation = !(String.IsNullOrEmpty(confirmEntry.Text)) ? confirmEntry.Text : "";
                    if (!(String.IsNullOrEmpty(checkDigitEntry.Text)))
                    {
                        pallet.CheckDigit = Convert.ToInt32(checkDigitEntry.Text);
                    }

                    var content = await CommonFunction.CallWebService(1,pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPalletTrxURL(),this);
                    clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (update_response.IsGood)
                    {

                        if (pallet.Action.Equals("Pickup"))
                        {
                            GetPalletTrx(palletIdEntry.Text);
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

                            palletIdEntry.Text = String.Empty;
                            confirmEntry.Text = String.Empty;
                            checkDigitEntry.Text = String.Empty;
                        }

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
                                palletIdEntry.Text = result.Text;
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
                if(palletIdEntry.IsFocused)
                {
                    palletIdEntry.Unfocus();
                }

                string encodePalletId = System.Net.WebUtility.UrlEncode(palletID);

                var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getPalletInquiryURL(encodePalletId),this);
                clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(newPallet_response.IsGood)
                {
                    pallet = JObject.Parse(content)["Result"].ToObject<clsPalletTrx>();

                    palletDesc.IsVisible = true;

                    Title = (pallet.Action == "Pickup" || pallet.Action == "Dropoff") ? "Putaway" : record.Name;

                    switch(pallet.Action)
                    {
                        case "Pickup":
                            btnPickUp.IsVisible = true;
                            btnDropOff.IsVisible = false;
                            suggestView.IsVisible = false;
                            confirmView.IsVisible = false;
                            checkDigitView.IsVisible = false;
                            break;

                        case "Dropoff":
                            if (!String.IsNullOrEmpty(pallet.NewLocation))
                            {
                                btnPickUp.IsVisible = false;
                                btnDropOff.IsVisible = true;
                                suggestView.IsVisible = true;
                                confirmView.IsVisible = true;
                                checkDigitView.IsVisible = true;
                                suggestedEntry.Text = pallet.NewLocation;

                                confirmEntry.Focus();
                            }
                            break;

                        default:
                            btnVerify.IsVisible = true;
                            break;
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
               
                loading.IsVisible = false;
            }
            catch
            {

            }
           
        }
    }
}
