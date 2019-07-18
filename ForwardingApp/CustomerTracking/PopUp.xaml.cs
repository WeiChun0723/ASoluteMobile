using System;
using ASolute.Mobile.Models;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;

namespace ASolute_Mobile.CustomerTracking
{
    public partial class PopUp : PopupPage
    {
        string provider_code, process_id, rfcHours, provider_container, previousPage;
        

        public PopUp(string code, string rfc,string rfcHour,string container,string lastPage)
        {
            InitializeComponent();
           
            provider_code = code;
            process_id = rfc;
            rfcHours = rfcHour;
            provider_container = container;
            previousPage = lastPage;
            datePicker.MinimumDate = DateTime.Now;

            for (int i = 0; i <= 23.5 * 60; i += 30)
            {
                int h = i / 60;

                int m = i % 60;

                TimeSpan timeSpan = new TimeSpan(h, m, 0);

                timePicker.Items.Add(timeSpan.ToString(@"hh\:mm"));
            }

            timePicker.SelectedIndex = 24;

        }

        public async void updateRFC(object sender, EventArgs e)
        {
            try
            {
                if(rfcHours != "")
                {
                    DateTime compareTime = DateTime.Now.AddHours(Convert.ToInt32(rfcHours));

                    string currentTime = datePicker.Date.ToString("yyyy-MM-dd") + " " + timePicker.SelectedItem;

                    if (Convert.ToDateTime(currentTime) > compareTime)
                    {
                        if (String.IsNullOrEmpty(remarks.Text))
                        {
                            remarks.Text = "";
                        }

                        var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.updateContainerRFCURL(provider_code, process_id, currentTime, remarks.Text), this);
                        clsResponse rfc_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (rfc_response.IsGood)
                        {

                            await PopupNavigation.Instance.PopAsync(true);

                            datePicker.Unfocus();

                            if (previousPage == "containerDetail")
                            {
                                MessagingCenter.Send<App>((App)Application.Current, "RefreshDetail");
                            }

                            MessagingCenter.Send<App, string>((App)Application.Current, "RefreshContainers", provider_container);
                            MessagingCenter.Send<App>((App)Application.Current, "RefreshCategory");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Error", "Required Date & Time must be later than " + compareTime.ToString("yyyy-MM-dd hh:mm"), "OK");
                    }
                }
                else
                {
                    await DisplayAlert("Error", "This record not available for RFC.", "OK");
                }
              
            }
           catch
            {

            }

        }

    }
}
