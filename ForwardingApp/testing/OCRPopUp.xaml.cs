using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Forms;
using Rg.Plugins.Popup.Services;


namespace ASolute_Mobile
{
    public partial class OCRPopUp : PopupPage
    {
        public OCRPopUp(string possibleResult)
        {
            InitializeComponent();


            ocrResult.Text = possibleResult;
            /*foreach(String suggest in possibleResult)
            {
                resultPicker.Items.Add(suggest);
            }*/
        }

      /*  void Handle_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            ocrResult.Text = resultPicker.Items[resultPicker.SelectedIndex];
        }*/

        public async void ConfirmResult(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync(true);
            MessagingCenter.Send<App,string>((App)Application.Current, "ResultReturn", ocrResult.Text);

        }
    }
}
