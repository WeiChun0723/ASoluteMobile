using ASolute_Mobile.HaulageScreen;
using ASolute_Mobile.Models;
using Plugin.DeviceInfo;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

namespace ASolute_Mobile
{
    public partial class MasterPage : ContentPage
    {
        public ListView ListView { get { return listView; } }

        public MasterPage()
        {
            InitializeComponent();


            LogoImage.Source = (File.Exists(Ultis.Settings.GetAppLogoFileLocation()))? ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation()) : "user_icon.png";

            ownerName.Text = Ultis.Settings.SessionUserItem.UserId;

            appVersion.Text = "App version: " + CrossDeviceInfo.Current.AppVersion;

            List<SummaryItems> contextMenu = App.Database.GetSummarysAsync("ContextMenu");

            var masterPageItems = new List<MasterPageItem>();
            masterPageItems.Clear();
            foreach (SummaryItems item in contextMenu)
            {
                string option;
                if (item.Display)
                {
                    MasterPageItem pageItem = new MasterPageItem();

                    pageItem.Id = item.Id;
                    pageItem.Title = item.Value;
                    option = item.Id;

                    switch (option)
                    {
                        case "CallOffice":
                            pageItem.IconSource = "controller.png";
                            break;

                        case "CallMe":
                            pageItem.IconSource = "call.png";
                            break;

                        case "Panic":
                            pageItem.IconSource = "panic.png";
                            break;

                        case "ChangePwd":
                            pageItem.IconSource = "password.png";
                            pageItem.TargetType = typeof(ChangePasswordPage);


                            break;
                        case "CurrentLoc":
                            pageItem.IconSource = "map.png";
                            pageItem.TargetType = typeof(CurrentLocation);

                            break;

                    }

                    masterPageItems.Add(pageItem);
                }
            }

            /*MasterPageItem provider = new MasterPageItem();
            provider.Id = "AddProvider";
            provider.Title = "Add Service Providers";
            provider.IconSource = "language.png";
            masterPageItems.Add(provider);*/


            MasterPageItem language = new MasterPageItem();
            language.Id = "Language";
            language.Title = (Ultis.Settings.Language.Equals("English")) ? "Language" : "Bahasa";
            language.IconSource = "language.png";
            masterPageItems.Add(language);

            MasterPageItem logoff = new MasterPageItem();
            logoff.Id = "LogOff";
            logoff.Title = (Ultis.Settings.Language.Equals("English")) ? "Log Off" : "Log Keluar";
            logoff.IconSource = "logout.png";
            masterPageItems.Add(logoff);

            listView.ItemsSource = masterPageItems;
            listView.ItemTapped += (object sender, ItemTappedEventArgs e) =>
            {
                // don't do anything if we just de-selected the row
                if (e.Item == null) return;
                // do something with e.SelectedItem
                ((ListView)sender).SelectedItem = null; // de-select the row
            };
            Icon = "hamburger.png";
        }
    }
}
