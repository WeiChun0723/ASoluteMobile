using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PickingEntry : ContentPage
    {
        List<clsWhsItem> clsWhsItems = new List<clsWhsItem>();
        string fieldName, linkID, pickingTitle;
        bool tapped = true;

        public PickingEntry(clsWhsItem summary, string id, string title, List<clsWhsItem> items)
        {
            InitializeComponent();

            linkID = id;
            pickingTitle = title;
            pickingDesc.Children.Clear();

            Label topBlank = new Label();
            pickingDesc.Children.Add(topBlank);

            clsWhsItems = items;

            foreach (clsCaptionValue desc in summary.Summary)
            {
                Label caption = new Label();
                caption.FontSize = 13;
              
                if(desc.Caption.Equals(""))
                {
                    caption.Text = "      " +  desc.Value;
                }
                else
                {
                    caption.Text = "      " + desc.Caption + ": " + desc.Value;
                }

                if(desc.Caption.Equals("Pallet"))
                {

                    Title = pickingTitle + " # " + desc.Value;
                }

                pickingDesc.Children.Add(caption);
            }

            Label bottomBlank = new Label();
            pickingDesc.Children.Add(bottomBlank);

        }

        async void PalletIDScan(object sender, EventArgs e)           
        {
            fieldName = "PalletIDScan";
            BarCodeScan(fieldName);
        }

        void ConfirmScan(object sender, EventArgs e)
        {
            fieldName = "ConfirmScan";
            BarCodeScan(fieldName);
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
                                palletIDEntry.Text = result.Text;

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

        async void ConfirmPicking(object sender, EventArgs e)
        {
            clsPalletTrx pallet = new clsPalletTrx
            {
                LinkId = linkID,
                OldLocation = confirmEntry.Text,
                Id = palletIDEntry.Text,
                CheckDigit = Convert.ToInt32(checkDigitEntry.Text)
            };

            var content = await CommonFunction.PostRequest(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postPickingDetail());
            clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(update_response.IsGood)
            {
                await DisplayAlert("Success", "Picking task updated", "OK");

                //Navigation.RemovePage(Navigation.NavigationStack[Navigation.NavigationStack.Count - 1]);

                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", update_response.Message, "OK");
            }
        }
    }
}
