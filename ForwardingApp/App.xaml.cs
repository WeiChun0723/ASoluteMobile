﻿using System;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.Text;
using PCLCrypto;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Data;
using ASolute_Mobile.CustomerTracking;
using Com.OneSignal;
using Com.OneSignal.Abstractions;
using System.Collections.Generic;
using ASolute_Mobile.Models;
using ASolute_Mobile.HaulageScreen;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace ASolute_Mobile
{
    public partial class App : Application
    {
        static Database database;
        public static string databaseLocation;
        string sessionKey = Ultis.Settings.SessionSettingKey;
        public static double gpsLocationLat {get; set; }
        public static double gpsLocationLong {get; set; }
        public static double DisplayScreenWidth { get; set; }

        public App()
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDU4MTBAMzEzNjJlMzMyZTMwT1FtdENrWjVXVVZGN2dVQ0pxRHRmMkJ1aEpINzJIWklDdVdTdmEyU2I3VT0=");

            InitializeComponent();

           MainPage = new TestingOCR();

            /* if (sessionKey != "")
             {
                 MainPage = new MainPage();

             }
             else 
             {
                 if(Ultis.Settings.AppFirstInstall == "First")
                 {
                     MainPage = new CustomNavigationPage(new SplashScreen());
                 }
                 else
                 {
                     MainPage = new CustomNavigationPage(new LoginPage());
                 }

                // MainPage = new CustomNavigationPage(new LoginPage());
             }


            Ultis.Settings.SessionBaseURI = "https://api.asolute.com/host/api/";
             MainPage = new CustomNavigationPage(new AppNavigation());

            OneSignal.Current.StartInit("804c5448-99ec-4e95-829f-c98c0ea6acd9")
                         .InFocusDisplaying(Com.OneSignal.Abstractions.OSInFocusDisplayOption.Notification)
                         .HandleNotificationReceived(HandleNotificationReceived)
                         .EndInit();*/

        }

        void HandleNotificationReceived(OSNotification notification)
        {

          /*if(!String.IsNullOrEmpty(notification.payload.body))
            {
              

                ChatRecord chat = new ChatRecord
                {
                    Content = notification.payload.body ,
                    Sender = "OtherPPL",
                    updatedDate = DateTime.Now ,
                    BackgroundColor = "#cedcff"
                };

                App.Database.SaveChat(chat);

                MessagingCenter.Send<App>((App)Application.Current, "Testing");
            }*/

            Ultis.Settings.NewJob = "Yes";
            Ultis.Settings.UpdatedRecord = "RefreshJobList";
            Ultis.Settings.RefreshMenuItem = "Yes";
            MessagingCenter.Send<App>((App)Application.Current, "Testing");
        }

        public static Database Database
        {
            get
            {
                if (database == null)
                {
                    databaseLocation = DependencyService.Get<IFileHelper>().GetLocalFilePath("TodoSQLite.db3");
                    database = new Database(databaseLocation);
                }
                return database;
            }
        }

        protected override void OnStart()
        {
           
        }

        protected override void OnSleep()
        {
           
        }

        protected override void OnResume()
        {
         
        }

        public static void DropDatabase()
        {
            database.DropDB();
            database = null;
        }
    }
}
