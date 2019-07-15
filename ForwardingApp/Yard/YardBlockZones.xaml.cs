using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.Yard
{
    public partial class YardBlockZones : ContentPage
    {
        List<clsYardMapBlock> yardBlocks;

        public YardBlockZones()
        {
            InitializeComponent();

            Title = "Zones";

            GetBlockList();
        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            string[] selectedZone = ((YardZone)e.Item).Zone.Split(' ');

            var zoneBlocks = yardBlocks.FindAll(b => b.Id.Contains(selectedZone[1]));

            await Navigation.PushAsync(new YardBlockLists(zoneBlocks));
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetBlockList();
        }

        async void GetBlockList()
        {
            loading.IsVisible = true;

            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getYardMapPosition(), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response.IsGood)
                {
                    yardBlocks = JObject.Parse(content)["Result"].ToObject<List<clsYardMapBlock>>();

                    var gate = yardBlocks.Find(x => x.Id == "GATE-IN");
                    yardBlocks.Remove(gate);

                    List<YardZone> zones = new List<YardZone>();

                    foreach (clsYardMapBlock yardBlock in yardBlocks)
                    {
                        var blockZone = "Zone " + yardBlock.Id.Substring(0, 1);

                        //var blockZone = "Zone " + yardBlock.ZoneCode;

                        if (!(zones.Exists(x => x.Zone == blockZone)))
                        {
                            YardZone yardZone = new YardZone
                            {
                                Zone = blockZone
                            };

                            zones.Add(yardZone);
                        }
                    }

                    if (zones.Count == 0)
                    {
                        noData.IsVisible = true;
                    }
                    else
                    {
                        listView.ItemsSource = null;
                        listView.ItemsSource = zones;
                        listView.RowHeight = 130;
                        noData.IsVisible = false;
                    }

                    listView.IsRefreshing = false;
                }
            }
            catch
            {

            }

            loading.IsVisible = false;
        }
    }

    class YardZone
    {
        public string Zone { get; set; }
    }
}
