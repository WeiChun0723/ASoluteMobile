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
using ASolute_Mobile.Droid.Services;
using Android;
using ImageCircle.Forms.Plugin.Droid;
using Haulage.Droid.MobilePrinter;

namespace ASolute_Mobile.Droid
{

    [Activity(Label = "ASolute Fleet", Icon = "@drawable/appIcon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {
        private static Activity activity;
        readonly string logTag = "MainActivity";

        protected override void OnCreate(Bundle bundle)
        {
            try
            {
                TabLayoutResource = Haulage.Droid.Resource.Layout.Tabbar;
                ToolbarResource = Haulage.Droid.Resource.Layout.Toolbar;
                base.OnCreate(bundle);
               
                Xamarin.Essentials.Platform.Init(this, bundle);
                Rg.Plugins.Popup.Popup.Init(this, bundle);
                global::Xamarin.Forms.Forms.Init(this, bundle);
                Xamarin.FormsMaps.Init(this, bundle);
                Xamarin.FormsGoogleMaps.Init(this, bundle);
                UserDialogs.Init(this);
                ImageCircleRenderer.Init();
                ZXing.Net.Mobile.Forms.Android.Platform.Init();
                ZXing.Mobile.MobileBarcodeScanner.Initialize(Application);
                Ultis.Settings.App = PackageName;

                if ( Build.VERSION.SdkInt >= BuildVersionCodes.M)
                {
                    if (PackageName.Equals("asolute.Mobile.AILSHaulage"))
                    {
                        RequestPermissions(new String[] { Manifest.Permission.AccessFineLocation }, 1);
                    }

                    if (CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted && PackageName.Equals("asolute.Mobile.AILSHaulage"))
                    {
                        StartLocationTracking();
                    }
                }

                if(PackageName.Equals("asolute.Mobile.AILSBUS"))
                {
                    var backgroundDataSyncPendingIntent = PendingIntent.GetBroadcast(this, 0, new Intent(this, typeof(BackgroundDataSyncReceiver)), PendingIntentFlags.UpdateCurrent);
                    var alarmManagerBackgroundDataSync = GetSystemService(AlarmService).JavaCast<AlarmManager>();
                    alarmManagerBackgroundDataSync.SetRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime(), 60000, backgroundDataSyncPendingIntent);
                }

                App.DisplayScreenWidth = Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density;
                App.DisplayScreenHeight = Resources.DisplayMetrics.HeightPixels / Resources.DisplayMetrics.Density;

                LoadApplication(new App());
            }
            catch
            {

            }
        }

        internal static Activity GetActivity()
        {
            return activity;
        }

        public void StartLocationTracking()
        {

            LocationApp.Current.LocationServiceConnected += (object sender, ServiceConnectedEventArgs e) =>
            {
                Log.Debug(logTag, "ServiceConnected Event Raised");
                // notifies us of location changes from the system
                LocationApp.Current.LocationService.LocationChanged += HandleLocationChanged;
                //notifies us of user changes to the location provider (ie the user disables or enables GPS)
                LocationApp.Current.LocationService.ProviderDisabled += HandleProviderDisabled;
                LocationApp.Current.LocationService.ProviderEnabled += HandleProviderEnabled;
                // notifies us of the changing status of a provider (ie GPS no longer available)
                LocationApp.Current.LocationService.StatusChanged += HandleStatusChanged;
            };

            LocationApp.StartLocationService();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            Plugin.Permissions.PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            switch (requestCode)
            {
                case PrinterDiscoveryImplementation.RequestLocationId:
                    if (grantResults[0] == Permission.Granted)
                    {
                        //Permission granted
                        PrinterDiscoveryImplementation discoveryImp = new PrinterDiscoveryImplementation();
                        discoveryImp.FindBluetoothPrinters(PrinterDiscoveryImplementation.TempHandler);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Location permission denied. Cannot do Bluetooth discovery.");
                    }
                    break;
            }

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (grantResults.Length > 0 && permissions.Length > 0)
                {
                    if (permissions[0].Equals(Manifest.Permission.AccessFineLocation) && PackageName.Equals("asolute.Mobile.AILSHaulage"))
                    {
                        StartLocationTracking();
                    }
                }
            }
           
        }

        protected override void OnPause()
        {
            Log.Debug(logTag, "OnPause: Location app is moving to background");
            base.OnPause();
        }

        protected override void OnResume()
        {
            Log.Debug(logTag, "OnResume: Location app is moving into foreground");
            base.OnResume();
        }

        protected override void OnDestroy()
        {

            //Log.Debug(logTag, "OnDestroy: Location app is becoming inactive");
            base.OnDestroy();

            //LocationApp.StopLocationService();
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

