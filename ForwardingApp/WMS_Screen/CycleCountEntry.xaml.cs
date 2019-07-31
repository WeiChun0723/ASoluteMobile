using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.XForms.ComboBox;
using Xamarin.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class CycleCountEntry : ContentPage
    {
        ListItems selectedItem;
        int countVer;
        List<string> zones = new List<string>();
        List<string> racks = new List<string>();
        List<string> levels = new List<string>();

        public CycleCountEntry(ListItems item)
        {
            InitializeComponent();
            Title = "Cycle Count";
            selectedItem = item;

            string[] ver = selectedItem.Name.Split('/');

            countVer = Convert.ToInt16(ver[1]);

            GetZonesData(); 
        }


        async void HandleSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as SfComboBox;

            switch(comboBox.StyleId)
            {
                case "zoneBox":
                    racks.Clear();
                    levels.Clear();
                    rackBox.Text = "";
                    lvlBox.Text = "";
                    lvlBox.ComboBoxSource = null;

                    var rack_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getRackListURL(selectedItem.Id,zoneBox.Text, countVer), this);
                    if (rack_content != null)
                    {
                        clsResponse rack_response = JsonConvert.DeserializeObject<clsResponse>(rack_content);
                        if (rack_response.IsGood)
                        {
                            var rackList = JObject.Parse(rack_content)["Result"].ToObject<List<clsKeyValue>>();

                            foreach (clsKeyValue rack in rackList)
                            {
                                if(!(String.IsNullOrEmpty(rack.Value)))
                                {
                                    racks.Add(rack.Value);
                                }
                               
                            }

                            rackBox.ComboBoxSource = null;
                            rackBox.ComboBoxSource = racks;
                        }
                    }
                    break;

                case "rackBox":
                    levels.Clear();

                    var level_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getLevelListURL(selectedItem.Id, zoneBox.Text,rackBox.Text, countVer), this);
                    if (level_content != null)
                    {
                        clsResponse level_response = JsonConvert.DeserializeObject<clsResponse>(level_content);
                        if (level_response.IsGood)
                        {
                            var levelList = JObject.Parse(level_content)["Result"].ToObject<List<clsKeyValue>>();

                            foreach (clsKeyValue level in levelList)
                            {
                                if(!(String.IsNullOrEmpty(level.Value)))
                                {
                                    levels.Add(level.Value);
                                }
                                
                            }

                            lvlBox.ComboBoxSource = null;
                            lvlBox.ComboBoxSource = levels;
                        }
                    }
                    break;
            }
        }

        async void Handle_Clicked(object sender, EventArgs e)
        {
            if(!(String.IsNullOrEmpty(zoneBox.Text)) && !(String.IsNullOrEmpty(rackBox.Text)) && !(String.IsNullOrEmpty(lvlBox.Text)))
            {
                await Navigation.PushAsync(new CycleCountDetailsEntry(selectedItem.Id, zoneBox.Text, rackBox.Text, lvlBox.Text,countVer));
            }
            else
            {
                await DisplayAlert("Missing field", "Please fill in all mandatory field.", "OK");
            }
        }

        async void GetZonesData()
        {
            try
            {
                zones.Clear();

                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getZoneListURL(selectedItem.Id, countVer), this);
                if (content != null)
                {
                    clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                    if (response.IsGood)
                    {
                        var zoneList = JObject.Parse(content)["Result"].ToObject<List<clsKeyValue>>();

                        foreach (clsKeyValue zone in zoneList)
                        {
                            if (!(String.IsNullOrEmpty(zone.Value)))
                            {
                                zones.Add(zone.Value);
                            }

                        }

                        zoneBox.ComboBoxSource = zones;
                    }

                }
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
            
        }
    }
}
