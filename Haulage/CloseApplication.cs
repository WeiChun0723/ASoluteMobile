using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ASolute_Mobile.Droid;
using ASolute_Mobile.Utils;

[assembly: Xamarin.Forms.Dependency(typeof(CloseApplication))]
namespace ASolute_Mobile.Droid
{
    public class CloseApplication : CloseApp
    {
        public void close_app()
        {
            Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
        }
    }
}