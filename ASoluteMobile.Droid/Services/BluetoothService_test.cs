using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.Bluetooth;
using ASolute_Mobile.WoosimPrinterService;
using Java.Util;

[assembly: Xamarin.Forms.Dependency(typeof(Haulage.Droid.BluetoothService_test))]
namespace Haulage.Droid
{
    public class BluetoothService_test : IBthService
    {
        private static readonly string TAG = "BluetoothService_test";

        public event EventHandler<BTMessage> RecvMessage;

        private System.IO.Stream outStream = null;
        private System.IO.Stream inStream = null;
        private BluetoothSocket btSocket = null;
        private bool isPageMode = false;
        private static bool connTF = false;

        private CustomQueue que;
        private System.Threading.Thread thread2 = null;

        private BluetoothDevice btDevice = null;
        private Task ThreadPhaser;

        private static CancellationTokenSource cts;

        public BluetoothService_test()
        {
        }
        async public Task<bool> connectBTDevice(string bda)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
                throw new System.Exception("No Bluetooth adapter found.");

            if (!adapter.IsEnabled)
                throw new System.Exception("Please, turn on the bluetooth adapter.");

            UUID BTSPPUUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            var a = adapter.BondedDevices.Where(p => p.Address == bda.ToUpper()).ToList();
            btDevice = a.FirstOrDefault();

            btSocket = btDevice.CreateInsecureRfcommSocketToServiceRecord(BTSPPUUID);
            btSocket.Connect();

            if (btSocket.IsConnected == true)
            {
                outStream = btSocket.OutputStream;
                inStream = btSocket.InputStream;

                connTF = true;
                cts = new CancellationTokenSource();
                System.Threading.Thread thread = new System.Threading.Thread(() => ReadThread(this, inStream));
                thread.Start();


            }
            return true;
        }

        async protected static void ReadThread(BluetoothService_test a, System.IO.Stream stm)
        {
            while (!cts.IsCancellationRequested)
            {
                try
                {
                    byte[] at = new byte[1024];
                    Task<int> readTask = stm.ReadAsync(at, 0, 1024);
                    using (cts.Token.Register(() => stm.Close()))
                    {
                        readTask.Wait(cts.Token);
                        int iRead = await readTask;

                        BTMessage msg = new BTMessage();
                        msg.type = 0;
                        msg.length = iRead;
                        msg.data = at;

                        a.RecvMessage(a, msg);
                    }

                }
                catch (System.OperationCanceledException)
                {
                    //Handle the cancelled task.
                    string text = int.MaxValue.ToString();
                }
                catch (System.Exception e)
                {
                    string text = e.ToString();
                }
            }
        }

        public bool disconnBTDevice()
        {
            cts.Cancel();

            try
            {
                inStream.Close(); inStream.Dispose();
                outStream.Close(); outStream.Dispose();
                btSocket.Close(); btSocket.Dispose();
            }
            catch (Java.Lang.Exception e)
            {
                string text = e.ToString();

            }
            catch (System.Exception e)
            {
                string text = e.ToString();

            }
            return true;
        }

        public void WriteComm(byte data)
        {
            Task.Run(() =>
            {
                outStream.WriteByte(data);
            });
        }
        async public void WriteComm(byte[] data)
        {
            await outStream.WriteAsync(data, 0, data.Length);
        }

        public void processData(byte[] data, int dataLength)
        {
            if (que == null)
                que = new CustomQueue(50);

            que.EnQueue(data, dataLength);

            //처리를 위한 thread 생성
            //이미 생성되어 있으면 패스
            if (ThreadPhaser == null)
            {
                ThreadPhaser = new Task(() =>
                   PhaserThread(this)
                );
                ThreadPhaser.Start();
            }
        }

