using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using ASolute_Mobile.WoosimPrinterService;
using ASolute_Mobile.WoosimPrinterService.library.Cmds;
using Xamarin.Forms;

namespace ASolute_Mobile.BusTicketing
{
    public partial class CheckOut : ContentPage
    {

        bool connectedPrinter = false;

        public CheckOut()
        {
            InitializeComponent();

            Title = "Check Out";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            MessagingCenter.Unsubscribe<App>((App)Application.Current, "Testing");

            if (connectedPrinter == true)
            {
                DependencyService.Get<IBthService>().disconnBTDevice();
            }

        }

        async void PrintTotal(object sender, System.EventArgs e)
        {
            print.IsVisible = true;

            try
            {
                if (connectedPrinter == false)
                {

                    bool x = await DependencyService.Get<IBthService>().connectBTDevice("00:15:0E:E6:25:23");


                    if (!x)
                    {
                        Device.BeginInvokeOnMainThread(async () =>
                        {
                            await DisplayAlert("Error", "Unable connect", "OK");
                        });
                    }
                    else
                    {
                        connectedPrinter = true;
                    }
                }

                System.IO.MemoryStream buffer = new System.IO.MemoryStream(512);

                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("PERAK TRANSIT BERHAD\r\n\r\n"));

                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("Date & Time:\r\n"));

                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString("dd/MM/yyyy h:mm:ss tt") + "\r\n\r\n"));


                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("Settlement Amount:\r\n"));

                WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("RM52.60" + "\r\n\r\n"));

                WriteMemoryStream(buffer, WoosimPageMode.print());

                DependencyService.Get<IBthService>().WriteComm(buffer.ToArray());

            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

            print.IsVisible = false;

        }

        async private void WriteMemoryStream(System.IO.MemoryStream stream, byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
