using System;
using System.Threading.Tasks;

namespace ASolute_Mobile.WoosimPrinterService
{

        public class BTMessage
        {
            public static int Throughout_Type = 0;
            public static int MSR_Response_Type = 1;
            public static int Status_Response_Type = 2;
            public static int UnKnown_Response_Type = 3;

            public int type { get; set; }
            public int length { get; set; }
            public byte[] data { get; set; }
        }

        public interface IBthService
        {
            Task<bool> connectBTDevice(string bda);
            bool disconnBTDevice();

            void WriteComm(byte data);
            void WriteComm(byte[] data);

            void processData(byte[] data, int dataLength);


            event EventHandler<BTMessage> RecvMessage;
        }

}
