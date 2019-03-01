using System;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using ASolute_Mobile.Droid;

namespace Haulage.Droid
{
    [Service]
    public class GPS_Service : Service
    {
        public GPS_Service()
        {
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
            var pendingIntent = PendingIntent.GetActivity(this, 0, new Intent(this, typeof(MainActivity)), 0);
            var notification = new Notification.Builder(this)
            .SetContentIntent(pendingIntent)
            .SetContentTitle("GPS Tracking")
            .SetContentText("App sending location.")
            .SetSmallIcon(Resource.Drawable.appIcon)
            .Build();
            StartForeground((int)NotificationFlags.ForegroundService, notification);
            return StartCommandResult.Sticky;
        }


    }
}
