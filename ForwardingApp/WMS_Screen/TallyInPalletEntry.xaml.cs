using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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
        clsWhsItem productPallet = new clsWhsItem();
        string id, fieldName;
        clsPalletNew newPallet;
        List<string> size = new List<string>();
        List<string> unit = new List<string>();
        List<string> status = new List<string>();
        bool tapped = true;

        public TallyInPalletEntry(clsWhsItem product, string tallyInID)
        {
            InitializeComponent();

            id = tallyInID;

            productPallet = product;

            Title = "Tally In # " + product.ProductCode;

            expDatePicker.MinimumDate = DateTime.Now;
            manuDatePicker.MinimumDate = DateTime.Now;

            expDate.Text = DateTime.Now.ToString("dd/MM/yyyy");
            manuDate.Text = DateTime.Now.ToString("dd/MM/yyyy");

            palletDesc.Children.Clear();

            Label topBlank = new Label();
            palletDesc.Children.Add(topBlank);

            string[] descs = (product.Description.Replace("\r\n", "t")).Split('t');

            foreach (string desc in descs)
            {
                Label caption = new Label();


                if (desc.Equals(""))
                {
                    caption.Text = "    " + desc;
                    caption.FontAttributes = FontAttributes.Bold;
                }
                else
                {
                    caption.Text = "    " + desc;
                }

                palletDesc.Children.Add(caption);
            }

            Label bottomBlank = new Label();
            palletDesc.Children.Add(bottomBlank);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            GetNewPalletList();
        }

        async void GetNewPalletList()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.loadNewPallet(id, productPallet.ProductLinkId));
            clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (newPallet_response.IsGood)
            {
                newPallet = JObject.Parse(content)["Result"].ToObject<clsPalletNew>();

                foreach (clsKeyValue sizes in newPallet.PalletSize)
                {
                    size.Add(sizes.Value);
                }

                foreach (clsKeyValue units in newPallet.ProductUom)
                {
                    unit.Add(units.Key);
                }

                foreach (clsKeyValue status_ in newPallet.StockStatus)
                {
                    status.Add(status_.Key);
                }

                sizeBox.ComboBoxSource = size;
                unitBox.ComboBoxSource = unit;
                statusBox.ComboBoxSource = status;

                statusBox.Text = newPallet.DefaultStockStatus;

                foreach (clsAttribute attr in newPallet.Attribute)
                {
                    if (attr.Caption == "Lot No")
                    {
                        string1lbl.IsVisible = true;
                        lotEntry.IsVisible = true;
                        String01.Text = attr.Caption;
                    }
                    else if (attr.Caption == "Batch No")
                    {
                        string2lbl.IsVisible = true;
                        batEntry.IsVisible = true;
                        String02.Text = attr.Caption;
                    }
                    else if (attr.Caption == "Color")
                    {
                        string3lbl.IsVisible = true;
                        colorEnrty.IsVisible = true;
                        String03.Text = attr.Caption;
                    }
                    else if (attr.Caption == "Exp Date")
                    {
                        explbl.IsVisible = true;
                        expEntry.IsVisible = true;
                        ExpiryDate.Text = attr.Caption;
                    }
                    else if (attr.Caption == "Manu Date")
                    {
                        manulbl.IsVisible = true;
                        manuEntry.IsVisible = true;
                        MgfDate.Text = attr.Caption;
                    }
                }

            }
            else
            {
                await DisplayAlert("Error", newPallet_response.Message, "OK");
            }
        }



        void PalletScan(object sender, EventArgs e)
        {
            fieldName = "PalletScan";
            BarCodeScan(fieldName);
        }

        void EntryScan(object sender, EventArgs e)
        {
            var image = sender as Image;

            if (image.StyleId.Equals("batchNum"))
            {
                fieldName = "BatchScan";
                BarCodeScan(fieldName);
            }
            else if (image.StyleId.Equals("lotNum"))
            {
                fieldName = "LotScan";
                BarCodeScan(fieldName);
            }
            else if (image.StyleId.Equals("manuDateScan"))
            {
                fieldName = "ManuScan";
                BarCodeScan(fieldName);
            }
        }

        void ExpiryDateScan(object sender, EventArgs e)
        {
            fieldName = "ExpiryScan";
            BarCodeScan(fieldName);
        }

        void DateEntry_Focused(object sender, Xamarin.Forms.FocusEventArgs e)
        {


            var dateEntry = sender as Entry;

            if (dateEntry.StyleId.Equals("expDate"))
            {
                expDatePicker.Focus();
                expDate.Unfocus();
            }
            else
            {
                manuDatePicker.Focus();
                manuDate.Unfocus();
            }

        }

        void DatePicker_DateSelected(object sender, Xamarin.Forms.DateChangedEventArgs e)
        {
            var date = sender as DatePicker;


            if (date.StyleId.Equals("expDatePicker"))
            {
                expDate.Text = expDatePicker.Date.ToString("dd/MM/yyyy");
            }
            else
            {
                manuDate.Text = manuDatePicker.Date.ToString("dd/MM/yyyy");
            }

        }

        async void BarCodeScan(string field)
        {
            if (tapped)
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

                            if (field == "PalletScan")
                            {
                                palletNo.Text = result.Text;

                            }
                            else if (field == "BatchScan")
                            {

                                batchNo.Text = result.Text;
                            }
                            else if (field == "LotScan")
                            {
                                lotNo.Text = result.Text;
                            }
                            else if (field == "ManuScan")
                            {
                                DateTime enteredDate = DateTime.Parse(result.Text);
                                manuDatePicker.Date = enteredDate;
                            }
                            else if (field == "ExpiryScan")
                            {
                                DateTime enteredDate = DateTime.Parse(result.Text);
                                expDatePicker.Date = enteredDate;

                            }
                        });
                    };
                }
                catch (Exception e)
                {
                    await DisplayAlert("Error", e.Message, "OK");
                }

                tapped = true;
            }

        }

        async void ConfirmAddPallet(object sender, EventArgs e)
        {
            try
            {
                if (!(String.IsNullOrEmpty(quantity.Text)) && !(String.IsNullOrEmpty(sizeBox.Text))
             && !(String.IsNullOrEmpty(statusBox.Text)) && !(String.IsNullOrEmpty(unitBox.Text)))
                {

                    string[] test = sizeBox.Text.Split('(');

                    string[] numbers = Regex.Split(test[1], @"\D+");

                    clsPallet pallet = new clsPallet
                    {
                        Id = id,
                        ProductCode = productPallet.ProductCode,
                        PalletId = (!(String.IsNullOrEmpty(palletNo.Text))) ? palletNo.Text : String.Empty,
                        PalletSize = newPallet.PalletSize[sizeBox.SelectedIndex].Key,
                        PalletTI = Convert.ToInt16(numbers[1]),
                        PalletHI = Convert.ToInt16(numbers[2]),
                        Qty = Convert.ToInt32(quantity.Text),
                        Uom = newPallet.ProductUom[unitBox.SelectedIndex].Key,
                        StockStatus = statusBox.Text,
                        String01 = (!(String.IsNullOrEmpty(lotNo.Text))) ? lotNo.Text : String.Empty,
                        String02 = (!(String.IsNullOrEmpty(batchNo.Text))) ? batchNo.Text : String.Empty,
                        String03 = (!(String.IsNullOrEmpty(color.Text))) ? color.Text : String.Empty,
                    };

                    pallet.ExpiryDate = (expDatePicker.Date.ToString("yyyy-MM-dd") == DateTime.Now.Date.ToString("yyyy-MM-dd")) ? String.Empty : expDatePicker.Date.ToString("yyyy-MM-dd");
                    pallet.MfgDate = (manuDatePicker.Date.ToString("yyyy-MM-dd") == DateTime.Now.Date.ToString("yyyy-MM-dd")) ? String.Empty : manuDatePicker.Date.ToString("yyyy-MM-dd");

                    var content = await CommonFunction.PostRequest(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPallet(id));
                    clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (upload_response.IsGood)
                    {
                        await DisplayAlert("Success", "New pallet added.", "OK");
                        palletNo.Text = String.Empty;
                        sizeBox.Text = String.Empty;
                        quantity.Text = String.Empty;
                        unitBox.Text = String.Empty;
                        statusBox.Text = newPallet.DefaultStockStatus;
                        batchNo.Text = String.Empty;
                        expDate.Text = String.Empty;
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
            catch (Exception error)
            {
                await DisplayAlert("Error", error.Message, "OK");
            }

        }
    }
}
