using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace ASolute_Mobile.Yard
{
    public partial class YardListPopUp : PopupPage
    {
        class clsYardBlock
        {
            public string Id { get; set; }
            public int TotalBay { get; set; }
            public int TotalLevel { get; set; }
        }

        List<clsYardBlock> yardBlocks;
        List<string> blocks = new List<string>();
        List<string> bays = new List<string>();
        List<string> levels = new List<string>();
        string recordId;

        public YardListPopUp(ListItems item)
        {
            InitializeComponent();

            recordId = item.Id;

            GetYardValue();
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
            bays.Clear();
            levels.Clear();

            var selectedBay = yardBlocks.Find(item => item.Id == blockComboBox.SelectedValue.ToString());
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

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            if(blockComboBox.SelectedIndex != -1 && bayComboBox.SelectedIndex != -1 && levelComboBox.SelectedIndex != -1)
            {
                string locationId = blockComboBox.Text + "-" + bayComboBox.Text + "-" + levelComboBox.Text;
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.confirmBlock(recordId, locationId), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    await DisplayAlert("Success", "Location confirmed.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Missing field", "Please provide valid location.", "OK");
            }
        }
    }
}
