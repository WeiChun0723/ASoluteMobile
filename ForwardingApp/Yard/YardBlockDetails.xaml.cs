using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.Yard
{
    public partial class YardBlockDetails : ContentPage
    {
        List<BlockContainer> blockContainers = new List<BlockContainer>();

        public YardBlockDetails(string blockID)
        {
            InitializeComponent();

            Title = blockID.Split('(')[0] + " block detail";

            GetBlockContainer(blockID);
        }



        async void GetBlockContainer(string blockID)
        {
            loading.IsVisible = true;

            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getCollectionInquiry(), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response != null)
                {
                    var records = JObject.Parse(content)["Result"].ToObject<List<clsDataRow>>();

                    foreach (clsDataRow record in records)
                    {
                        BlockContainer container = new BlockContainer();

                        foreach (clsCaptionValue summary in record.Summary)
                        {
                            switch (summary.Caption)
                            {
                                case "Container No.":
                                    container.ContainerNo = summary.Value;
                                    break;

                                case "Location":
                                    container.Location = summary.Value;
                                    break;

                                case "Customer":
                                    container.Customer = summary.Value;
                                    break;

                                case "Closing Date":
                                    container.ClosingDate = summary.Value;
                                    break;
                            }
                        }

                        if (!(String.IsNullOrEmpty(container.Location)) && container.Location.Contains(blockID.Substring(0, 3)))
                        {
                            blockContainers.Add(container);
                        }

                    }

                    dataGrid.ItemsSource = blockContainers;
                }
            }
            catch
            {

            }

            loading.IsVisible = false;
            
        }
    }

    class BlockContainer
    {
        public string ContainerNo { get; set; }
        public string Location { get; set; }
        public string Customer { get; set; }
        public string ClosingDate { get; set; }
    }
}