        async private void PhaserThread(BluetoothService_test service)
        {
            while (!service.que.isEmpty())
            {
                byte getMethod = service.que.DeQueue();
                await Task.Delay(100);
                switch (getMethod)
                {
                    case 0x00:
                        break;

                    case 0x02:
                        {
                            byte caseSwitch2 = service.que.DeQueue();

                            //send MSR data
                            if (caseSwitch2 >= 0x43 && caseSwitch2 <= 0x47)
                                service.phasingMSRdata(caseSwitch2);
                            else
                            {
                                byte[] tempData = new byte[2];
                                tempData[0] = getMethod;
                                tempData[1] = caseSwitch2;

                                service.passingUnKownData(tempData);
                            }
                        }
                        break;
                    case 0x1B:
                        {
                            byte[] tempData = new byte[1];
                            tempData[0] = getMethod;
                            service.passingUnKownData(tempData);
                        }
                        break;

                    default:
                        {
                            //일반 응답 데이터 전달
                            byte[] testa = new byte[1];
                            testa[0] = getMethod;
                            service.passingUnKownData(testa);
                        }
                        break;
                }
            }

            service.ThreadPhaser = null;
        }

        private void phasingMSRdata(byte valData)
        {
            byte val1 = 0x02;
            byte val2 = valData;

            /*
             * valData 별 MSR 타입과 데이터 길이
             * 1. 0x43
             *   a. 12T  = prefix(2) + other_prefix(3) + data(76) + postfix(4) = 2 + 83
             *   b. 23T  = prefix(2) + other_prefix(3) + data(37) + postfix(4) = 2 + 44
             *   c. 123T = prefix(2) + other_prefix(3) + data(76) + postfix(4) = 2 + 83
             *   
             * 2. 0x44
             *   a. 12T  = prefix(2) + other_prefix(3) + data(37) + postfix(3)  = 2 + 43
             *   b. 23T  = prefix(2) + other_prefix(3) + data(104) + postfix(3) = 2 + 110
             *   c. 123T = prefix(2) + other_prefix(3) + data(37) + postfix(3)  = 2 + 43
             *  
             * 3. 0x45
             *   a. 12T  = prefix(2) + other_prefix(4) + data(76) + gap(1) + data(37) + postfix(4)  = 2 + 122
             *   b. 23T  = prefix(2) + other_prefix(4) + data(37) + gap(1) + data(104) + postfix(4) = 2 + 150
             *   c. 123T = prefix(2) + other_prefix(4) + data(76) + gap(1) + data(37) + postfix(4)  = 2 + 122
             *
             * 4. 0x46 (123T only)
             *   a. 123T = prefix(2) + other_prefix(4)
                            + data(76) + gap(1) + data(37) + gap(1) 
                            + data(104) + postfix(4)
                            = 2 + 227
             *   
             * 5. 0x47 (123T only)
             *   a. 123T = prefix(2) + other_prefix(3) + data(104) + postfix(3) = 2 + 110   
             */

            byte[] containedData = new byte[2 + que.getUseSpace()];
            containedData[0] = val1;
            containedData[1] = val2;
            int cnt = que.getUseSpace();
            byte[] test = que.DeQueue(cnt);
            Buffer.BlockCopy(test, 0, containedData, 2, test.Length);

            BTMessage data = new BTMessage();
            data.type = BTMessage.MSR_Response_Type;
            data.length = containedData.Length;
            data.data = containedData;

            RecvMessage(this, data);

        }

        //정상 파싱 처리하지 않는 데이터
        private void passingUnKownData(byte[] val1)
        {
            byte[] containedData = new byte[val1.Length + que.getUseSpace()];

            Buffer.BlockCopy(val1, 0, containedData, 0, val1.Length);

            int cnt = que.getUseSpace();
            byte[] test = que.DeQueue(cnt);
            Buffer.BlockCopy(test, 0, containedData, val1.Length, test.Length);

            BTMessage data = new BTMessage();
            data.type = BTMessage.UnKnown_Response_Type;
            data.length = containedData.Length;
            data.data = containedData;

            RecvMessage(this, data);
        }
    }
}
