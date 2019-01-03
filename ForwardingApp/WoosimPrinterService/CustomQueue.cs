using System;
namespace ASolute_Mobile.WoosimPrinterService
{
    public class CustomQueue
    {
        private static int iFront;
        private static int iRear;
        private static byte[] buffer;
        private static int bufferMax;
        public CustomQueue(int QueueSize)
        {
            bufferMax = QueueSize;
            buffer = new byte[bufferMax];
            iFront = 0;
            iRear = 0;
        }

        public void EnQueue(byte data)
        {
            if (isFull())
                return;

            buffer[iRear] = data;
            iRear = ++iRear % bufferMax;
        }

        public void EnQueue(byte[] data, int readDataLen)
        {
            if (getUseSpace() == bufferMax - 1)
                return;
            else if (readDataLen > bufferMax - iRear)
            {
                int readCnt = 0;
                readCnt = bufferMax - iRear;
                Buffer.BlockCopy(data, 0, buffer, iRear, readCnt);
                Buffer.BlockCopy(data, readCnt, buffer, 0, readDataLen - readCnt);
                iRear = (iRear + readDataLen) % bufferMax;
            }
            else
            {
                Buffer.BlockCopy(data, 0, buffer, iRear, readDataLen);
                iRear = (iRear + readDataLen) % bufferMax;
            }
        }


        public byte DeQueue()
        {
            byte returnVal = buffer[iFront];
            iFront = ++iFront % bufferMax;
            return returnVal;
        }
        public byte[] DeQueue(int n)
        {
            if (n > getUseSpace())
                return null;
            else if (n > bufferMax - iFront)
            {
                byte[] data = new byte[n];

                int readCnt = 0;
                readCnt = bufferMax - iFront;
                Buffer.BlockCopy(buffer, iFront, data, 0, readCnt);

                Buffer.BlockCopy(buffer, 0, data, readCnt, n - readCnt);

                iFront = (iFront + n) % bufferMax;

                return data;
            }
            else
            {
                byte[] data = new byte[n];

                Buffer.BlockCopy(buffer, iFront, data, 0, n);
                iFront = (iFront + n) % bufferMax;
                return data;
            }
        }

        public bool isEmpty()
        {
            return iFront == iRear ? true : false;
        }
        public bool isFull()
        {
            return getUseSpace() == (bufferMax - 1) ? true : false;
        }
        public int getUseSpace()
        {
            return (iRear - iFront + bufferMax) % bufferMax;
        }
        public void SkipQueue(int iSkipVal)
        {
            iFront += iSkipVal;
            iFront %= bufferMax;
        }
        public byte PeekQueue()
        {
            return buffer[iFront];
        }
    }
}
