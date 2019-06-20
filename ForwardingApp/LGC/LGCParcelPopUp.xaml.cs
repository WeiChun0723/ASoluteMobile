using System;
using System.Collections.Generic;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;

namespace ASolute_Mobile.LGC
{
    public partial class LGCParcelPopUp : PopupPage
    {
        string orderNo = "";

        public LGCParcelPopUp(ListItems item)
        {
            InitializeComponent();

            parcelSummary.Text = item.Summary;
            
            orderNo = item.Id;
        }

        async void Handle_Clicked(object sender, System.EventArgs e)
        {
            loading.IsVisible = true;

            var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.printParcelLabelURL(orderNo), this);
            clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

            if(response.IsGood)
            {
                await DisplayAlert("Success", "Printed parcel label.", "OK");
            }

            loading.IsVisible = false;
        }

       
    }
}
