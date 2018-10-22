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
using XLabs.Ioc;
using XLabs.Ioc.Autofac;
using Plugin.CurrentActivity;

namespace ASolute_Mobile.Droid
{
    [Activity(Label = "AILS Business", Icon = "@drawable/appIcon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {

        readonly string logTag = "MainActivity";
             
        protected override void OnCreate(Bundle bundle)
        {
           
            TabLayoutResource = Haulage.Droid.Resource.Layout.Tabbar;
                
            ToolbarResource = Haulage.Droid.Resource.Layout.Toolbar;
            
            base.OnCreate(bundle);

            Rg.Plugins.Popup.Popup.Init(this, bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.FormsMaps.Init(this, bundle);
            OxyPlot.Xamarin.Forms.Platform.Android.PlotViewRenderer.Init();

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
                      
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
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

        public override void OnBackPressed()
        {
            if (Rg.Plugins.Popup.Popup.SendBackPressed(base.OnBackPressed))
            {
                // Do something if there are some pages in the `PopupStack`
            }
            else
            {
                // Do something if there are not any pages in the `PopupStack`
            }
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

