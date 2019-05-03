using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Acr.UserDialogs;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.CustomRenderer;
using ASolute_Mobile.InputValidation;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.ComboBox;
using Xamarin.Forms;
using ZXing.Mobile;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class TallyInPalletEntry : ContentPage
    {
        clsWhsItem productPallet = new clsWhsItem();
        string id, productSKU, actionID;
        clsPalletNew newPallet;
        List<string> sizes = new List<string>();
        List<string> units = new List<string>();
        List<string> status = new List<string>();
        CustomEntry customEntry;
        CustomDatePicker customDatePicker;
        List<bool> checkField = new List<bool>();


        public TallyInPalletEntry(clsWhsItem product, string tallyInID, string action)
        {
            InitializeComponent();

            id = tallyInID;

            actionID = action;

            productPallet = product;

            Title = "Tally In # " + productPallet.ProductCode;

            palletDesc.Children.Clear();

            Label topBlank = new Label();
            palletDesc.Children.Add(topBlank);

            string[] descs = (productPallet.Description.Replace("\r\n", "t")).Split('t');

            foreach (string desc in descs)
            {
                Label caption = new Label();

                if (desc == "")
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

        protected override  void OnAppearing()
        {
            base.OnAppearing();

            if(this != null)
            {
                //MessagingCenter.Send(this, "preventLandScape");
            }
        }

        void Handle_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {
            var picker = sender as SfComboBox;

            if(picker.StyleId == "unitBox")
            {
                List<string> palletSizes = sizeBox.ComboBoxSource;

                foreach (string size in palletSizes)
                {
                    string[] defaultSize = size.Split('(');
                    if(defaultSize[1].Contains(unitBox.Text))
                    {
                        sizeBox.Text = size;
                    }
                   
                }
            }
        }

        async void GetNewPalletList()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.loadNewPalletURL(id, productPallet.ProductLinkId),this);
                clsResponse newPallet_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (newPallet_response.IsGood)
                {
                    newPallet = JObject.Parse(content)["Result"].ToObject<clsPalletNew>();
                    unitBox.Text = newPallet.DefaultProductUom;
                    statusBox.Text = newPallet.DefaultStockStatus;

                    foreach (clsKeyValue size in newPallet.PalletSize)
                    {
                        sizes.Add(size.Value);

                        string[] defaultSize = size.Value.Split('(');

                        if (defaultSize[1].Contains(newPallet.DefaultProductUom))
                        {
                            sizeBox.Text = size.Value;
                        }
                      
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

                        if (attr.Key == "ExpiryDate" || attr.Key == "MfgDate")
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

                            customEntry.Behaviors.Add(new MaxLengthValidation { MaxLength = 50 });

                            entryStack.Children.Add(customEntry);
                        }

                        if (attr.Value == "O" || attr.Value == "")
                        {
                            if (customEntry != null)
                            {
                                customEntry.LineColor = Color.WhiteSmoke;
                            }

                        }
                        else if (attr.Value == "M")
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

        async void PalletScan(object sender, EventArgs e)
        {
            try
            {
                var scanPage = new ZXingScannerPage();
                scanPage.AutoFocus();
                await Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                  {
                      Device.BeginInvokeOnMainThread(async () =>
                      {
                          scanPage.PauseAnalysis();
                          try
                          {
                              if (!(actionID == "BARRY"))
                              {
                                  await Navigation.PopAsync();
                                  palletNo.Text = result.Text;
                              }
                              else
                              {
                                  string productCode = result.Text.Substring(0, 13);
                                  string productRef = result.Text.Substring(16, 10);
                                  string productQTY = result.Text.Substring(26, 2);

                                  if (productCode == productPallet.ProductCode)
                                  {
                                      palletNo.Text = productRef;
                                      quantity.Text = productQTY;
                                      DisplayScanStatus(result.Text + " scanned");
                                  }
                                  else
                                  {
                                      DisplayScanStatus("Product not matched.");
                                  }

                                  if (scanPage != null)
                                  {
                                      scanPage.ResumeAnalysis();
                                  }
                              }
                          }
                         catch
                          {
                              DisplayScanStatus("Please scan again");
                          }

                      });
                  };
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        void DisplayScanStatus(string scanStatus)
        {
            var toastConfig = new ToastConfig(scanStatus);
            toastConfig.SetDuration(4000);
            toastConfig.Position = 0;
            toastConfig.SetMessageTextColor(System.Drawing.Color.Black);

            if(scanStatus.Contains("scanned"))
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.Green);
            }
            else
            {
                toastConfig.SetBackgroundColor(System.Drawing.Color.Red);
            }
            UserDialogs.Instance.Toast(toastConfig);
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

        async void ConfirmAddPallet(object sender, EventArgs e)
        {
            try
            {
                checkField.Clear();

                foreach (clsAttribute attr in newPallet.Attribute)
                {
                    if (attr.Value == "M")
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

                    clsPallet pallet = new clsPallet
                    {
                        Id = id,
                        ProductCode = productPallet.ProductCode,
                        PalletId = (!(String.IsNullOrEmpty(palletNo.Text))) ? palletNo.Text : String.Empty,
                        PalletSize = newPallet.PalletSize[sizes.FindIndex(x => x.Equals(sizeBox.Text))].Key,
                        Qty = Convert.ToInt32(quantity.Text),
                        Uom = newPallet.ProductUom[units.FindIndex(x => x.Equals(unitBox.Text))].Key,
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

                    var content = await CommonFunction.PostRequestAsync(pallet, Ultis.Settings.SessionBaseURI, ControllerUtil.postNewPalletURL(id));
                    clsResponse upload_response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (upload_response.IsGood)
                    {
                        await DisplayAlert("Success", "New pallet added.", "OK");
                        palletNo.Text = String.Empty;
                        quantity.Text = String.Empty;   

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
