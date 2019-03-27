using System;
using System.Collections.Generic;
using System.Linq;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.Planner
{
    public partial class Equipments : ContentPage
    {
        string eqCategory;
        List<clsCaptionValue> equipmentList;

        public Equipments(string category)
        {
            InitializeComponent();
            eqCategory = category;

            StackLayout main = new StackLayout();

            Label title1 = new Label
            {
                FontSize = 15,
                Text = category,
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


            EquipmentData();

        }

        async void EquipmentData()
        {
            loading.IsVisible = true;

            var content = await CommonFunction.GetRequestAsync(Ultis.Settings.SessionBaseURI, ControllerUtil.getEqList(eqCategory));
            clsResponse equipment_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if (equipment_response.IsGood)
            {
                equipmentList = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                dataGrid.ItemsSource = equipmentList;
            }

            loading.IsVisible = false;

        }

        private async void OnFilterTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                string searchKey = e.NewTextValue.ToUpper();

                if (string.IsNullOrEmpty(searchKey))
                {
                    dataGrid.AutoExpandGroups = false;
                    dataGrid.ItemsSource = equipmentList;
                }

                else
                {
                    try
                    {
                        dataGrid.AutoExpandGroups = true;
                        dataGrid.ItemsSource = equipmentList.Where(x => x.Caption.Contains(searchKey));
                    }
                    catch
                    {
                        await DisplayAlert("Error", "Please try again", "OK");
                    }
                }
            }
            catch
            {
                await DisplayAlert("Error", "Please try again.", "OK");
            }

        }


        async void Handle_GridTapped(object sender, Syncfusion.SfDataGrid.XForms.GridTappedEventArgs e)
        {
            try
            {
                clsCaptionValue category = new clsCaptionValue();
                category = ((clsCaptionValue)e.RowData);

                if (category != null)
                {
                    await Navigation.PushAsync(new EquipmentDetails(category.Caption, null));
                }
            }
            catch(Exception ex)
            {

                //await DisplayAlert("error", ex.Message, "ok");
                //this is to avoid app be crash when the user press the grid header.
            }
        }

        void Handle_Pulling(object sender, Syncfusion.SfPullToRefresh.XForms.PullingEventArgs args)
        {
            args.Cancel = false;
            var prog = args.Progress;
        }

        void Handle_Refreshing(object sender, System.EventArgs e)
        {
            EquipmentData();
            pullToRefresh.IsRefreshing = false;
        }

        void Handle_Refreshed(object sender, System.EventArgs e)
        {

            pullToRefresh.IsRefreshing = false;
        }
    }
}
