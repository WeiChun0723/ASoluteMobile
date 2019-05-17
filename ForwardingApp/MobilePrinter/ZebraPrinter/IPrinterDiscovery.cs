using System;
using LinkOS.Plugin.Abstractions;

namespace ASolute_Mobile.ZebraPrinter
{
    public interface IPrinterDiscovery
    {
        void FindBluetoothPrinters(IDiscoveryHandler handler);

        void FindUsbPrinters(IDiscoveryHandler handler);

        void RequestUsbPermission(IDiscoveredPrinterUsb printer);

        void CancelDiscovery();
    }
}
