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

            if (sessionKey != "")
            {
                MainPage = new MainPage();
            }
            else
            {
                MainPage = new CustomNavigationPage(new SplashScreen());
                //MainPage = new CustomNavigationPage(new LoginPage());
                
            }

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
            // Handle string that contain driver name and owner name that will attach to the crash report
            /*Crashes.GetErrorAttachments = (ErrorReport report) =>
            {
                string userinfo = "";
                if (Ultis.Settings.CrashInLoginPage == "Yes" )
                {
                userinfo = "App ID: " + Ultis.Settings.GeneratedAppID + Environment.NewLine + "App crash in login page.";
                }
                
                else
                {
                 
                userinfo = "App ID: " + Ultis.Settings.GeneratedAppID + Environment.NewLine + "==============================" + Environment.NewLine +
                "Owner name: " + Ultis.Settings.SessionUserItem.CompanyName + Environment.NewLine + "Driver name: " + Ultis.Settings.SessionDriveName +
                Environment.NewLine + "Account ID : " + Ultis.Settings.SessionUserId;                      
                }

                return new ErrorAttachmentLog[]
                {
                   ErrorAttachmentLog.AttachmentWithText(userinfo,"Info")
                };
                
            };

            AppCenter.Start("android=b4e96eda-a89f-499f-a407-7f81c9f06e15;",
            typeof(Analytics), typeof(Crashes));*/
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
