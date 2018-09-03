using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

namespace ASolute_Mobile.Droid
{
    [BroadcastReceiver]
    public class BackgroundCleanUpReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            using (PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "BackgroundCleanUpReceiver"))
            {
                wakeLock.Acquire();
                
                wakeLock.Release();
            }
        }
    }
}
