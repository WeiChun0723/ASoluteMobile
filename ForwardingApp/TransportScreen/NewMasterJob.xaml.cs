using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class NewMasterJob : ContentPage
    {
        List<string> customerList = new List<string>();
        List<string> pickupList = new List<string>();
        List<clsKeyValue> customers;
        Lists comboBoxList;
        public static List<MasterJob> masterJobList = new List<MasterJob>();
        public static string masterJobNo,scannedResult,dropOff,type,quantity;


        public NewMasterJob()
        {
            InitializeComponent();

            Title = "New Master Job";

            GetCustomerList();

            MessagingCenter.Subscribe<App>((App)Application.Current, "SetPageTitle", (sender) => {
                Title = masterJobNo;
            });

            MessagingCenter.Subscribe<App>((App)Application.Current, "RefreshNewMasterJobList", (sender) => {
                listView.ItemsSource = null;
                listView.ItemsSource = masterJobList;
            });

            MessagingCenter.Subscribe<App>((App)Application.Current, "LaunchBarCodeScanner", async (sender) => {
                var scanPage = new ZXingScannerPage();
                await Navigation.PushAsync(scanPage);

                scanPage.OnScanResult += (result) =>
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        await Navigation.PopAsync();

                        scannedResult = result.Text;

                        var selectedCustomer = customers.Find(cust => cust.Value == customerComboBox.Text);

                        var selectedPickup = comboBoxList.PickupList.Find(pick => pick.Value == pickUpComboBox.Text);

                        await PopupNavigation.Instance.PushAsync(new NewMasterJobPopUp(selectedCustomer, selectedPickup, comboBoxList));

                    });
                };
            });
        }

        async void GetCustomerList()
        {
            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCustomerList(), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response != null)
            {
                customerList.Clear();

                customers = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                foreach(clsKeyValue customer in customers)
                {
                    customerList.Add(customer.Value);
                }

                customerComboBox.ComboBoxSource = customerList;
            }
        }

        async void Handle_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {
            var selectedCustomer = customers.Find(cust => cust.Value == customerComboBox.Text);

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCustomerDetail(selectedCustomer.Key), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response != null)
            {
                pickupList.Clear();

                comboBoxList = JObject.Parse(content)["Result"].ToObject<Lists>();

                foreach(clsKeyValue pickUp in comboBoxList.PickupList)
                {
                    pickupList.Add(pickUp.Value);
                }

                pickUpComboBox.ComboBoxSource = null;
                pickUpComboBox.ComboBoxSource = pickupList;
            }
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            if(customerComboBox.SelectedIndex != -1 && pickUpComboBox.SelectedIndex != -1)
            {
                var selectedCustomer = customers.Find(cust => cust.Value == customerComboBox.Text);

                var selectedPickup = comboBoxList.PickupList.Find(pick => pick.Value == pickUpComboBox.Text);

                await PopupNavigation.Instance.PushAsync(new NewMasterJobPopUp(selectedCustomer, selectedPickup, comboBoxList));
            }
            else
            {
                await DisplayAlert("Missing field", "Please enter all mandatory field", "OK");
            }
        }
    }

    public class Lists
    {
        public List<clsKeyValue> PickupList { get; set; }
        public List<clsKeyValue> DropoffList { get; set; }
        public List<clsKeyValue> CargoTypeList { get; set; }
    }

    public class MasterJob
    {
        public string DropoffCode { get; set; }
        public string DONo { get; set; }
        public string CargoType { get; set; }
        public string Quantity { get; set; }
    }
}
