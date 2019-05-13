using System;
using Android.OS;

namespace ASolute_Mobile.Droid.Services
{
    public class ServiceConnectedEventArgs : EventArgs
    {
        public IBinder Binder { get; set; }
    }
}