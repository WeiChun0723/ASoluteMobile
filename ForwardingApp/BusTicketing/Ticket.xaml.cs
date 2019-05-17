using System;
using System.Collections.Generic;
using System.Text;
using ASolute.Mobile.Models;
using ASolute_Mobile.Models;
using ASolute_Mobile.WoosimPrinterService;
using ASolute_Mobile.WoosimPrinterService.library.Cmds;
using ASolute_Mobile.ZebraPrinter;
using Syncfusion.XForms.Buttons;
using Xamarin.Forms;

namespace ASolute_Mobile.BusTicketing
{
    public partial class Ticket : ContentPage
    {
        bool connectedPrinter = false;
        ListItems route = new ListItems();
        Double ticketRate;

        public Ticket(ListItems startStop, string selectStop, double rate)
        {
            InitializeComponent();

            fromEntry.Text = startStop.StopName;
            toEntry.Text = selectStop;
            lblPrice.Text = "RM " + String.Format("{0:0.00}", rate);

            Title = "Bus Ticket";

            route = startStop;
            ticketRate = rate;
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

        async void PrintBusTicket(object sender, System.EventArgs e)
        {
            print.IsVisible = true;

            string ticketType = "";

            SoldTicket ticket = new SoldTicket();

            var ticketButton = sender as SfButton;

            switch(ticketButton.StyleId)
            {
                case "btnPublic":
                    ticket.TicketType = TicketTypeConst.Public;
                    ticketType = btnPublic.Text;
                    break;

                case "btnStudent":
                    ticket.TicketType = TicketTypeConst.Student;
                    ticketType = btnStudent.Text;
                    break;
            }


            if(Ultis.Settings.StartEndStatus == "End")
            {
                ticket.TrxTime = DateTime.Now;
                ticket.TruckId = Ultis.Settings.SessionUserItem.TruckId;
                ticket.DriverId = Ultis.Settings.SessionUserItem.DriverId;
                ticket.TripId = Ultis.Settings.TripRecordID;
                ticket.RouteId = "T30B";
                ticket.StopId = route.StopId;
                ticket.PaymentType = PaymentType.Cash;
                ticket.Amount = ticketRate;
                ticket.Uploaded = false;
                ticket.SerialNumber = "T30B-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                ticket.From = fromEntry.Text;
                ticket.To = toEntry.Text;
                App.Database.SaveTicketTransaction(ticket);

                try
                {
                    /* TicketTemplate test = new TicketTemplate();

                    if(App.myPrinter!= null)
                    {
                        test.PrintLineMode(ticket);
                    }
                    else
                    {
                        await Navigation.PushAsync(new ZebraPrinterList());
                    }*/
                    if (connectedPrinter == false)
                     {

                         bool x = await DependencyService.Get<IBthService>().connectBTDevice("00:15:0E:E6:25:23");
                         //bool x = await DependencyService.Get<IBthService>().connectBTDevice("84:25:3F:1F:82:75");

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

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(DateTime.UtcNow.ToString("dd/MM/yyyy h:mm:ss tt") + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(lblPrice.Text + "   " + ticketType + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(fromEntry.Text + " ---> " + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes(toEntry.Text + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH02, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT02, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("T30B-" + DateTime.Now.ToString("yyyyMMddHHmmssfff") + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("NO BAS : " + Ultis.Settings.SessionUserItem.TruckId + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("NO Pemandu : " + Ultis.Settings.SessionUserItem.DriverId + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("Pemandu : " + Ultis.Settings.SessionUserItem.UserName + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("KOD LALUAN : T30B" + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("Pembayaran : Cash" + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("Perak Transit Info : +6012-4500806" + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimCmd.setTextStyle(0, true, (int)WoosimCmd.TEXTWIDTH.TEXTWIDTH01, (int)WoosimCmd.TEXTHEIGHT.TEXTHEIGHT01, false));
                     WriteMemoryStream(buffer, Encoding.ASCII.GetBytes("SIMPAN TICKET UNTUK PEMERIKSAAN." + "\r\n\r\n"));

                     WriteMemoryStream(buffer, WoosimPageMode.print());

                     DependencyService.Get<IBthService>().WriteComm(buffer.ToArray());
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Please start trip before generate ticket", "OK");
            }

            print.IsVisible = false;

        }

        async private void WriteMemoryStream(System.IO.MemoryStream stream, byte[] data)
        {
            await stream.WriteAsync(data, 0, data.Length);
        }
    }
}
