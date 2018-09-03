using System;
using Android.App;
using Android.Content;
using Android.OS;
using Xamarin.Forms;

namespace ASolute_Mobile.Droid
{
    public class PeriodicService : Service
    {
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            // From shared code or in your PCL
            MessagingCenter.Send<object, string>(this, "JobSync", "Hello from Android");

            return StartCommandResult.NotSticky;
        }
    }
}
