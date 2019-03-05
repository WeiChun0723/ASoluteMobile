using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Net.Wifi;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;

namespace ASolute_Mobile.Droid
{
    [BroadcastReceiver]
    public class BackgroundDataSyncReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
        
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);

            using (PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial  , "BackgroundDataSyncReceiver"))
            {
                wakeLock.Acquire();

                Task.Run( () => { BackgroundTask.GetGPS(); });

                //wakeLock.Release();
            }

        } 

    }

}
