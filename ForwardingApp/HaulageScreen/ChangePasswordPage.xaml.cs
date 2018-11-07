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
            }
            else
            {
                Title = "Tukar kata laluan";
            }

            newPasswordEntry.Completed += (s, e) =>
            {
                //usernameEntry.Unfocus();
                confirmPasswordEntry.Focus();
            };

            if (Ultis.Settings.App == "Transport")
            {
                chgPassTitle.Text = "ASolute Transport";
                
            }
            else if (Ultis.Settings.App == "Fleet")
            {
                chgPassTitle.Text = "ASolute Fleet";
            }
            else if (Ultis.Settings.App == "Courier")
            {
                chgPassTitle.Text = "ASolute Courier";
            }

        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (Ultis.Settings.NewJob.Equals("Yes"))
            {
                CommonFunction.CreateToolBarItem(this);
            }
            else
            {
                this.ToolbarItems.Clear();
            }

            MessagingCenter.Subscribe<App>((App)Application.Current, "Testing", (sender) => {

                try
                {
                    CommonFunction.NewJobNotification(this);
                }
                catch (Exception e)
                {
                    DisplayAlert("Notification error", e.Message, "OK");
                }
            });
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");
        }

        protected override bool OnBackButtonPressed()
        {
            Application.Current.MainPage = new MainPage();
            return true;
        }

        async void ChangePassword_Clicked(object sender, EventArgs e)
        {

            if (!(string.IsNullOrEmpty(newPasswordEntry.Text) && string.IsNullOrEmpty(confirmPasswordEntry.Text)))
            {
                if(newPasswordEntry.Text.Equals(confirmPasswordEntry.Text)){
                             
                    string encryptedNewPassword = clsCommonFunc.AES_Encrypt(newPasswordEntry.Text);

                    var client = new HttpClient();
                    client.BaseAddress = new Uri(Ultis.Settings.SessionBaseURI);
                    var uri = ControllerUtil.getChangePasswordURL(encryptedNewPassword);
                    var response = await client.GetAsync(uri);
                    var reply = await response.Content.ReadAsStringAsync();
                    clsResponse json_response = JsonConvert.DeserializeObject<clsResponse>(reply);

                    if(json_response.IsGood == true)
                    {
                        await DisplayAlert("Change Password", "Change Password is Successful! Please login again.", "Okay");
                        //BackgroundTask.Logout(this);
                        Application.Current.MainPage = new MainPage();
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
    }
}
