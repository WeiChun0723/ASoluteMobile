using System;
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
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("MzMzNDFAMzEzNjJlMzMyZTMwWGIyZ3BLaHVqMzBINU5TbkFrc2YxVllKeWVTTEM5Q1hZV0p0U0tHS0RJTT0=");

            InitializeComponent();

            if (sessionKey != "")
            {
                MainPage = new MainPage();

            }
            else 
            {
                 MainPage = new CustomNavigationPage(new SplashScreen());
                //MainPage = new CustomNavigationPage(new LoginPage());
            }
          
            //MainPage = new CustomNavigationPage(new CustomerTracking.DataGrid());
            // Ultis.Settings.SessionBaseURI = "https://api.asolute.com/host/api/";
             // MainPage = new CustomNavigationPage(new AppNavigation());

               OneSignal.Current.StartInit("804c5448-99ec-4e95-829f-c98c0ea6acd9")
                        .InFocusDisplaying(Com.OneSignal.Abstractions.OSInFocusDisplayOption.Notification)
                        .HandleNotificationReceived(HandleNotificationReceived)
                        .EndInit();

        }

        void HandleNotificationReceived(OSNotification notification)
        {
            var test = notification;
            Ultis.Settings.NewJob = "Yes";
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
