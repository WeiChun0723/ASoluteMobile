using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using ASolute.Mobile.Models;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class ChangePasswordPage : ContentPage
    {
        string sessionKey = Ultis.Settings.SessionSettingKey;

        public ChangePasswordPage()
        {
            InitializeComponent();
            if (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))
            {
                logoImageHolder.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }

            if (Ultis.Settings.Language.Equals("English"))
            {
                Title = "Change Password";
                newPassView.Hint = "New Password";
                confirmPassView.Hint = "New Password";
                SubmitButton.Text = "Confirm";
                BackButton.Text = "Back to Menu";
            }
            else
            {
                Title = "Tukar kata laluan";
                newPassView.Hint = "Kata laluan baru";
                confirmPassView.Hint = "Kata laluan baru";
                SubmitButton.Text = "Sahkan";
                BackButton.Text = "Menu Utama";
            }

            newPasswordEntry.Completed += (s, e) =>
            {
                confirmPasswordEntry.Focus();
            };

            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new MainPage();
            return true;
        }

        void BackButton_Clicked(object sender, EventArgs e)
        {
            Application.Current.MainPage = new MainPage();
        }

        async void ChangePassword_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!(string.IsNullOrEmpty(newPasswordEntry.Text) && string.IsNullOrEmpty(confirmPasswordEntry.Text)))
                {
                    if (newPasswordEntry.Text.Equals(confirmPasswordEntry.Text))
                    {

                        string encryptedNewPassword = clsCommonFunc.AES_Encrypt(newPasswordEntry.Text);

                        var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getChangePasswordURL(encryptedNewPassword));
                        clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(content);

                        if (json_response.IsGood == true)
                        {
                            string response = (Ultis.Settings.Language.Equals("English")) ? "Change Password is Successful! Please login again." : "Berjaya ! Sila login semula.";

                            await DisplayAlert("", response, "OK");

                            BackgroundTask.Logout(this);

                        }
                        else
                        {
                            await DisplayAlert("Fail", json_response.Message, "OK");
                        }

                    }
                    else
                    {
                        await DisplayAlert("New Password Not Match", "Entered New Password and Confirm New Password must be the same.", "Okay");
                    }

                }
                else
                {
                    await DisplayAlert("Field missing", "Entered all field before proceed.", "Okay");
                }
            }
            catch(Exception)
            {
               
            }
        }
    }
}
