using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using ZXing.Net.Mobile.Forms;

namespace ASolute_Mobile.TransportScreen
{
    public partial class NewMasterJobPopUp : PopupPage
    {
        List<string> dropOffList = new List<string>();
        List<string> cargoTypeList = new List<string>();
        clsKeyValue cust, pick;
        Lists comboBoxData;

        public NewMasterJobPopUp(clsKeyValue customerCode,clsKeyValue pickupCode, Lists comboBoxList)
        {
            InitializeComponent();

            cust = customerCode;
            pick = pickupCode;
            comboBoxData = comboBoxList;

            foreach(clsKeyValue drop in comboBoxList.DropoffList)
            {
                dropOffList.Add(drop.Value);
            }
            dropOffComboBox.ComboBoxSource = dropOffList;

            foreach(clsKeyValue cargo in comboBoxList.CargoTypeList)
            {
                cargoTypeList.Add(cargo.Value);
            }
            cargoTypeComboBox.ComboBoxSource = cargoTypeList;

            loadUserPreviousInput();
        }

        void loadUserPreviousInput()
        {
            if (!(String.IsNullOrEmpty(NewMasterJob.dropOff)))
            {
                dropOffComboBox.Text = NewMasterJob.dropOff;
            }

            if (!(String.IsNullOrEmpty(NewMasterJob.scannedResult)))
            {
                doNo.Text = NewMasterJob.scannedResult;
            }

            if (!(String.IsNullOrEmpty(NewMasterJob.type)))
            {
                cargoTypeComboBox.Text = NewMasterJob.type;
            }

            if (!(String.IsNullOrEmpty(NewMasterJob.quantity)))
            {
                quantity.Text = NewMasterJob.quantity;
            }
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;
            try
            {
                if(!(String.IsNullOrEmpty(dropOffComboBox.Text)) && !(String.IsNullOrEmpty(doNo.Text)) && !(String.IsNullOrEmpty(cargoTypeComboBox.Text)) && !(String.IsNullOrEmpty(quantity.Text)))
                {
                    var selectedDropOff = comboBoxData.DropoffList.Find(dropoff => dropoff.Value == dropOffComboBox.Text);
                    var selectedCargoType = comboBoxData.CargoTypeList.Find(cargoType => cargoType.Value == cargoTypeComboBox.Text);

                    clsTruckingJobModel jobModel = new clsTruckingJobModel
                    {
                        MasterJobNo = (!(String.IsNullOrEmpty(NewMasterJob.masterJobNo))) ? NewMasterJob.masterJobNo : null,
                        CustomerCode = cust.Key,
                        PickupCode = pick.Key,
                        DropoffCode = selectedDropOff.Key,
                        DONo = doNo.Text,
                        CargoType = selectedCargoType.Key,
                        Quantity = Convert.ToInt32(quantity.Text)
                    };

                    NewMasterJob.dropOff = "";
                    NewMasterJob.type = "";
                    NewMasterJob.scannedResult = "";
                    NewMasterJob.quantity = "";

                    var content = await CommonFunction.CallWebService(1, jobModel, Ultis.Settings.SessionBaseURI, ControllerUtil.postCustomerDetail(), this);
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                    if (response!=null)
                    {
                        await DisplayAlert("Success", "Job added.", "OK");

                        MasterJob job = new MasterJob
                        {
                            DropoffCode = "Dropoff: " + selectedDropOff.Value,
                            DONo = "DoNo.: " + doNo.Text,
                            CargoType = "Type: " + selectedCargoType.Value,
                            Quantity = "Qty: " + quantity.Text
                        };
                        NewMasterJob.masterJobList.Add(job);

                        NewMasterJob.masterJobNo = response.Result["MasterJobNo"];

                        MessagingCenter.Send<App>((App)Application.Current, "SetPageTitle");

                        MessagingCenter.Send<App>((App)Application.Current, "RefreshNewMasterJobList");

                        dropOffComboBox.Text = null;
                        doNo.Text = "";
                        cargoTypeComboBox.Text = null;
                        quantity.Text = "";
                    }
                }
                else
                {
                    await DisplayAlert("Missing field", "Please enter all mandatory field.", "OK");
                }
            }
            catch
            {

            }

            loading.IsVisible = false;
        }

        void Handle_Tapped(object sender, System.EventArgs e)
        {
            NewMasterJob.dropOff = dropOffComboBox.Text;
            NewMasterJob.type = cargoTypeComboBox.Text;
            NewMasterJob.quantity = quantity.Text;

            PopupNavigation.Instance.PopAsync();
            MessagingCenter.Send<App>((App)Application.Current, "LaunchBarCodeScanner");
        }
    }
}
