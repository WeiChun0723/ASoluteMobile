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

                if (Ultis.Settings.App.Equals("asolute.Mobile.AILSBUS"))
                {
                    //Task.Run(async () => { await BackgroundTask.DownloadBusStopList(); });
                    Task.Run(async () => { await BackgroundTask.UploadPendingRecord(); });
                }
                else if(Ultis.Settings.App.Equals("asolute.Mobile.Forwarding") || Ultis.Settings.App.Equals("com.asolute.Forwarding"))
                {
                    Task.Run(async () => { await BackgroundTask.DownloadLatestJobs(null); });
                    Task.Run(async () => { await BackgroundTask.UploadLatestJobs(); }).Wait();
                }

                wakeLock.Release();
            }

        } 

    }

}
