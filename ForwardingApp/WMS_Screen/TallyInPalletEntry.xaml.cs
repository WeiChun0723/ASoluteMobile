using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.InputValidation;
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
        List<string> sizes = new List<string>();
        List<string> units = new List<string>();
        List<string> status = new List<string>();
        CustomEntry customEntry;
        CustomDatePicker customDatePicker;
        List<bool> checkField = new List<bool>();

        public TallyInPalletEntry(clsWhsItem product, string tallyInID)
        {
            InitializeComponent();

            id = tallyInID;

            productPallet = product;

            Title = "Tally In # " + product.ProductCode;

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

            GetNewPalletList();
        }

        async void GetNewPalletList()
        {
            try
            {
                var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.loadNewPallet(id, productPallet.ProductLinkId));
                clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (newPallet_response.IsGood)
                {
                    newPallet = JObject.Parse(content)["Result"].ToObject<clsPalletNew>();

                    foreach (clsKeyValue size in newPallet.PalletSize)
                    {
                        sizes.Add(size.Value);
                    }

                    foreach (clsKeyValue unit in newPallet.ProductUom)
                    {
                        units.Add(unit.Key);
                    }

                    foreach (clsKeyValue status_ in newPallet.StockStatus)
                    {
                        status.Add(status_.Key);
                    }

                    sizeBox.ComboBoxSource = sizes;
                    unitBox.ComboBoxSource = units;
                    statusBox.ComboBoxSource = status;

                    statusBox.Text = newPallet.DefaultStockStatus;

                    int row = 4, column = 0;
                    foreach (clsAttribute attr in newPallet.Attribute)
                    {
                        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });

                        Label fieldLbl = new Label
                        {
                            Text = attr.Caption,
                            FontAttributes = FontAttributes.Bold,
                            VerticalTextAlignment = TextAlignment.Center
                        };

                        StackLayout entryStack = new StackLayout
                        {
                            Orientation = StackOrientation.Horizontal,
                            StyleId = attr.Key
                        };

                        if (attr.Key.Equals("ExpiryDate") || attr.Key.Equals("MfgDate"))
                        {
                            customDatePicker = new CustomDatePicker
                            {
                                StyleId = attr.Key,
                                HorizontalOptions = LayoutOptions.FillAndExpand,
                                NullableDate = null
                            };

                            customDatePicker.Unfocused += (object sender, FocusEventArgs e) =>
                            {
                                DateUnfocus(sender);
                            };

                            entryStack.Children.Add(customDatePicker);
                        }
                        else
                        {
                            customEntry = new CustomEntry
                            {
                                StyleId = attr.Key,
                                HorizontalOptions = LayoutOptions.FillAndExpand,

                            };

                            customEntry.Behaviors.Add(new MaxLengthValidation { MaxLength = 20 });


                            entryStack.Children.Add(customEntry);
                        }

                        if (attr.Value.Equals("O") || attr.Value.Equals(""))
                        {
                            if (customEntry != null)
                            {
                                customEntry.LineColor = Color.WhiteSmoke;
                            }

                        }
                        else if (attr.Value.Equals("M"))
                        {
                            if (customEntry != null)
                            {
                                customEntry.LineColor = Color.LightYellow;
                            }
                            if (customDatePicker != null)
                            {
                                customDatePicker.BackgroundColor = Color.LightYellow;
                            }
                        }

                        Image scan = new Image
                        {
                            Source = "barCode.png",
                            WidthRequest = 60,
                            HeightRequest = 30,
                            VerticalOptions = LayoutOptions.Center,
                            StyleId = attr.Key
                        };

                        var scan_barcode = new TapGestureRecognizer
                        {
                            NumberOfTapsRequired = 1
                        };

                        scan_barcode.Tapped += (sender, e) =>
                        {
                            var image = sender as Image;

                            EntryScan(image);
                        };
                        scan.GestureRecognizers.Add(scan_barcode);

                        entryStack.Children.Add(scan);


                        grid.Children.Add(fieldLbl, column, row);
                        column++;
                        grid.Children.Add(entryStack, column, row);
                        row++;
                        column = 0;

                    }

                }
                else
                {
                    await DisplayAlert("Error", newPallet_response.Message, "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }

        void DateUnfocus(object sender)
        {
            var date = sender as CustomDatePicker;

            foreach (View t in grid.Children)
            {
                if (t.StyleId == date.StyleId)
                {
                    var stack = (StackLayout)t;

                    foreach (View v in stack.Children)
                    {
                        if (v.StyleId == date.StyleId)
                        {
                            string type = v.GetType().ToString();

                            if (type == "ASolute_Mobile.CustomRenderer.CustomDatePicker")
                            {
                                CustomDatePicker picker = (CustomDatePicker)v;

                                if (picker.Date.ToString("yyyy-MM-dd").Equals(DateTime.Now.ToString("yyyy-MM-dd")))
                                {
                                    picker.NullableDate = picker.Date;
                                }

                            }
                        }
                    }
                }
            }
        }

        void PalletScan(object sender, EventArgs e)
        {
            fieldName = "PalletScan";
            BarCodeScan(fieldName);
        }

        async void EntryScan(Image image)
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

                        CustomEntry entry = null;
                        CustomDatePicker picker = null;

                        foreach (View t in grid.Children)
                        {
                            if (t.StyleId == image.StyleId)
                            {
                                var stack = (StackLayout)t;

                                foreach (View v in stack.Children)
                                {
                                    if (v.StyleId == image.StyleId)
                                    {
                                        string type = v.GetType().ToString();

                                        if (type == "ASolute_Mobile.CustomRenderer.CustomEntry")
                                        {
                                            entry = (CustomEntry)v;
                                            entry.Text = result.Text;
                                        }
                                        else if (type == "ASolute_Mobile.CustomRenderer.CustomDatePicker")
                                        {
                                            picker = (CustomDatePicker)v;
                                            picker.Date = DateTime.Parse(result.Text);
                                        }

                                    }
                                }
                            }
                        }

                    });
                };
            }
            catch
            {

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

                        palletNo.Text = result.Text;

                    });
                };
            }
            catch (Exception e)
            {
                await DisplayAlert("Error", e.Message, "OK");
            }
        }

        async void ConfirmAddPallet(object sender, EventArgs e)
        {
            try
            {
                checkField.Clear();

                foreach (clsAttribute attr in newPallet.Attribute)
                {
                    if (attr.Value.Equals("M"))
                    {
                        string test = (!(String.IsNullOrEmpty(SearchControl(attr.Key, "GetValue")))) ? SearchControl(attr.Key, "GetValue") : String.Empty;

                        if (String.IsNullOrEmpty(test))
                        {
                            checkField.Add(false);
                        }
                        else
                        {
                            checkField.Add(true);
                        }
                    }
                }

                if (!(String.IsNullOrEmpty(quantity.Text)) && !(String.IsNullOrEmpty(sizeBox.Text))
                    && !(String.IsNullOrEmpty(statusBox.Text)) && !(String.IsNullOrEmpty(unitBox.Text)) && !(checkField.Contains(false)))
                {

                    string[] size = sizeBox.Text.Split('(');

                    string[] numbers = Regex.Split(size[1], @"\D+");

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
                        String01 = (!(String.IsNullOrEmpty(SearchControl("String01", "GetValue")))) ? SearchControl("String01", "GetValue") : String.Empty,
                        String02 = (!(String.IsNullOrEmpty(SearchControl("String02", "GetValue")))) ? SearchControl("String02", "GetValue") : String.Empty,
                        String03 = (!(String.IsNullOrEmpty(SearchControl("String03", "GetValue")))) ? SearchControl("String03", "GetValue") : String.Empty,
                        String04 = (!(String.IsNullOrEmpty(SearchControl("String04", "GetValue")))) ? SearchControl("String04", "GetValue") : String.Empty,
                        String05 = (!(String.IsNullOrEmpty(SearchControl("String05", "GetValue")))) ? SearchControl("String05", "GetValue") : String.Empty,
                        String06 = (!(String.IsNullOrEmpty(SearchControl("String06", "GetValue")))) ? SearchControl("String06", "GetValue") : String.Empty,
                        ExpiryDate = (!(String.IsNullOrEmpty(SearchControl("ExpiryDate", "GetValue")))) ? SearchControl("ExpiryDate", "GetValue") : String.Empty,
                        MfgDate = (!(String.IsNullOrEmpty(SearchControl("MfgDate", "GetValue")))) ? SearchControl("MfgDate", "GetValue") : String.Empty,
                    };

                    var content = await CommonFunction.PostRequestAsync(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPallet(id));
                    clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (upload_response.IsGood)
                    {
                        await DisplayAlert("Success", "New pallet added.", "OK");
                        palletNo.Text = String.Empty;
                        sizeBox.Text = String.Empty;
                        quantity.Text = String.Empty;
                        unitBox.Text = String.Empty;
                        statusBox.Text = newPallet.DefaultStockStatus;

                        List<string> dynamicFields = new List<string>
                            {
                            "String01",
                            "String02",
                            "String03",
                            "String04",
                            "String05",
                            "String06",
                            "ExpiryDate",
                            "MfgDate"
                            };

                        foreach (string field in dynamicFields)
                        {
                            SearchControl(field, "ClearValue");
                        }
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

        string SearchControl(string controlID, string action)
        {
            foreach (View t in grid.Children)
            {
                if (t.StyleId == controlID)
                {
                    var stack = (StackLayout)t;

                    foreach (View v in stack.Children)
                    {
                        if (v.StyleId == controlID)
                        {
                            string type = v.GetType().ToString();

                            if (type == "ASolute_Mobile.CustomRenderer.CustomEntry")
                            {
                                CustomEntry entry = (CustomEntry)v;

                                if (action == "GetValue")
                                {
                                    return entry.Text;
                                }
                                else
                                {
                                    entry.Text = String.Empty;
                                }

                            }
                            else if (type == "ASolute_Mobile.CustomRenderer.CustomDatePicker")
                            {
                                CustomDatePicker picker = (CustomDatePicker)v;

                                if (action == "GetValue")
                                {
                                    if (picker.NullableDate == null)
                                    {
                                        return "";
                                    }
                                    else
                                    {
                                        return picker.Date.ToString("yyyy-MM-dd");
                                    }
                                }
                                else
                                {
                                    if (picker.NullableDate != null)
                                    {
                                        picker.Date = DateTime.Now;
                                        picker.NullableDate = null;
                                    }
                                }

                            }
                        }
                    }
                }
            }

            return null;
        }

    }
}
