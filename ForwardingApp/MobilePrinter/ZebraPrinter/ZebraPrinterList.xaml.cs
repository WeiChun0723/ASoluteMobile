using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;
using Plugin.Connectivity.Abstractions;
using Xamarin.Forms;

namespace ASolute_Mobile.ZebraPrinter
{
    public partial class ZebraPrinterList : ContentPage
    {
        public delegate void PrinterSelectedHandler(IDiscoveredPrinter printer);
        public static event PrinterSelectedHandler OnPrinterSelected;


        ObservableCollection<IDiscoveredPrinter> printers;

        public ZebraPrinterList()
        {
            InitializeComponent();

            printers = new ObservableCollection<IDiscoveredPrinter>();

            printerList.ItemsSource = printers;

            Title = "Choose printer to connect.";        
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            new Task(new Action(() => {
                if (printers != null && printers.Count > 0)
                {
                    printers.Clear();
                }
                StartDiscovery(ConnectionType.USB);
            })).Start();
        }

        private void StartDiscovery(ConnectionType connectionType)
        {
            UpdateStatus($"Discovering {connectionType.ToString()} printers...");

            try
            {
                System.Diagnostics.Debug.WriteLine($"Starting {connectionType.ToString()} discovery...");

                switch (connectionType)
                {
                    case ConnectionType.Bluetooth:
                        DependencyService.Get<IPrinterDiscovery>().FindBluetoothPrinters(new DiscoveryHandlerImplementation(this, ConnectionType.Bluetooth));
                        break;

                    case ConnectionType.Network:
                        NetworkDiscoverer.Current.LocalBroadcast(new DiscoveryHandlerImplementation(this, ConnectionType.Network));
                        break;

                    case ConnectionType.USB:
                        DependencyService.Get<IPrinterDiscovery>().FindUsbPrinters(new DiscoveryHandlerImplementation(this, ConnectionType.USB));
                        break;
                }
            }
            catch (Exception e)
            {
                if (e is NotImplementedException && connectionType == ConnectionType.USB)
                {
                    StartDiscovery(ConnectionType.Bluetooth);
                }
                else
                {
                    string errorMessage = $"Error discovering {nameof(connectionType)} printers: {e.Message}";
                    System.Diagnostics.Debug.WriteLine(errorMessage);
                    ShowErrorAlert(errorMessage);
                }
            }
        }

