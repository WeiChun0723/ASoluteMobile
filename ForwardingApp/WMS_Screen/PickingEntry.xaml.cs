using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        int index = 0, count = 0;

        public PickingEntry(clsWhsItem pickingSummary, string id, string title, List<clsWhsItem> items)
        {
            InitializeComponent();

            linkID = id;
            pickingTitle = title;
            clsWhsItems = items;

            //find the item index in the list
            index = items.FindIndex(a => a.PalletId == pickingSummary.PalletId);

            LoadPickingSummary(pickingSummary.Summary);
        }

        void LoadPickingSummary(List<clsCaptionValue> values)
        {
            pickingDesc.Children.Clear();

            Label topBlank = new Label();
            pickingDesc.Children.Add(topBlank);

            foreach (clsCaptionValue desc in values)
            {
                Label caption = new Label();
                caption.FontSize = 13;

                if (desc.Caption.Equals(""))
                {
                    caption.Text = "      " + desc.Value;
                }
                else
                {
                    caption.Text = "      " + desc.Caption + ": " + desc.Value;
                }

                if (desc.Caption.Equals("Pallet"))
                {
                    Title = pickingTitle + " # " + desc.Value;
                }

                pickingDesc.Children.Add(caption);
            }

            Label bottomBlank = new Label();
            pickingDesc.Children.Add(bottomBlank);
        }

        void ScanBarCode(object sender, EventArgs e)
        {
            var image = sender as Image;

            if (image.StyleId.Equals("confirmScan"))
            {
                fieldName = "ConfirmScan";
                BarCodeScan(fieldName);
            }
            else if (image.StyleId.Equals("palletScan"))
            {
                fieldName = "PalletIDScan";
                BarCodeScan(fieldName);
            }
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
                            palletIDEntry.Text = result.Text;

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

        async void ConfirmPicking(object sender, EventArgs e)
        {
            clsPalletTrx pallet = new clsPalletTrx
            {
                LinkId = linkID,
                OldLocation = confirmEntry.Text,
                Id = palletIDEntry.Text,
                CheckDigit = Convert.ToInt32(checkDigitEntry.Text)  
            };

            var content = await CommonFunction.PostRequestAsync(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postPickingDetail());
            clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (update_response.IsGood)
            {
                await DisplayAlert("Success", "Picking task updated", "OK");

                try
                {
                    checkDigitEntry.Text = String.Empty;
                    palletIDEntry.Text = String.Empty;
                    confirmEntry.Text = String.Empty;

                    index++;
                    count++;

                    if (this.Title.Contains("Loose Pick"))
                    {
                        genButton.IsVisible = true;
                        genButton.Text = "Items : " + count;
                    }

                    LoadPickingSummary(clsWhsItems[index].Summary);
                }
                catch
                {
                    await DisplayAlert("Finish", "This is the last items in the list.", "OK");
                    //await Navigation.PopAsync();
                }
            }
            else
            {
                await DisplayAlert("Error", update_response.Message, "OK");
            }
        }

        async void GenPallet_Clicked(object sender, EventArgs e)
        {
            var content = await CommonFunction.PostRequestAsync(null,Ultis.Settings.SessionBaseURI, ControllerUtil.generatePallet(linkID));
            clsResponse genPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(genPallet_response.IsGood)
            {
                await DisplayAlert("Success", "Pallet generated.", "OK");
                await Navigation.PopAsync();
            }
            else
            {
                await DisplayAlert("Error", genPallet_response.Message, "OK");
            }
        }
    }
}
