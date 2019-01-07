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
    public partial class TallyInPalletEntry : ContentPage
    {
        clsWhsSummary productPallet = new clsWhsSummary();
        string id,fieldName;
        clsPalletNew palletList;
        List<string> size = new List<string>();
        List<string> unit = new List<string>();
        List<string> status = new List<string>();

        public TallyInPalletEntry(clsWhsSummary product, string tallyInID, string screenTitle)
        {
            InitializeComponent();

            id = tallyInID;

            productPallet = product;

            Title = "Tally In # " + screenTitle;

            datePicker.MinimumDate = DateTime.Now;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            palletDesc.Text = productPallet.Description;

            GetNewPalletList();
        }

        async void GetNewPalletList()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.loadNewPallet(id, productPallet.ProductLinkId));
            clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(newPallet_response.IsGood)
            {
                palletList = JObject.Parse(content)["Result"].ToObject<clsPalletNew>();

                foreach(clsKeyValue sizes in palletList.PalletSize)
                {
                    size.Add(sizes.Key);
                }

                foreach (clsKeyValue units in palletList.ProductUom)
                {
                    unit.Add(units.Key);
                }

                foreach (clsKeyValue status_ in palletList.StockStatus)
                {
                    status.Add(status_.Key);
                }

                sizeBox.ComboBoxSource = size;
                unitBox.ComboBoxSource = unit;
                statusBox.ComboBoxSource = status;

                statusBox.Text = palletList.DefaultStockStatus;

            }
        }

        void Size_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {
            foreach(clsKeyValue selectedSize in palletList.PalletSize)
            {
                if(sizeBox.Text.Equals(selectedSize.Key))
                {
                    palletTI.Text = selectedSize.Extra01;
                    palletHI.Text = selectedSize.Extra02;
                }
            }
        }

        void PalletScan(object sender, EventArgs e)
        {
            fieldName = "PalletScan";
            BarCodeScan(fieldName);
        }

        void BatchScan(object sender, EventArgs e)
        {
            fieldName = "BatchScan";
            BarCodeScan(fieldName);
        }

        void ExpiryDateScan(object sender, EventArgs e)
        {
            fieldName = "ExpiryScan";
            BarCodeScan(fieldName);

        }

        void DateEntry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {
            datePicker.Focus();
            date.Unfocus();
        }

        void DatePicker_DateSelected(object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            date.Text = datePicker.Date.ToString("dd/MM/yyyy");
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

                        if (field == "PalletScan")
                        {
                            palletNo.Text = result.Text;

                        }
                        else if (field == "BatchScan")
                        {

                            batchNo.Text = result.Text;
                        }
                        else if (field == "ExpiryScan")
                        {
                            DateTime enteredDate = DateTime.Parse(result.Text);

                            datePicker.Date = enteredDate;
                          
                        }
                    });
                };
            }
            catch(Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

      

        async void ConfirmAddPallet(object sender, EventArgs e)
        {
            try
            {
                if (!(String.IsNullOrEmpty(palletNo.Text)) && !(String.IsNullOrEmpty(palletHI.Text)) && !(String.IsNullOrEmpty(palletTI.Text)) && !(String.IsNullOrEmpty(quantity.Text)) && !(String.IsNullOrEmpty(sizeBox.Text))
             && !(String.IsNullOrEmpty(statusBox.Text)) && !(String.IsNullOrEmpty(unitBox.Text)))
                {
                    clsPallet pallet = new clsPallet
                    {
                        Id = id,
                        ProductCode = productPallet.ProductCode,
                        PalletId = palletNo.Text,
                        PalletSize = sizeBox.Text,
                        PalletTI = Convert.ToInt16(palletTI.Text),
                        PalletHI = Convert.ToInt16(palletHI.Text),
                        Qty = Convert.ToInt32(quantity.Text),
                        Uom = unitBox.Text,
                        StockStatus = statusBox.Text,
                        String01 = (!(String.IsNullOrEmpty(batchNo.Text))) ? batchNo.Text : String.Empty,
                      
                    };

                    pallet.ExpiryDate = (datePicker.Date.ToString("yyyy-MM-dd") == DateTime.Now.Date.ToString("yyyy-MM-dd")) ? String.Empty : datePicker.Date.ToString("yyyy-MM-dd");
  
                    var content = await CommonFunction.PostRequest(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPallet(id));
                    clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (upload_response.IsGood)
                    {
                        await DisplayAlert("Success", "New pallet added.", "OK");
                        palletNo.Text = String.Empty;
                        sizeBox.Text = String.Empty;
                        palletTI.Text = String.Empty;
                        palletHI.Text = String.Empty;
                        quantity.Text = String.Empty;
                        unitBox.Text = String.Empty;
                        statusBox.Text = palletList.DefaultStockStatus; 
                        batchNo.Text = String.Empty;
                        //datePicker.Date = Convert.ToDateTime("");
                    }
                    else
                    {
                        await DisplayAlert("Error", upload_response.Message, "OK");
                    }

                }
                else
                {
                    await DisplayAlert("Missing field", "Please fill in all mandatory field.", "OK");
                }
            }
            catch(Exception error)
            {
                await DisplayAlert("Error", error.Message, "OK");
            }
          
        }
    }
}
