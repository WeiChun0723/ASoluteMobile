using System;
using System.Threading.Tasks;
using Android.Content;
using Android.OS;

namespace ASolute_Mobile.Droid
{
    [BroadcastReceiver]
    public class BackgroundDataSyncReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            using (PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "BackgroundDataSyncReceiver"))
            {
                wakeLock.Acquire();
                //Task.Run(async () => { await BackgroundTask.BackgroundUploadImage(); });
                // Task.Run(async () => { await BackgroundTask.DownloadLatestRecord(null);}).Wait();
                //Task.Run(async () => { await BackgroundTask.UploadLatestRecord(); });
              
                wakeLock.Release();
            }
        } 
    }
}
