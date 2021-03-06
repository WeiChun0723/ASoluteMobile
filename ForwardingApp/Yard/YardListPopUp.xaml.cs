﻿using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace ASolute_Mobile.Yard
{
    public partial class YardListPopUp : PopupPage
    {
        ListItems selectedItem = new ListItems();
        List<clsYardBlock> yardBlocks;
        List<string> blocks = new List<string>();
        List<string> bays = new List<string>();
        List<string> levels = new List<string>();

        public YardListPopUp(ListItems item)
        {
            InitializeComponent();

            selectedItem = item;

            switch(item.Category)
            {
                case "PendingStorage":
                    blockComboBox.IsVisible = true;
                    bayComboBox.IsVisible = true;
                    levelComboBox.IsVisible = true;
                    GetYardValue();
                    break;

                case "Outbound":
                    outboundSummary.IsVisible = true;
                    outboundSummary.Text = item.Summary;
                    break;
            }
           
        }

        async void GetYardValue()
        {
            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getBlockList(), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    yardBlocks = JObject.Parse(content)["Result"].ToObject<List<clsYardBlock>>();
                    foreach (clsYardBlock yardBlock in yardBlocks)
                    {
                        blocks.Add(yardBlock.Id);
                    }
                    blockComboBox.ComboBoxSource = blocks;

                    if(!(String.IsNullOrEmpty(selectedItem.Action)))
                    {
                        blockComboBox.Text = selectedItem.Action;
                       
                        LoadComboBoxValue();
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        //refresh bay and level drop down list value when choose other block
        void Handle_SelectionChanged(object sender, Syncfusion.XForms.ComboBox.SelectionChangedEventArgs e)
        {
            LoadComboBoxValue();
        }

        void LoadComboBoxValue()
        {
            bays.Clear();
            levels.Clear();
            bayComboBox.IsEnabled = true;
            levelComboBox.IsEnabled = true;

            var selectedBay = yardBlocks.Find(item => item.Id == blockComboBox.Text);

            if (selectedBay.TotalBay == 0 && selectedBay.TotalLevel == 0)
            {
                bayComboBox.IsEnabled = false;
                levelComboBox.IsEnabled = false;
            }
            else
            {
                for (int bayCount = 1; bayCount <= selectedBay.TotalBay; bayCount++)
                {
                    string number = (bayCount < 10) ? "0" + bayCount : bayCount.ToString();
                    bays.Add(number);
                }

                bayComboBox.Text = "Bay";
                bayComboBox.ComboBoxSource = null;
                bayComboBox.ComboBoxSource = bays;

                for (int levelCount = 1; levelCount <= selectedBay.TotalLevel; levelCount++)
                {
                    levels.Add(levelCount.ToString());
                }

                levelComboBox.Text = "Level";
                levelComboBox.ComboBoxSource = null;
                levelComboBox.ComboBoxSource = levels;
            }
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            try
            {
                switch (selectedItem.Category)
                {
                    case "PendingStorage":
                        if (!(String.IsNullOrEmpty(blockComboBox.Text)))
                        {
                            string locationId = (bayComboBox.IsEnabled == false && levelComboBox.IsEnabled == false) ? blockComboBox.Text : blockComboBox.Text + "-" + bayComboBox.Text + "-" + levelComboBox.Text;
                            var inboundContent = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.confirmBlock(selectedItem.Id, locationId), this);
                            clsResponse inboundResponse = JsonConvert.DeserializeObject<clsResponse>(inboundContent);

                            if (inboundResponse != null)
                            {
                                await DisplayAlert("Success", "Location confirmed.", "OK");
                                await PopupNavigation.Instance.PopAsync(true);
                                MessagingCenter.Send<App>((App)Application.Current, "RefreshYard");
                            }
                        }
                        else
                        {
                            await DisplayAlert("Missing field", "Please provide valid location.", "OK");
                        }
                        break;

                    case "Outbound":
                        var outboundContent = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.confirmOutbound(selectedItem.Id), this);
                        clsResponse outboundResponse = JsonConvert.DeserializeObject<clsResponse>(outboundContent);

                        if (outboundResponse != null)
                        {
                            await DisplayAlert("Success", "Outbound confirmed.", "OK");
                            await PopupNavigation.Instance.PopAsync(true);
                            MessagingCenter.Send<App>((App)Application.Current, "RefreshYard");
                        }
                        break;

                }
            }
            catch
            {

            }
           


        }
    }
}
