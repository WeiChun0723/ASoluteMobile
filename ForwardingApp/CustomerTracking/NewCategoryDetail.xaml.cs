using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rg.Plugins.Popup.Services;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class NewCategoryDetail : ContentPage
    {
        string haulierCode = "";
        List<ListItems> items = new List<ListItems>();

        public NewCategoryDetail(List<ListItems> sortedItem, string selectedCategory, string haulier)
        {
            InitializeComponent();

            Title = selectedCategory;

            haulierCode = haulier;

            loading.IsVisible = true;

            categoryDetail.ItemsSource = sortedItem;

            loading.IsVisible = false;

            items = sortedItem;

            MessagingCenter.Subscribe<App, string>((App)Application.Current, "RefreshContainers", (sender, id) =>
            {
                RemoveContainerFromList(id);
            });

        }

        async void Handle_ItemTapped(object sender, Xamarin.Forms.ItemTappedEventArgs e)
        {
            await Navigation.PushAsync(new ContainerDetails(haulierCode, ((ListItems)e.Item)));
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            var button = sender as SfButton;
            StackLayout listViewItem = (StackLayout)button.Parent;
            Label label = (Label)listViewItem.Children[0];
            ListItems item = items.Find(x => x.Summary.Contains(label.Text));

          
            switch (button.Text)
            {
                case "Confirm Receive":
                    var cd_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.updatePODURL(haulierCode, item.Id), this);
                    clsResponse cd_response = JsonConvert.DeserializeObject<clsResponse>(cd_content);

                    if (cd_response.IsGood)
                    {
                        RemoveContainerFromList(item.Id);
                        MessagingCenter.Send<App>((App)Application.Current, "RefreshCategory");
                    }
                    break;

                case "RFC":
                    string rfcValue = "";
                    string rfcHours = "";

                    var rfc_content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getContainerDetailURL(haulierCode, item.Id), this);
                    clsResponse rfc_response = JsonConvert.DeserializeObject<clsResponse>(rfc_content);

                    
                    if(rfc_response.IsGood)
                    {
                        var containers = JObject.Parse(rfc_content)["Result"].ToObject<clsDataRow>();

                        foreach (clsCaptionValue details in containers.Details)
                        {
                            if (details.Caption.Equals("RFC"))
                            {
                                rfcValue = details.Value;
                            }
                            else if (details.Caption.Equals("RFC Hour"))
                            {
                                rfcHours = details.Value;
                            }
                        }

                        await PopupNavigation.Instance.PushAsync(new PopUp(haulierCode, rfcValue, rfcHours, item.Id,"containerList"));
                    }
                    break;
            }

        }

        void RemoveContainerFromList(string container_Id)
        {
            items.RemoveAll(i => i.Id == container_Id);
            categoryDetail.ItemsSource = null;
            categoryDetail.ItemsSource = items;
        }
    }
}