        private void ShowErrorAlert(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                if(App.myPrinter == null)
                {
                    DisplayAlert("Error", "Please select a printer.", "OK");
                }
                else
                {
                    DisplayAlert("Error", message, "OK");
                }

            });
        }

        private void UpdateStatus(string message)
        {
            Device.BeginInvokeOnMainThread(() => {
                statusLbl.Text = message;
            });
        }

        private class DiscoveryHandlerImplementation : IDiscoveryHandler
        {

            private ZebraPrinterList selectPrinterPage;
            private ConnectionType connectionType;

            public DiscoveryHandlerImplementation(ZebraPrinterList selectPrinterPage, ConnectionType connectionType)
            {
                this.selectPrinterPage = selectPrinterPage;
                this.connectionType = connectionType;
            }

            public void DiscoveryError(string message)
            {
                System.Diagnostics.Debug.WriteLine($"Error discovering {Enum.GetName(typeof(ConnectionType), connectionType)} printers: {message}");
                selectPrinterPage.ShowErrorAlert(message);

                if (connectionType == ConnectionType.USB)
                {
                    selectPrinterPage.StartDiscovery(ConnectionType.Bluetooth);
                }
                else if (connectionType == ConnectionType.Bluetooth)
                {
                    selectPrinterPage.StartDiscovery(ConnectionType.Network);
                }
                else
                    selectPrinterPage.UpdateStatus("Discovery finished");
            }

            public void DiscoveryFinished()
            {
                System.Diagnostics.Debug.WriteLine($"Finished discovering {Enum.GetName(typeof(ConnectionType), connectionType)} printers");
                if (connectionType == ConnectionType.USB)
                {
                    selectPrinterPage.StartDiscovery(ConnectionType.Bluetooth);
                }
                else if (connectionType == ConnectionType.Bluetooth)
                {
                    selectPrinterPage.StartDiscovery(ConnectionType.Network);
                }
                else
                    selectPrinterPage.UpdateStatus("Discovery finished");
            }

            public void FoundPrinter(IDiscoveredPrinter discoveredPrinter)
            {
                System.Diagnostics.Debug.WriteLine($"Found printer: {discoveredPrinter.ToString()}");
                Device.BeginInvokeOnMainThread(() => {
                    selectPrinterPage.printerList.BatchBegin();

                    if (!selectPrinterPage.printers.Contains(discoveredPrinter))
                    {
                        selectPrinterPage.printers.Add(discoveredPrinter);
                    }
                    selectPrinterPage.printerList.BatchCommit();
                });
            }
        }

        void Handle_ItemSelected(object sender, Xamarin.Forms.SelectedItemChangedEventArgs e)
        {
            DependencyService.Get<IPrinterDiscovery>().CancelDiscovery();
            if (e.SelectedItem is IDiscoveredPrinterUsb)
            {
                if (!((IDiscoveredPrinterUsb)e.SelectedItem).HasPermissionToCommunicate)
                {
                    DependencyService.Get<IPrinterDiscovery>().RequestUsbPermission(((IDiscoveredPrinterUsb)e.SelectedItem));
                }
            }

            OnPrinterSelected?.Invoke((IDiscoveredPrinter)e.SelectedItem);

            App.myPrinter = (IDiscoveredPrinter)e.SelectedItem;


            Navigation.PopAsync();
        }

        void Handle_Clicked(object sender, System.EventArgs e)
        {
            IConnection connection = null;
            try
            {
                connection = App.myPrinter.Connection;
                connection.Open();
                if (!CheckPrinterLanguage(connection))
                {
                    //ResetPage();
                    return;
                }
                IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(connection);
                IPrinterStatus status = printer.CurrentStatus;
                ShowStatus(status);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception:" + ex.Message);
                ShowErrorAlert(ex.Message);
            }
            finally
            {
                if ((connection != null) && (connection.IsConnected))
                    connection.Close();

            }


        }

        private void ShowStatus(IPrinterStatus status)
        {
            Device.BeginInvokeOnMainThread(() => {
                if (status.IsReadyToPrint)
                {

                   // checkStatus.Text = "Printer Status: Printer Ready";
                   //checkStatus.TextColor = Color.Green;
                   
                }
                else
                {
                    //checkStatus.Text = "Printer Status: Printer Error";
                   // checkStatus.TextColor = Color.Red;
                   
                }
            });
        }

        protected bool CheckPrinterLanguage(IConnection connection)
        {
            if (!connection.IsConnected)
                connection.Open();

            //  Check the current printer language
            byte[] response = connection.SendAndWaitForResponse(GetBytes("! U1 getvar \"device.languages\"\r\n"), 500, 100);
            string language = Encoding.UTF8.GetString(response, 0, response.Length);
            if (language.Contains("line_print"))
            {
               // ShowAlert("Switching printer to ZPL Control Language.", "Notification");
            }
            // printer is already in zpl mode
            else if (language.Contains("zpl"))
            {
                return true;
            }

            //  Set the printer command languege
            connection.Write(GetBytes("! U1 setvar \"device.languages\" \"zpl\"\r\n"));
            response = connection.SendAndWaitForResponse(GetBytes("! U1 getvar \"device.languages\"\r\n"), 500, 100);
            language = Encoding.UTF8.GetString(response, 0, response.Length);
            if (!language.Contains("zpl"))
            {
                ShowErrorAlert("Printer language not set. Not a ZPL printer.");
                return false;
            }
            return true;
        }

        protected static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            bytes = Encoding.UTF8.GetBytes(str);
            return bytes;
        }
    }
}
