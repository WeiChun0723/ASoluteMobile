using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{

    public partial class DataGrid : ContentPage
    {

        List<clsVolume> volumes;

        ObservableCollection<HaulageVolume> data = new ObservableCollection<HaulageVolume>();

        public DataGrid()
        {
         
            InitializeComponent();

            Title = DateTime.Now.ToString("MMMM yyyy");

            loading.IsRunning = true;
            loading.IsVisible = true;
            loading.IsEnabled = true;

            GetHaulageVolume();
        }

        public async void GetHaulageVolume()
        {
            var volume_content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getHaulageVolume());
            clsResponse volume_response = JsonConvert.DeserializeObject<clsResponse>(volume_content);

            if (volume_response.IsGood)
            {
                volumes = JObject.Parse(volume_content)["Result"].ToObject<List<clsVolume>>();

                foreach(clsVolume volume in volumes)
                {
                    HaulageVolume dataValue = new HaulageVolume
                    {
                        Entity = volume.Entity,
                        Job = volume.Job.ToString("N0"),
                        Revenue = volume.Revenue.ToString("N0")
                    };

                    data.Add(dataValue);
                }

                dataGrid.ItemsSource = data;

                job.ItemsSource = data;
                job.XBindingPath = "Entity";
                job.YBindingPath = "Job";


                revenue.ItemsSource = volumes;
                revenue.XBindingPath = "Entity";
                revenue.YBindingPath = "Revenue";
            }
            else
            {
                await DisplayAlert("JsonError", volume_response.Message, "OK");
            }

            loading.IsRunning = false;
            loading.IsVisible = false;
            loading.IsEnabled = false;
        }

    }
}
