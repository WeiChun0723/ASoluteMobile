using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.Planner
{
    public partial class EqCategory : ContentPage
    {
        public EqCategory()
        {
            InitializeComponent();


            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text =  "My Equipments",
                TextColor = Color.White
            };

            Label title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            loading.IsVisible = true;

            EquipmentCategory();
        }


        async void EquipmentCategory()
        {


            var content = await CommonFunction.CallWebService(0,null,Ultis.Settings.SessionBaseURI, ControllerUtil.getEqCategoryURL(),this);
            clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(equipment_response.IsGood)
            {
                List<clsCaptionValue> equipmentList = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                dataGrid.ItemsSource = equipmentList;
            }


            loading.IsVisible = false;
        }

        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            clsCaptionValue category = new clsCaptionValue();
            category = ((clsCaptionValue)e.RowData);

            if(category != null)
            {
                await Navigation.PushAsync(new Equipments(category.Caption));
            }

        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            EquipmentCategory();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
