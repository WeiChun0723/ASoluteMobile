using System;
using System.Collections.Generic;
using System.IO;
using ASolute_Mobile.Models.HaulageViewModel;
using Xamarin.Forms;

namespace ASolute_Mobile.CommonScreen
{
    public partial class ChangeEnterpriseName : ContentPage
    {
        public ChangeEnterpriseName()
        {
            InitializeComponent();

            if (!(Device.RuntimePlatform == Device.iOS))
            {
                NavigationPage.SetHasNavigationBar(this, false);
            }

          
            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }


            BindingContext = new GetBaseURLViewModel();


            if (!(String.IsNullOrWhiteSpace(Ultis.Settings.AppEnterpriseName)))
            {
                nameEntry.Text = Ultis.Settings.AppEnterpriseName;
            }

        }
    }
}
