using System;
using System.Net.Http;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using ASolute.Mobile.Models;
using ASolute_Mobile;
using ASolute_Mobile.Droid;
using ASolute_Mobile.Utils;
using Newtonsoft.Json;
using Xamarin.Forms;

namespace Haulage.Droid.Services
{
    [Service]
    public class GPS_Service : Service
    {
        private static LocationApp current;

        public static LocationApp Current
        {
            get { return current; }
        }



        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnCreate()
        {
            base.OnCreate();


        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
           /* string channelId = "";
            Notification notification = null;
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {

                channelId = createNotificationChannel("my_channel", "my_background_service");
                notification = new Notification.Builder(this, channelId)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("GPS Tracking")
                .SetContentText("Sending location.")
                .SetSmallIcon(Resource.Drawable.appIcon)
                .Build();
            }
            else
            {

                notification = new Notification.Builder(this)
                .SetContentIntent(pendingIntent)
                .SetContentTitle("GPS Tracking")
                .SetContentText("Sending location.")
                .SetSmallIcon(Resource.Drawable.appIcon)
                .Build();
            }

            StartForeground((int)NotificationFlags.ForegroundService, notification);*/

            Device.StartTimer(TimeSpan.FromSeconds(60),  ()  =>
            {

                Task.Run( async () => {
                    //BackgroundTask.GetGPS();

                    string location = (App.gpsLocationLat.Equals(0) || App.gpsLocationLong.Equals(0)) ? "0,0" : String.Format("{0:0.0000}", App.gpsLocationLat) + "," + String.Format("{0:0.0000}", App.gpsLocationLong);

                     if (!(String.IsNullOrEmpty(ASolute_Mobile.Ultis.Settings.SessionSettingKey)))
                     {
                         var client = new HttpClient();
                         client.BaseAddress = new Uri(ASolute_Mobile.Ultis.Settings.SessionBaseURI);
                         var sendGPSURI = ControllerUtil.getGPSTracking(location, "");
                         var content = await client.GetAsync(sendGPSURI);
                         var response = await content.Content.ReadAsStringAsync();
                         clsResponse gps_response = JsonConvert.DeserializeObject<clsResponse>(response);

                     }

                    await Task.Delay(600000);
                });

                return true;
            });
          

            return StartCommandResult.Sticky;
        }

        private string createNotificationChannel(string channel_id, string channel_name)
        {
            var chan = new NotificationChannel(channel_id, channel_name, NotificationImportance.None);
            chan.LockscreenVisibility = NotificationVisibility.Private;
            var service = GetSystemService(Context.NotificationService) as NotificationManager;
            service.CreateNotificationChannel(chan);


            return channel_id;
        }

      
    }
}
