using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models.ProvidersModel;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class AddServiceProvider : ContentPage
    {
        List<clsProvider> providers = new List<clsProvider>();
        List<clsProvider> selectedProvider = new List<clsProvider>();

        public AddServiceProvider()
        {
            InitializeComponent();
            Title = "Add service provider";
            getAvailableProviders();
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new MainPage();
            return true;
        }

        public async void Confirm_Clicked(object sender, EventArgs e)
        {
            try
            {
                selectedProvider.Clear();
                ObservableCollection<Provider> checkedProvider = new ObservableCollection<Provider>();
                checkedProvider = ProviderListViewModel.CheckedList;
                foreach (Provider provider in checkedProvider)
                {
                    if (provider.IsSelected == true)
                    {
                        clsProvider chosenProvider = new clsProvider();
                        chosenProvider.Code = provider.Code;
                        chosenProvider.Name = provider.Name;
                        chosenProvider.Url = provider.Url;

                        selectedProvider.Add(chosenProvider);
                    }

                }

                var content = await CommonFunction.PostRequest(selectedProvider, Ultis.Settings.SessionBaseURI, ControllerUtil.saveProvider());
                clsResponse save_response = JsonConvert.DeserializeObject<clsResponse>(content);

                if(save_response.IsGood)
                {
                    await DisplayAlert("Succeed", "Provider added to the home list", "OK");
                }
                else
                {
                    await DisplayAlert("Failed", save_response.Message, "OK");
                }

            }
            catch(Exception exception)
            {
                await DisplayAlert("Error", exception.Message, "Ok");
            }

        }

        public async void getAvailableProviders()
        {
            var content = await CommonFunction.GetWebService(Ultis.Settings.SessionBaseURI, ControllerUtil.getAvailableProvider());
            clsResponse available_response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(available_response.IsGood)
            {
                for (int check = 0; check < available_response.Result.Count; check++)
                {
                    clsProvider provider = new clsProvider();

                    provider.Code = available_response.Result[check]["Code"];
                    provider.Name = available_response.Result[check]["Name"];
                    provider.Url = available_response.Result[check]["Url"];
                    providers.Add(provider);
                }


                BindingContext = new ProviderListViewModel(providers);

            }
            else
            {
                await DisplayAlert("JsonError", available_response.Message, "OK");
            }
        }
    }
}
