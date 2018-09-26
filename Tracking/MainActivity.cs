using System;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Android.Util;
using Android.Locations;
using Acr.UserDialogs;
using Xamarin.Forms.Platform.Android;
using Autofac;
using XLabs.Platform.Services.Media;
using Tesseract.Droid;
using Tesseract;
using XLabs.Ioc;
using XLabs.Ioc.Autofac;
using Plugin.CurrentActivity;
using TinyIoC;
using XLabs.Platform.Device;
using XLabs.Ioc.TinyIOC;
using ASolute_Mobile.Droid.Services;
using Android;
using Com.OneSignal;
using Android.Support.V4.App;
using System.Threading.Tasks;
using Android.Widget;
using Xamarin.Forms;

namespace ASolute_Mobile.Droid
{
    [Activity(Label = "AILS Tracking", Icon = "@drawable/appIcon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
       
        readonly string logTag = "MainActivity";
        //private static int MY_PERMISSIONS_REQUEST_READ_PHONE_STATE = 0;

        protected override void OnCreate(Bundle bundle)
        {
            //Ultis.Settings.App = "Tracking";
            TabLayoutResource = Haulage.Droid.Resource.Layout.Tabbar;                
            ToolbarResource = Haulage.Droid.Resource.Layout.Toolbar;
            OneSignal.Current.StartInit("cf7d6e9e-e959-4749-aeba-6846623e8b1a").EndInit();
            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            base.OnCreate(bundle);

            Rg.Plugins.Popup.Popup.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);

            if(Ultis.Settings.DeviceUniqueID.Equals(""))
            {
                TryToGetPermissions();
            }

            /*var container = TinyIoCContainer.Current;
            container.Register<IDevice>(AndroidDevice.CurrentDevice);
            container.Register<ITesseractApi>((cont, parameters) =>
            {
                return new TesseractApi(ApplicationContext, AssetsDeployment.OncePerInitialization);
            });

            Resolver.SetResolver(new TinyResolver(container));*/

            UserDialogs.Init(this);

            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            LoadApplication(new App());


            var backgroundDataSyncPendingIntent = PendingIntent.GetBroadcast(this, 0, new Intent(this, typeof(BackgroundDataSyncReceiver)), PendingIntentFlags.UpdateCurrent);
            var alarmManagerBackgroundDataSync = GetSystemService(AlarmService).JavaCast<AlarmManager>();
            alarmManagerBackgroundDataSync.SetRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime(), 60000, backgroundDataSyncPendingIntent);

            var cleanUpPendingIntent = PendingIntent.GetBroadcast(this, 1, new Intent(this, typeof(BackgroundCleanUpReceiver)), PendingIntentFlags.UpdateCurrent);
            var alarmManagercleanUpPending = GetSystemService(AlarmService).JavaCast<AlarmManager>();
            DateTime midnightDateTime = DateTime.Today;
            TimeSpan ts = new TimeSpan(0, 0, 0);
            midnightDateTime = midnightDateTime.Date + ts;
            alarmManagercleanUpPending.SetRepeating(AlarmType.RtcWakeup, midnightDateTime.Ticks, AlarmManager.IntervalDay, cleanUpPendingIntent);

            App.DisplayScreenWidth = Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density;

       
            //LocationApp.StartLocationService();     

        }

        #region RuntimePermissions

        async Task TryToGetPermissions()
        {
            if ((int)Build.VERSION.SdkInt >= 23)
            {
                await GetPermissionsAsync();
                return;
            }


        }
        const int RequestLocationId = 0;

        readonly string[] PermissionsGroupLocation =
            {
                            //TODO add more permissions
                            Manifest.Permission.ReadPhoneState

        };

        async Task GetPermissionsAsync()
        {
            const string permission = Manifest.Permission.ReadPhoneState;

            if (CheckSelfPermission(permission) == (int)Android.Content.PM.Permission.Granted)
            {
                //TODO change the message to show the permissions name
                Android.Telephony.TelephonyManager mTelephonyMgr;
                mTelephonyMgr = (Android.Telephony.TelephonyManager)Forms.Context.GetSystemService(Android.Content.Context.TelephonyService);
                Ultis.Settings.DeviceUniqueID =  mTelephonyMgr.DeviceId;
               
            }

            if (ShouldShowRequestPermissionRationale(permission))
            {
                //set alert for executing the task
                AlertDialog.Builder alert = new AlertDialog.Builder(this);
                alert.SetTitle("Permissions Needed");
                alert.SetMessage("The application need special permissions to continue");
                alert.SetPositiveButton("Request Permissions", (senderAlert, args) =>
                {
                    RequestPermissions(PermissionsGroupLocation, RequestLocationId);
                });

                alert.SetNegativeButton("Cancel", (senderAlert, args) =>
                {
                    Toast.MakeText(this, "Cancelled!", ToastLength.Short).Show();
                });

                Dialog dialog = alert.Create();
                dialog.Show();


                return;
            }

            RequestPermissions(PermissionsGroupLocation, RequestLocationId);

        }
      

        #endregion


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case RequestLocationId:
                    {
                        if (grantResults[0] == (int)Android.Content.PM.Permission.Granted)
                        {
                            Android.Telephony.TelephonyManager mTelephonyMgr;
                            mTelephonyMgr = (Android.Telephony.TelephonyManager)Forms.Context.GetSystemService(Android.Content.Context.TelephonyService);
                            Ultis.Settings.DeviceUniqueID = mTelephonyMgr.DeviceId;

                        }
                        else
                        {
                            //Permission Denied :(
                           // Toast.MakeText(this, "Special permissions denied", ToastLength.Short).Show();

                        }
                    }
                    break;
            }

            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }       

        protected override void OnPause()
        {
            //if(Ultis.Settings.SessionUserItem.CaptureGPS){
            Log.Debug(logTag, "OnPause: Location app is moving to background");
            base.OnPause();
            //}
        }


        protected override void OnResume()
        {
            //if (Ultis.Settings.SessionUserItem.CaptureGPS)
            //{
            Log.Debug(logTag, "OnResume: Location app is moving into foreground");
            base.OnResume();
          
            //}
        }

        protected override void OnDestroy()
        {
            //if (Ultis.Settings.SessionUserItem.CaptureGPS)
            //{
            Log.Debug(logTag, "OnDestroy: Location app is becoming inactive");
            base.OnDestroy();

            // Stop the location service:
            //LocationApp.StopLocationService();
            //}
        }

        public void HandleLocationChanged(object sender, LocationChangedEventArgs e)
        {
            Android.Locations.Location location = e.Location;
            Log.Debug(logTag, "Foreground updating");
           
            App.gpsLocationLat = location.Latitude;
            App.gpsLocationLong = location.Longitude;
                                 
        }

        public void HandleProviderDisabled(object sender, ProviderDisabledEventArgs e)
        {
            Log.Debug(logTag, "Location provider disabled event raised");
        }

        public void HandleProviderEnabled(object sender, ProviderEnabledEventArgs e)
        {
            Log.Debug(logTag, "Location provider enabled event raised");
        }

        public void HandleStatusChanged(object sender, StatusChangedEventArgs e)
        {
            Log.Debug(logTag, "Location status changed, event raised");
        }

    }

   
}

