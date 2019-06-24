using System;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;
using Newtonsoft.Json;
using ASolute_Mobile.Utils;
using ASolute.Mobile.Models;
using ZXing.Net.Mobile.Forms;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ASolute_Mobile.LGC
{

    public partial class ChinaReceiving : ContentPage
    {
        int maxWeight, minLength, minWidth, maxDimension;

        public ChinaReceiving()
        {
            InitializeComponent();

            Title = "China Receiving";

            GetParcelRules();
        }

        async void GetParcelRules()
        {
            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getParcelRulesURL(), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response.IsGood == true)
            {
                var rules = JObject.Parse(content)["Result"].ToObject<List<clsKeyCaptionValue>>();

                string allRule = "";

                foreach (clsKeyCaptionValue rule in rules)
                {
                    allRule += rule.Caption + ": " + rule.Value + "\r\n";

                    switch(rule.Key)
                    {
                        case "MinLength":
                            minLength = Convert.ToInt16(rule.Value) ;
                            break;

                        case "MinWidth":
                            minWidth = Convert.ToInt16(rule.Value);
                            break;

                        case "MaxDimension":
                            maxDimension = Convert.ToInt16(rule.Value);
                            break;

                        case "MaxWeight":
                            maxWeight = Convert.ToInt16(rule.Value);
                            break;
                    }

                }

                rulesDesc.Text = allRule;
            }
           
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            var icon = sender as Image;

            var scanPage = new ZXingScannerPage();
            await Navigation.PushAsync(scanPage);

            scanPage.OnScanResult += (result) =>
            {
                Device.BeginInvokeOnMainThread(() =>
               {
                   Navigation.PopAsync();

                   switch (icon.StyleId)
                   {
                       case "consigNote_icon":
                           consigNote.Text = result.Text;
                           break;

                       case "cartonBox_icon":
                           cartonBox.Text = result.Text;
                           break;
                   }
               });
            };
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;

            var button = sender as SfButton;

            switch (button.StyleId)
            {
                case "btnNext":
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.validateConsingmentNoteURL(consigNote.Text), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (response.IsGood == true)
                    {
                        recevingStack.IsVisible = true;
                    }
                    break;

                case "btnSubmit":

                    if ((!String.IsNullOrEmpty(consigNote.Text)) && (!String.IsNullOrEmpty(cartonBox.Text)) && (!String.IsNullOrEmpty(length.Text)) && (!String.IsNullOrEmpty(width.Text)) &&
                        (!String.IsNullOrEmpty(height.Text)) && (!String.IsNullOrEmpty(unit.Text)))
                    {
                        if(Convert.ToInt16(length.Text) >= minLength && Convert.ToInt16(width.Text) >= minWidth && (Convert.ToInt16(length.Text) * Convert.ToInt16(width.Text) * Convert.ToInt16(height.Text) <= maxDimension)
                            && Convert.ToInt16(unit.Text) <= maxWeight)
                        {
                            if (Convert.ToDouble(unit.Text) > 999)
                            {
                                await DisplayAlert("Error", "Weight cannot more than 999", "OK");
                            }
                            else
                            {
                                clsParcelModel parcelModel = new clsParcelModel
                                {
                                    ConsignmentNo = consigNote.Text,
                                    CartonNo = cartonBox.Text,
                                    Length = Convert.ToInt16(length.Text),
                                    Width = Convert.ToInt16(width.Text),
                                    Height = Convert.ToInt16(height.Text),
                                    Weight = Convert.ToDouble(unit.Text),
                                    Volume = Convert.ToDouble(M3.Text)
                                };

                                var submit_content = await CommonFunction.CallWebService(1, parcelModel, Ultis.Settings.SessionBaseURI, ControllerUtil.postReceiveAndUpdate(), this);
                                clsResponse submit_response = JsonConvert.DeserializeObject<clsResponse>(submit_content);

                                if (submit_response.IsGood == true)
                                {
                                    await DisplayAlert("Success", "Submit successfully.", "OK");

                                    consigNote.Text = String.Empty;
                                    length.Text = String.Empty;
                                    width.Text = String.Empty;
                                    height.Text = String.Empty;
                                    M3.Text = String.Empty;
                                    unit.Text = String.Empty;
                                    cartonBox.Text = String.Empty;

                                    recevingStack.IsVisible = false;

                                    consigNote.Focus();
                                }
                            }
                        }
                        else
                        {
                            await DisplayAlert("Error", "Please follow the rules given above.", "OK");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Missing Field", "Please key in all field", "OK");
                    }

                    break;
            }

            loading.IsVisible = false;
        }

        async void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            try
            {
                if ((!String.IsNullOrEmpty(length.Text)) && (!String.IsNullOrEmpty(width.Text)) && (!String.IsNullOrEmpty(height.Text)))
                {
                    int l = Convert.ToInt16(length.Text);
                    int w = Convert.ToInt16(width.Text);
                    int h = Convert.ToInt16(height.Text);

                    decimal m3 = Decimal.Divide(l * w * h, 1000000);

                    M3.Text = String.Format("{0:0.000}", m3);
                }
            }
            catch
            {

            }

        }
    }
}
