using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class ProviderDetails : ContentPage
    {
        string providerCode;

        public ProviderDetails(string code)
        {
            InitializeComponent();
            providerCode = code;
            getCategory();
        }

        protected void refreshContainerList(object sender, EventArgs e)
        {

        }

        public async void CategorySelected(object sender, SelectedPositionChangedEventArgs e)
        {

        }

        public async void getCategory()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getCategoryList(providerCode));
            clsResponse provider_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(provider_response.IsGood)
            {
                categoryPicker.Items.Clear();

                var categories = JObject.Parse(content)["Result"].ToObject<List<clsCaptionValue>>();

                foreach(clsCaptionValue category in categories)
                {
                    categoryPicker.Items.Add(category.Value);
                }
            }
            else
            {
                await DisplayAlert("JsonError", provider_response.Message, "OK");
            }
        }
    }
}
