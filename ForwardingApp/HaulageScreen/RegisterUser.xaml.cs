using System;
using System.Collections.Generic;
using System.IO;
using ASolute_Mobile.Models.HaulageViewModel;
using Xamarin.Forms;

namespace ASolute_Mobile.HaulageScreen
{
    public partial class RegisterUser : ContentPage
    {
        public RegisterUser()
        {
            InitializeComponent();

            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }

            NavigationPage.SetHasNavigationBar(this, false);

            enterpriseEntry.Completed += (s, e) =>
            {
                userIDEntry.Focus();
            };

            userIDEntry.Completed += (s, e) =>
            {
                passwordEntry.Focus();
            };

            passwordEntry.Completed += (s, e) =>
            {
                icEntry.Focus();
            };

            icEntry.Completed += (s, e) =>
            {
                icEntry.Unfocus();
            };


            BindingContext = new RegisterUserViewModel();

            if (!(String.IsNullOrWhiteSpace(Ultis.Settings.AppEnterpriseName)))
            {
                enterpriseEntry.Text = Ultis.Settings.AppEnterpriseName;
            }
        }
    }
}
