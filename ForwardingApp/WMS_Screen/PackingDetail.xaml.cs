using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute.Mobile.Models.Warehouse;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.SfDataGrid.XForms;
using Xamarin.Forms;

namespace ASolute_Mobile.WMS_Screen
{
    public partial class PackingDetail : ContentPage
    {
        clsWhsHeader packingDetails;
        string packingID;
        public PackingDetail(string id)
        {
            InitializeComponent();

            packingID = id;

        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            GetPackingDetails();
        }

        async void GetPackingDetails()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.loadPackingDetail(packingID));
            clsResponse tallyIn_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (tallyIn_response.IsGood)
            {
                packingDetails = JObject.Parse(content)["Result"].ToObject<clsWhsHeader>();

                pickingDesc.Children.Clear();

                Label topBlank = new Label();
                pickingDesc.Children.Add(topBlank);

                foreach (clsCaptionValue desc in packingDetails.Summary)
                {
                    Label caption = new Label();

                    if (desc.Caption.Equals(""))
                    {
                        caption.Text = "      " + desc.Value;
                        caption.FontAttributes = FontAttributes.Bold;
                        Title = "Picking # " + desc.Value;
                    }
                    else
                    {
                        caption.Text = "      " + desc.Caption + ": " + desc.Value;
                    }

                    if (desc.Caption.Equals(""))
                    {

                        Title = "Packing # " + desc.Value;
                    }

                    pickingDesc.Children.Add(caption);
                }

                Label bottomBlank = new Label();
                pickingDesc.Children.Add(bottomBlank);

                dataGrid.AutoGenerateColumns = false;
                dataGrid.ItemsSource = packingDetails.Items;

                dataGrid.Columns.Clear();

                foreach (clsKeyValue gridField in packingDetails.ItemColumns)
                {
                    GridTextColumn gridColumn = new GridTextColumn();
                    gridColumn.MappingName = gridField.Key;
                    gridColumn.Width = 150;
                    
                    gridColumn.HeaderTemplate = new DataTemplate(() =>
                    {
                        ViewCell viewCell = new ViewCell();

                        Label label = new Label
                        {
                            Text = gridField.Value,
                            BackgroundColor = Color.Transparent,
                            VerticalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalOptions = LayoutOptions.CenterAndExpand,
                            HorizontalTextAlignment = TextAlignment.Center,
                            FontAttributes = FontAttributes.Bold
                        };

                        viewCell.View = label;
                        return viewCell;
                    });

                    dataGrid.Columns.Add(gridColumn);
                }
            }
            else
            {
                await DisplayAlert("Error", tallyIn_response.Message, "OK");
            }

            loading.IsVisible = false;
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            GetPackingDetails();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }

        async void PackNow(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new PackingEntry(packingID,Title));

        }

    }
}
