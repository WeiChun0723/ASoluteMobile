using System;
using System.Collections.Generic;
using System.IO;
using ASolute.Mobile.Models;
using ASolute_Mobile.CommonScreen;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class NewMainMenu : ContentPage
    {
        Label title1, title2;
        List<clsKeyValue> checkItems = new List<clsKeyValue>();

        public NewMainMenu()
        {
            InitializeComponent();

            StackLayout main = new StackLayout();

            title1 = new Label
            {
                FontSize = 15,
                Text = (Ultis.Settings.Language.Equals("English")) ? "Main Menu" : "Menu Utama",
                TextColor = Color.White
            };

            title2 = new Label
            {
                FontSize = 10,
                Text = Ultis.Settings.SubTitle,
                TextColor = Color.White
            };

            main.Children.Add(title1);
            main.Children.Add(title2);

            NavigationPage.SetTitleView(this, main);

            GetMainMenu();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            var image = App.Database.GetUserProfilePicture(Ultis.Settings.SessionUserItem.DriverId);

            profilePicture.Source = (image != null && image.imageData != null) ? ImageSource.FromStream(() => new MemoryStream(image.imageData)) : "user_icon.png";
        }

        async void Handle_Tapped(object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new UserInfo());
        }

        async void GetMainMenu()
        {
            loading.IsVisible = true;

            try
            {
                var content = await CommonFunction.CallWebService(0, null, Ultis.Settings.SessionBaseURI, ControllerUtil.getDownloadMenuURL(), this);
                clsResponse response = JsonConvert.DeserializeObject<clsResponse>(content);

                if (response != null)
                {
                    var menu = JObject.Parse(content)["Result"].ToObject<clsLogin>();

                    Ultis.Settings.SubTitle = menu.SubTitle;
                    title2.Text = Ultis.Settings.SubTitle;

                    //load value from the menu in json response "CheckList"
                    for (int check = 0; check < response.Result["Checklist"].Count; check++)
                    {
                        string itemKey = response.Result["Checklist"][check]["Key"];
                        string itemValue = response.Result["Checklist"][check]["Value"];

                        checkItems.Add(new clsKeyValue(itemKey, itemValue));
                    }

                    // clear the db before insert to it to prevent duplicate
                    App.Database.deleteRecords("MainMenu");
                    App.Database.deleteRecordSummary("MainMenu");

                    foreach (clsDataRow mainMenu in menu.MainMenu)
                    {
                        switch (mainMenu.Id)
                        {
                            //display user info
                            case "Info":
                                userInfo.Children.Clear();
                                foreach (clsCaptionValue userSummary in mainMenu.Summary)
                                {
                                    string labelStyle = (userSummary.Caption == "") ? "ProfileNameLabel" : "ProfileTagLabel";

                                    Label info = new Label
                                    {
                                        Style = (Xamarin.Forms.Style)Application.Current.Resources[labelStyle],

                                    };

                                    info.Text = (userSummary.Caption == "") ? userSummary.Value : userSummary.Caption + ": " + userSummary.Value;

                                    userInfo.Children.Add(info);
                                }
                                break;

                            //display expiry date info
                            case "Expiry":
                                expiryStack.IsVisible = true;
                                mainGrid.Children.Add(expiryStack, 0, 2);
                                expiryLabel.Text = mainMenu.Caption;
                                expiryGrid.Children.Clear();
                                int rowIndex = 0, columnIndex = 0;
                                foreach (clsCaptionValue expiryInfo in mainMenu.Summary)
                                {
                                    if (expiryInfo.Caption != "")
                                    {
                                        expiryGrid.ColumnDefinitions.Add(new ColumnDefinition());
                                        StackLayout expiryInfoStack = new StackLayout
                                        {
                                            BackgroundColor = Color.FromHex(mainMenu.BackColor)
                                        };


                                        Label date = new Label
                                        {
                                            Style = (Xamarin.Forms.Style)Application.Current.Resources["StatsNumberLabel"],
                                            Text = expiryInfo.Value,
                                            TextColor = Color.FromHex("#696969")
                                        };

                                        Label caption = new Label
                                        {
                                            Style = (Xamarin.Forms.Style)Application.Current.Resources["StatsCaptionLabel"],
                                            Text = expiryInfo.Caption,
                                            TextColor = Color.FromHex("#696969")
                                        };

                                        expiryInfoStack.Children.Add(date);
                                        expiryInfoStack.Children.Add(caption);

                                        expiryGrid.Children.Add(expiryInfoStack, columnIndex, rowIndex);
                                        columnIndex++;
                                    }

                                }
                                break;
                        }

                    }

                }

            }
            catch (Exception ex)
            {
                // await DisplayAlert("Exception", ex.Message, "OK");
            }
        }
    }
}
