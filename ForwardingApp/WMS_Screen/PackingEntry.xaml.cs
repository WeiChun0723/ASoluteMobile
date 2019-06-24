using System;
using System.Collections.Generic;
using ASolute.Mobile.Models.Warehouse;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;
using Acr.UserDialogs;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PackingEntry : ContentPage
    {
        string uploadID;

        List<clsWhsItem> items = new List<clsWhsItem>();
        List<clsWhsItem> itemsTest = new List<clsWhsItem>();
        bool tapped = true;

        public PackingEntry(string packingID, string screenTitle)
        {
            InitializeComponent();

            uploadID = packingID;

            Title = screenTitle;
        }

        void Handle_ItemsSourceChanged(object sender, Syncfusion.SfDataGrid.XForms.GridItemsSourceChangedEventArgs e)
        {
            dataGrid.ItemsSource = items;
        }

        void ClearEntry(object sender, System.EventArgs e)
        {
            SKUEntry.Text = "";
        }

        void Handle_TextChanged(object sender, Xamarin.Forms.TextChangedEventArgs e)
        {
            if(!(String.IsNullOrEmpty(SKUEntry.Text)))
            {
                cancel.IsVisible = true;
            }
            else
            {
                cancel.IsVisible = false;
            }
        }

        async void ConfirmScan(object sender, System.EventArgs e)
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

                            SKUEntry.Text = result.Text;

                        });
                    };
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }

                tapped = true;
            }
      
        }

        async void AddToGrid(object sender, System.EventArgs e)
        {
            try
            {
                if(!(String.IsNullOrEmpty(SKUEntry.Text)) && !(String.IsNullOrEmpty(SKUEntry.Text)))
                {
                    clsWhsItem item = new clsWhsItem
                    {
                        ProductCode = SKUEntry.Text,
                        LoadQty = Convert.ToInt32(QtyEntry.Text)
                    };

                    bool existItem = items.Any(check => check.ProductCode == item.ProductCode);

                    if (existItem == false)
                    {
                        items.Add(item);
                        itemsTest.Add(item);

                        dataGrid.ItemsSource = items;
                        dataGrid.ItemsSource = itemsTest;

                        SKUEntry.Text = String.Empty;
                        QtyEntry.Text = String.Empty;
                       
                        var toastConfig = new ToastConfig("Added to the list");
                        toastConfig.SetDuration(3000);
                        toastConfig.SetBackgroundColor(System.Drawing.Color.FromArgb(0, 128, 0));
                        toastConfig.SetPosition(0);
                        UserDialogs.Instance.Toast(toastConfig);
                    }
                    else
                    {
                        await DisplayAlert("Duplicate", "Item added to list.", "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Missing field", "Please enter all field.", "OK");
                }

            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }


        async void GeneratePallet(object sender, System.EventArgs e)
        {
            try
            {
                clsWhsHeader productList = new clsWhsHeader();

                productList.Id = uploadID;
                productList.Items = items;

                var content = await CommonFunction.CallWebService(1,productList, Ultis.Settings.SessionBaseURI, ControllerUtil.postPackingDetailURL(),this);
                clsResponse update_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (update_response.IsGood)
                {
                    await DisplayAlert("Success", "List uploaded", "OK");
                    await Navigation.PopAsync();
                }
                else
                {
                    await DisplayAlert("Error", update_response.Message, "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }
    }

   
}
