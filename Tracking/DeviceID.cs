using System;
using Android;
using Android.Content.PM;
using Android.Support.V4.App;
using ASolute_Mobile.Utils;
using Haulage.Droid;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(DeviceID))]
namespace Haulage.Droid
{

    public class DeviceID : GetDeviceID
    {

        public string GetIdentifier()
        {

            /* Android.Telephony.TelephonyManager mTelephonyMgr;
             mTelephonyMgr = (Android.Telephony.TelephonyManager)Forms.Context.GetSystemService(Android.Content.Context.TelephonyService);
            return mTelephonyMgr.DeviceId;*/

            return "";
         
        }
    }
}
