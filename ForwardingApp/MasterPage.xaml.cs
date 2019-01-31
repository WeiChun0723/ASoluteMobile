using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;
using ASolute_Mobile.HaulageScreen;
using ASolute_Mobile.Models;
using Plugin.DeviceInfo;
using System;
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
            if(File.Exists(Ultis.Settings.GetAppLogoFileLocation())){
                LogoImage.Source = ImageSource.FromFile(Ultis.Settings.GetAppLogoFileLocation());
            }
            else
            {
                LogoImage.Source = "user_icon.png";

            }
            ownerName.Text = Ultis.Settings.SessionUserItem.UserId;

            appVersion.Text = "App version: " + CrossDeviceInfo.Current.AppVersion; 

            List<SummaryItems> contextMenu = App.Database.GetSummarysAsync("ContextMenu");

            var masterPageItems = new List<MasterPageItem>();

            foreach (SummaryItems item in contextMenu)
              {
                  string option;
                if (item.Display && item.Id != "CurrentLoc")
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
              if (Ultis.Settings.Language.Equals("English"))
              {
                  language.Title = "Language";
              }
              else
              {
                  language.Title = "Bahasa";
              }
              language.IconSource = "language.png";            
              masterPageItems.Add(language);

              MasterPageItem logoff = new MasterPageItem();
              logoff.Id = "LogOff";
              if (Ultis.Settings.Language.Equals("English"))
              {
                  logoff.Title = "Log Off";
              }
              else
              {
                  logoff.Title = "Log Keluar";
              }
              logoff.IconSource = "logout.png";
              masterPageItems.Add(logoff);
            listView.ItemsSource = masterPageItems;

			listView.ItemTapped += (object sender, ItemTappedEventArgs e) => {
				// don't do anything if we just de-selected the row
				if (e.Item == null) return;
				// do something with e.SelectedItem
				((ListView)sender).SelectedItem = null; // de-select the row
			};

            Icon = "hamburger.png";
		}
	}
}
