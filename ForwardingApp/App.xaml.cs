using System;
using Xamarin.Forms.Xaml;
using System.Diagnostics;
using System.Text;
using PCLCrypto;
using System.Net.Http;
using Newtonsoft.Json;
using Xamarin.Forms;
using ASolute_Mobile.Data;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using ASolute_Mobile.Utils;
using ASolute_Mobile.Ultis;
using ASolute_Mobile.Courier;
using ASolute.Mobile.Models;
using ASolute_Mobile.CustomerTracking;

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
            InitializeComponent();

            /* if (sessionKey != "")
             {
                 MainPage = new MainPage();
             }
             else 
             {
                 //MainPage = new CustomNavigationPage(new SplashScreen());
                 Ultis.Settings.SessionBaseURI = "https://api.asolute.com/host/api/";
                 MainPage = new CustomNavigationPage(new AppNavigation());

             }*/
            Ultis.Settings.SessionBaseURI = "https://api.asolute.com/host/api/";
            MainPage = new CustomNavigationPage(new AppNavigation());

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
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        public static void DropDatabase()
        {
            database.DropDB();
            database = null;
        }
    }
}
