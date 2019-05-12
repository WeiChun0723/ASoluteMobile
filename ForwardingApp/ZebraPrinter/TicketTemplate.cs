using System;
using System.Text;
using ASolute_Mobile.Models;
using LinkOS.Plugin;
using LinkOS.Plugin.Abstractions;

namespace ASolute_Mobile.ZebraPrinter
{
    public class TicketTemplate
    {
        public void PrintLineMode(SoldTicket ticketContent)
        {
            IConnection connection = null;
            try
            {
                connection = App.myPrinter.Connection;
                connection.Open();
                IZebraPrinter printer = ZebraPrinterFactory.Current.GetInstance(connection);
                /* if ((!CheckPrinterLanguage(connection)) || (!PreCheckPrinterStatus(printer)))
                 {
                     ResetPage();
                     return;
                 }*/

                SendZplReceipt(connection, ticketContent);

            }
            catch (Exception ex)
            {
                // Connection Exceptions and issues are caught here

            }
            finally
            {
                if ((connection != null) && (connection.IsConnected))
                    connection.Close();

            }
        }

        private void SendZplReceipt(IConnection printerConnection, SoldTicket ticketContent)
        {
            /*Some basics of ZPL. Find more information here : http://www.zebra.com

                    ^XA indicates the beginning of a label
                    ^PW sets the width of the label (in dots)
                    ^MNN sets the printer in continuous mode (variable length receipts only make sense with variably sized labels)
                    ^LL sets the length of the label (we calculate this value at the end of the routine)
                    ^LH sets the reference axis for printing. 
                       You will notice we change this positioning of the 'Y' axis (length) as we build up the label. Once the positioning is changed, all new fields drawn on the label are rendered as if '0' is the new home position
                    ^FO sets the origin of the field relative to Label Home ^LH
                    ^A sets font information 
                    ^FD is a field description
                    ^GB is graphic boxes (or lines)
                    ^B sets barcode information
                    ^XZ indicates the end of a label


                    "^XA" +
                    "^POI^PW400^MNN^LL325^LH0,0" + "\r\n" +
                    "^FO50,50" + "\r\n" + "^A0,N,70,70" + "\r\n" + "^FD Shipping^FS" + "\r\n" +
                    "^FO50,130" + "\r\n" + "^A0,N,35,35" + "\r\n" + "^FDPurchase Confirmation^FS" + "\r\n" +
                    "^FO50,180" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDCustomer:^FS" + "\r\n" +
                    "^FO225,180" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDAcme Industries^FS" + "\r\n" +
                    "^FO50,220" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDDelivery Date:^FS" + "\r\n" +
                    "^FO225,220" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FD{0}^FS" + "\r\n" +
                    "^FO50,273" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FDItem^FS" + "\r\n" +
                    "^FO280,273" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FDPrice^FS" + "\r\n" +
                    "^FO50,300" + "\r\n" + "^GB350,5,5,B,0^FS" + "^XZ";

 int headerHeight = 325;

            string sdf = "yyyy/MM/dd";
            string dateString = new DateTime().ToString(sdf);
            string header = string.Format(tmpHeader, dateString);

            printerConnection.Write(GetBytes(header));

              int heightOfOneLine = 40;
            double totalPrice = 0;

            Dictionary<string, string> itemsToPrint = CreateListOfItems();
            foreach (string productName in itemsToPrint.Keys) {
                itemsToPrint.TryGetValue(productName, out string price);

                string lineItem = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,28,28" + "\r\n" + "^FD{0}^FS" + "\r\n" + "^FO280,10" + "\r\n" + "^A0,N,28,28" + "\r\n" + "^FD${1}^FS" + "^XZ";
                double.TryParse(price, out double tempPrice);
                totalPrice += tempPrice;
                string oneLineLabel = string.Format(lineItem, productName, price);

                printerConnection.Write(GetBytes(oneLineLabel));
            }

            long totalBodyHeight = (itemsToPrint.Count + 1) * heightOfOneLine;
            long footerStartPosition = headerHeight + totalBodyHeight;
            string tPrice = Convert.ToString(Math.Round((totalPrice), 2));

            string footer = string.Format("^XA^POI^LL600" + "\r\n" + 
                "^FO50,1" + "\r\n" + "^GB350,5,5,B,0^FS" + "\r\n" +
                "^FO50,15" + "\r\n" + "^A0,N,40,40" + "\r\n" + "^FDTotal^FS" + "\r\n" +
                "^FO175,15" + "\r\n" + "^A0,N,40,40" + "\r\n" + "^FD${0}^FS" + "\r\n" +
                "^FO50,130" + "\r\n" + "^A0,N,45,45" + "\r\n" + "^FDPlease Sign Below^FS" + "\r\n" +
                "^FO50,190" + "\r\n" + "^GB350,200,2,B^FS" + "\r\n" +
                "^FO50,400" + "\r\n" + "^GB350,5,5,B,0^FS" + "\r\n" +
                "^FO50,420" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FDThanks for choosing us!^FS" + "\r\n" +
                "^FO50,470" + "\r\n" + "^B3N,N,45,Y,N" + "\r\n" + "^FD0123456^FS" + "\r\n" + "^XZ", tPrice);

            printerConnection.Write(GetBytes(footer));
            */

            string ticketType = "";

            switch(ticketContent.TicketType)
            {
                case "S":
                    ticketType = "Student";
                    break;

                case "P":
                    ticketType = "Public";
                    break;
            }

            string tmpHeader = "^XA" +
                    "^POI^PW400^MNN^LL325^LH0,0" + "\r\n" +
                    "^FO50,50" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FD PERAK TRANSIT BERHAD^FS" + "\r\n" +
                    "^FO50,100" + "\r\n" + "^A0,N,30,30" + "\r\n" + "^FD" + ticketContent.TrxTime.ToString("dd MMMM yyyy dddd HH:mm:ss") + "^FS" + "\r\n" +
                    "^FO50,140" + "\r\n" + "^A0,N,35,35" + "\r\n" + "^FD" + "RM" + ticketContent.Amount + "  " + ticketType + "^FS" + "\r\n" +
                    "^FO50,200" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FD" + ticketContent.From + " ->" + "^FS" + "\r\n" +
                    "^FO50,240" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FD" + ticketContent.To + "^FS" + "\r\n" +
                    "^FO50,280" + "\r\n" + "^A0,N,25,25" + "\r\n" + "^FD" + ticketContent.SerialNumber + "^FS" + "\r\n" +
                    "^XZ";

                    
            string sdf = "yyyy/MM/dd";
            string dateString = new DateTime().ToString(sdf);
            string header = string.Format(tmpHeader, dateString);

            printerConnection.Write(GetBytes(header));

            string info1 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDNO BAS: ^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + Ultis.Settings.SessionUserItem.TruckId + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info1));

            string info2 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDNO Pemandu: ^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + Ultis.Settings.SessionUserItem.DriverId + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info2));

            string info3 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDPemandu:^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + Ultis.Settings.SessionUserItem.UserName + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info3));

            string info4 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDKOD LALUAN: ^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + "T30B" + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info4));

            string info5 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDPEMBAYARAN: ^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + "Cash" + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info5));

            string info6 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FDPerak Transit Info: ^FS" + "\r\n" + "^FO220,10" + "\r\n" + "^A0,N,22,22" + "\r\n" + "^FD" + "+6012-4500806" + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info6));

            string info7 = "^XA^POI^LL40" + "^FO50,10" + "\r\n" + "^A0,N,20,20" + "\r\n" + "^FDSIMPAN TICKET UNTUK PEMERIKSAAN: ^FS" + "\r\n" + "^FO280,10" + "\r\n" + "^A0,N,28,28" + "\r\n" + "^FD" + "^FS" + "^XZ";
            printerConnection.Write(GetBytes(info7));
        }

        protected static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length];
            bytes = Encoding.UTF8.GetBytes(str);
            return bytes;
        }
    }
}
