using System;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;
using ASolute_Mobile.Droid;
using ASolute_Mobile.ZebraPrinter;
using Haulage.Droid.MobilePrinter;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;

[assembly: Xamarin.Forms.Dependency(typeof(PrinterDiscoveryImplementation))]
namespace Haulage.Droid.MobilePrinter
{

    public class PrinterDiscoveryImplementation : IPrinterDiscovery
    {
        public readonly string[] PermissionsLocation = { Manifest.Permission.AccessCoarseLocation };
        public const int RequestLocationId = 0;

        public static IDiscoveryHandler TempHandler { get; set; }

        public PrinterDiscoveryImplementation() { }

        public void CancelDiscovery()
        {
            if (BluetoothAdapter.DefaultAdapter.IsDiscovering)
            {
                BluetoothAdapter.DefaultAdapter.CancelDiscovery();
                System.Diagnostics.Debug.WriteLine("Cancelling discovery...");
            }
        }

        public void FindBluetoothPrinters(IDiscoveryHandler handler)
        {
            const string permission = Manifest.Permission.AccessCoarseLocation;
            if (ContextCompat.CheckSelfPermission(Application.Context, permission) == (int)Permission.Granted)
            {
                BluetoothDiscoverer.Current.FindPrinters(Application.Context, handler);
                return;
            }

            TempHandler = handler;
            // Finally request permissions with the list of permissions and ID
            ActivityCompat.RequestPermissions(MainActivity.GetActivity(), PermissionsLocation, RequestLocationId);
        }

        public void FindUsbPrinters(IDiscoveryHandler handler)
        {
            UsbDiscoverer.Current.FindPrinters(Application.Context, handler);
        }

        public void RequestUsbPermission(IDiscoveredPrinterUsb printer)
        {
            if (!printer.HasPermissionToCommunicate)
            {
                printer.RequestPermission(Application.Context);
            }
        }
    }
}
