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
using System.Net;
using Xamarin.Forms;
using System.Threading.Tasks;
using Haulage.Droid;
using ASolute_Mobile.Droid.Services;
using Android.Net.Wifi;
using Plugin.Geolocator;
using Android;

namespace ASolute_Mobile.Droid
{
    [Activity(Label = "AILS Haulage", Icon = "@drawable/appIcon", Theme = "@style/MyTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : FormsAppCompatActivity
    {

        readonly string logTag = "MainActivity";
             
        protected override void OnCreate(Bundle bundle)
        {

            TabLayoutResource = Haulage.Droid.Resource.Layout.Tabbar;
                
            ToolbarResource = Haulage.Droid.Resource.Layout.Toolbar;
            
            base.OnCreate(bundle);

            RequestPermissions(new String[] { Manifest.Permission.AccessFineLocation }, 1);
           
            Rg.Plugins.Popup.Popup.Init(this, bundle);
            Xamarin.Essentials.Platform.Init(this, bundle);
            global::Xamarin.Forms.Forms.Init(this, bundle);
            Xamarin.FormsMaps.Init(this, bundle);
            Xamarin.FormsGoogleMaps.Init(this, bundle);
            UserDialogs.Init(this);
            ZXing.Net.Mobile.Forms.Android.Platform.Init();

            LoadApplication(new App());

            var pm = (PowerManager)GetSystemService(Context.PowerService);
            PowerManager.WakeLock _mWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "PartialWakeLockTag");
            _mWakeLock.Acquire();

            var wf = (WifiManager)GetSystemService(Context.WifiService);
            var _wifiLock = wf.CreateWifiLock(Android.Net.WifiMode.Full, "WifiLockTag");
            _wifiLock.Acquire();


            if(CheckSelfPermission(Manifest.Permission.AccessFineLocation) == Permission.Granted)
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


            App.DisplayScreenWidth = Resources.DisplayMetrics.WidthPixels / Resources.DisplayMetrics.Density;       
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            global::ZXing.Net.Mobile.Android.PermissionsHandler.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if(grantResults.Length > 0 && permissions.Length > 0  )
            {
                if(permissions[0].Equals(Manifest.Permission.AccessFineLocation))
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
           
            Log.Debug(logTag, "OnDestroy: Location app is becoming inactive");
            base.OnDestroy();
           
            LocationApp.StopLocationService();
           
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

