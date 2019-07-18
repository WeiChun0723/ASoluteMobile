using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class CycleCountDetailsEntry : ContentPage
    {
        string cycleCountId, cycleCountZone, cycleCountRack, cycleCountLevel;

        public CycleCountDetailsEntry(string id, string zone, string rack , string level)
        {
            InitializeComponent();

            cycleCountId = id;
            cycleCountZone = zone;
            cycleCountRack = rack;
            cycleCountLevel = level;

            Title = "Cycle Count Detail";

            locationEntry.Text = zone + "-" + rack + "-" + level;
        }

        async void Handle_Clicked(object sender, EventArgs e)
        {
            var button = sender as SfButton;

            switch(button.StyleId)
            {
                case "btnReverse":
                    var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCycleCountLoadURL(cycleCountId, cycleCountZone, cycleCountRack,"true"), this);
                    if (content != null)
                    {
                        clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);
                        if (response.IsGood)
                        {
                  
                        }
                    }
                    break;
            }
        }
    }
}
