using System;
using System.Text;
using Xamarin.Forms;

namespace ASolute_Mobile.WoosimPrinterService.library
{
    namespace Cmds
    {
        public static class WoosimCmd
        {
            public enum TEXTWIDTH : int
            {
                TEXTWIDTH01, TEXTWIDTH02,
                TEXTWIDTH03, TEXTWIDTH04,
                TEXTWIDTH05, TEXTWIDTH06,
                TEXTWIDTH07, TEXTWIDTH08
            }
            public enum TEXTHEIGHT : int
            {
                TEXTHEIGHT01, TEXTHEIGHT02,
                TEXTHEIGHT03, TEXTHEIGHT04,
                TEXTHEIGHT05, TEXTHEIGHT06,
                TEXTHEIGHT07, TEXTHEIGHT08
            }
            public enum MCU : int { M16C, ARM, RX }

            public static byte linefeed = 0x0A;

            public static byte[] initPrinter() { return new byte[] { 27, 64 }; }
            public static byte[] clearPrinterBuffer() { return new byte[] { 24 }; }
            public static byte[] printData() { return new byte[] { 10 }; }
            public static byte[] lineFeed(int lines)
            {
                if (0 <= lines && lines <= 255)
                {
                    byte[] LineFeedCmds = { 0x1B, 0x64, 0x00 };
                    LineFeedCmds[2] = (byte)lines;

                    return LineFeedCmds;
                }
                else
                    return new byte[] { 0x00 };
            }
            public static byte[] dotsFeed(int dots)
            {
                if (0 <= dots && dots <= 255)
                {
                    byte[] DotsFeedCmds = { 0x1B, 0x4A, 0x00 };
                    DotsFeedCmds[2] = (byte)dots;

                    return DotsFeedCmds;
                }
                else
                    return new byte[] { 0x00 };
            }
            public static byte[] setFontSize(int n)
            {
                if (0 <= n && n <= 2)
                {
                    byte[] Cmds = { 0x1B, 0x21, 0x00 };
                    Cmds[2] = (byte)n;

                    return Cmds;
                }
                else
                    return new byte[] { 0x00 };
            }
            public static byte[] setTextStyle(int underline, bool emphasize, int width, int height, bool reverse)
            {

                byte[] Cmds = {0x1B, 0x2D, 0x00,
                            0x1B, 0x45, 0x00,
                            0x1D, 0x21, 0x00,
                            0x1D, 0x42, 0x00};

                byte m_width = 0x00, m_height = 0x00, m_size = 0x00;

                //underline support No line, 1dot line and 2dot line 
                if (underline == 1)
                    Cmds[2] = 0x01;
                else if (underline == 2)
                    Cmds[2] = 0x02;
                else
                    Cmds[2] = 0x00;

                //Emphasize 
                if (!emphasize)
                    Cmds[5] = 0x00;
                else
                    Cmds[5] = 0x01;

                /* width & height */
                //See Woosim Commamnd Manual p.19
                // 0: origianl size
                // 1: double width or height
                // 2: triple width or height
                // ...
                // 7: octuple width or height.

                if (0 <= width && width <= 7)
                    m_width = (byte)width;
                else
                    m_width = 0x00;

                if (0 <= height && height <= 7)
                    m_height = (byte)height;
                else
                    m_height = 0x00;

                m_size = (byte)((m_height << 4) | m_width);
                Cmds[8] = m_size;

                if (!reverse)
                    Cmds[11] = 0x00;
                else
                    Cmds[11] = 0x01;

                return Cmds;
            }

            public static byte[] selectTTF(string ttfName)
            {
                byte[] pt1 = { 0x1B, 0x67, 0x46 };

                byte[] s1 = System.Text.Encoding.UTF8.GetBytes(ttfName);

                byte[] fi = { 0x00 };
                byte[] Cmds = new byte[pt1.Length + ttfName.Length + fi.Length];


                Buffer.BlockCopy(pt1, 0, Cmds, 0, pt1.Length);
                Buffer.BlockCopy(s1, 0, Cmds, pt1.Length, ttfName.Length);
                Buffer.BlockCopy(fi, 0, Cmds, pt1.Length + ttfName.Length, fi.Length);

                return Cmds;
            }

            public static byte[] printTextUsingTTF(string text, int iXSize, int iYSize)
            {
                byte[] sendTTFDataCmd = { 0x1B, 0x67, 0x55, 0x00, 0x00 };
                sendTTFDataCmd[3] = System.Convert.ToByte(iXSize);
                sendTTFDataCmd[4] = System.Convert.ToByte(iYSize);

                string printText = text;
                byte[] printTextDataHex = Encoding.BigEndianUnicode.GetBytes(printText);

                byte[] fi = { 0x00, 0x00 };

                byte[] Cmds = new byte[sendTTFDataCmd.Length + printTextDataHex.Length + fi.Length];

                Buffer.BlockCopy(sendTTFDataCmd, 0, Cmds, 0, sendTTFDataCmd.Length);
                Buffer.BlockCopy(printTextDataHex, 0, Cmds, sendTTFDataCmd.Length, printTextDataHex.Length);
                Buffer.BlockCopy(fi, 0, Cmds, sendTTFDataCmd.Length + printTextDataHex.Length, fi.Length);

                return Cmds;
            }

            public static byte[] getStatus() { return new byte[] { 16, 4, 4 }; }
            public static byte[] getModelInfo() { return new byte[] { 16, 89, 255 }; }


        }

        public static class WoosimPageMode
        {
            public enum WIDTH : int { _1INCH = 192, _2INCH = 384, _3INCH = 576, _4INCH = 832 }

            public static byte[] setPageMode() { return new byte[] { 27, 76 }; }
            public static byte[] setStandardMode() { return new byte[] { 27, 83 }; }
            public static byte[] initPageMode(int iXPos, int iYPos, int width, int height)
            {
                byte[] a1 = WoosimCmd.initPrinter();
                byte[] a2 = setPageMode();
                byte[] a3 = setArea(iXPos, iYPos, width, height);
                byte[] a4 = WoosimCmd.clearPrinterBuffer();

                byte[] Cmds = new byte[a1.Length + a2.Length + a3.Length + a4.Length];
                int cnt = 0;

                Buffer.BlockCopy(a1, 0, Cmds, cnt, a1.Length); cnt += a1.Length;
                Buffer.BlockCopy(a2, 0, Cmds, cnt, a2.Length); cnt += a2.Length;
                Buffer.BlockCopy(a3, 0, Cmds, cnt, a3.Length); cnt += a3.Length;
                Buffer.BlockCopy(a4, 0, Cmds, cnt, a4.Length);

                return Cmds;
            }

            public static byte[] setArea(int iXPos, int iYPos, int width, int height)
            {
                byte[] Cmds = { 0x1B, 0x57, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(iXPos % 256); Cmds[3] = (byte)(iXPos / 256);
                Cmds[4] = (byte)(iYPos % 256); Cmds[5] = (byte)(iYPos / 256);

                Cmds[6] = (byte)(width % 256); Cmds[7] = (byte)(width / 256);
                Cmds[8] = (byte)(height % 256); Cmds[9] = (byte)(height / 256);

                return Cmds;
            }

            public static byte[] setPosition(int iXPos, int iYPos)
            {
                byte[] Cmds = { 0x1B, 0x4F, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(iXPos % 256); Cmds[3] = (byte)(iXPos / 256);
                Cmds[4] = (byte)(iYPos % 256); Cmds[5] = (byte)(iYPos / 256);

                return Cmds;
            }
            public static byte[] setPosition(Point A)
            {
                byte[] Cmds = { 0x1B, 0x4F, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(A.X % 256); Cmds[3] = (byte)(A.X / 256);
                Cmds[4] = (byte)(A.Y % 256); Cmds[5] = (byte)(A.Y / 256);

                return Cmds;
            }

            public static byte[] drawBox(int width, int height, int thickness)
            {
                byte[] Cmds = { 0x1D, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(width % 256); Cmds[3] = (byte)(width / 256);
                Cmds[4] = (byte)(height % 256); Cmds[5] = (byte)(height / 256);

                if (0 <= thickness && thickness <= 255)
                    Cmds[6] = (byte)thickness;
                else
                    Cmds[6] = (byte)0;

                return Cmds;
            }
            public static byte[] drawBox(int iXPos, int iYPos, int width, int height, int thickness)
            {
                byte[] position = setPosition(iXPos, iYPos);
                byte[] Cmds = { 0x1D, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(width % 256); Cmds[3] = (byte)(width / 256);
                Cmds[4] = (byte)(height % 256); Cmds[5] = (byte)(height / 256);

                if (0 <= thickness && thickness <= 255)
                    Cmds[6] = (byte)thickness;
                else
                    Cmds[6] = (byte)0;

                byte[] total = new byte[Cmds.Length + position.Length];
                Buffer.BlockCopy(position, 0, total, 0, position.Length);
                Buffer.BlockCopy(Cmds, 0, total, position.Length, Cmds.Length);

                return total;
            }
            public static byte[] drawBox(Point A, int width, int height, int thickness)
            {
                byte[] position = setPosition(A);
                byte[] Cmds = { 0x1D, 0x69, 0x00, 0x00, 0x00, 0x00, 0x00 };

                Cmds[2] = (byte)(width % 256); Cmds[3] = (byte)(width / 256);
                Cmds[4] = (byte)(height % 256); Cmds[5] = (byte)(height / 256);

                if (0 <= thickness && thickness <= 255)
                    Cmds[6] = (byte)thickness;
                else
                    Cmds[6] = (byte)0;

                byte[] total = new byte[Cmds.Length + position.Length];
                Buffer.BlockCopy(position, 0, total, 0, position.Length);
                Buffer.BlockCopy(Cmds, 0, total, position.Length, Cmds.Length);

                return total;
            }
            public static byte[] drawLine(int iX1Pos, int iY1Pos, int iX2Pos, int iY2Pos, int thickness)
            {
                byte[] CmdHeader = { 27, 103, 49 };
                byte[] LineInfo = new byte[9];

                LineInfo[0] = (byte)(iX1Pos % 256); LineInfo[1] = (byte)(iX1Pos / 256);
                LineInfo[2] = (byte)(iY1Pos % 256); LineInfo[3] = (byte)(iY1Pos / 256);
                LineInfo[4] = (byte)(iX2Pos % 256); LineInfo[5] = (byte)(iX2Pos / 256);
                LineInfo[6] = (byte)(iY2Pos % 256); LineInfo[7] = (byte)(iY2Pos / 256);

                if (thickness >= 0 && thickness <= 256)
                    LineInfo[8] = (byte)thickness;
                else
                    LineInfo[8] = (byte)1;

                byte[] Cmds = new byte[12];
                Buffer.BlockCopy(CmdHeader, 0, Cmds, 0, CmdHeader.Length);
                Buffer.BlockCopy(LineInfo, 0, Cmds, CmdHeader.Length, LineInfo.Length);

                return Cmds;
            }
            public static byte[] drawLine(Point A1, Point B1, int thickness)
            {
                byte[] CmdHeader = { 27, 103, 49 };
                byte[] LineInfo = new byte[9];

                LineInfo[0] = (byte)(A1.X % 256); LineInfo[1] = (byte)(A1.X / 256);
                LineInfo[2] = (byte)(A1.Y % 256); LineInfo[3] = (byte)(A1.Y / 256);
                LineInfo[4] = (byte)(B1.X % 256); LineInfo[5] = (byte)(B1.X / 256);
                LineInfo[6] = (byte)(B1.Y % 256); LineInfo[7] = (byte)(B1.Y / 256);

                if (thickness >= 0 && thickness <= 256)
                    LineInfo[8] = (byte)thickness;
                else
                    LineInfo[8] = (byte)1;

                byte[] Cmds = new byte[12];
                Buffer.BlockCopy(CmdHeader, 0, Cmds, 0, CmdHeader.Length);
                Buffer.BlockCopy(LineInfo, 0, Cmds, CmdHeader.Length, LineInfo.Length);

                return Cmds;
            }


            public static byte[] print(bool stay)
            {
                if (stay)
                    return new byte[] { 27, 12 };
                else
                    return new byte[] { 12 };
            }
            public static byte[] print()
            {
                return new byte[] { 12 };
            }

        }

        public static class WoosimBarcode
        {
            public enum QRCODE_CAPACITY : byte { L = 76, M = 77, Q = 81, H = 72 }
            public enum GS1Type : byte { Omnidirectional, Truncated, Stacked, StackedOmnnidirectional, Limited, Expanded, ExpandedStacked }
            public enum _1DBarcodeType : int { UPCA = 65, UPCE, EAN13, EAN8, CODE39, ITF, CODABAR, CODE93, CODE128 };
            public enum _1DBarcodeWidth : byte { Width1 = 1, Width2, Width3, Width4, Width5, Width6, Width7, Width8 }
            public static int _1DBarcodeDefaultHeight = 60;

            public static byte[] create1DBarcode(int iBarcodeType, byte ucBarWidth, int iBarHeight, bool bHRI, string barcodeData)
            {
                byte[] a1 = { 0x1d, (byte)'w', 0x00 };
                byte[] a2 = { 0x1d, (byte)'h', 0x00 };
                byte[] a3 = { 0x1d, (byte)'H', 0x00 };
                byte[] a4 = { 0x1d, (byte)'k', 0x00, 0x00 };

                //바코드 폭,높이 설정 설정 
                if (ucBarWidth >= 1 && ucBarWidth <= 8)
                    a1[2] = (byte)ucBarWidth;
                else
                    a1[2] = 1;

                if (iBarHeight >= 1 && iBarHeight <= 255)
                    a2[2] = (byte)iBarHeight;
                else
                    a2[2] = 1;

                if (bHRI ? true : false)
                    a3[2] = 1;
                else
                    a3[2] = 0;

                //바코드 

                byte[] s1 = System.Text.Encoding.UTF8.GetBytes(barcodeData);

                a4[2] = (byte)iBarcodeType;
                a4[3] = (byte)barcodeData.Length;

                byte[] Cmds = new byte[a1.Length + a2.Length + a3.Length + a4.Length + s1.Length];
                int cnt = 0;

                Buffer.BlockCopy(a1, 0, Cmds, cnt, a1.Length); cnt += a1.Length;
                Buffer.BlockCopy(a2, 0, Cmds, cnt, a2.Length); cnt += a2.Length;
                Buffer.BlockCopy(a3, 0, Cmds, cnt, a3.Length); cnt += a3.Length;
                Buffer.BlockCopy(a4, 0, Cmds, cnt, a4.Length); cnt += a4.Length;
                Buffer.BlockCopy(s1, 0, Cmds, cnt, s1.Length);

                return Cmds;
            }
            public static byte[] createGS1Databar(byte ucGS1Type, int row, string gs1Data)
            {
                byte[] cf1 = { 29, 49, 0, 0, 0 };
                cf1[2] = ucGS1Type;
                cf1[3] = (byte)row;

                byte[] s1 = System.Text.Encoding.UTF8.GetBytes(gs1Data);

                byte[] Cmds = new byte[cf1.Length + gs1Data.Length];
                Buffer.BlockCopy(cf1, 0, Cmds, 0, cf1.Length - 1);
                Buffer.BlockCopy(s1, 0, Cmds, cf1.Length - 1, gs1Data.Length);
                Buffer.BlockCopy(cf1, 4, Cmds, cf1.Length + gs1Data.Length - 1, 1);

                return Cmds;
            }

            public static byte[] createQRCode(int version, byte level, int module, byte[] barcodeData)
            {
                byte[] cf1 = { 0x1D, 0x5A, 0x02, 0x1B, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00 };
                byte[] Cmds = new byte[cf1.Length + barcodeData.Length];

                byte lower = (byte)(barcodeData.Length % 256);
                byte upper = (byte)(barcodeData.Length / 256);

                if (1 <= version && version <= 40)
                    cf1[5] = (byte)version;
                else
                    cf1[5] = 0x00;

                cf1[6] = level;

                switch (module)
                {
                    case 0: cf1[7] = 0; break;
                    case 1: cf1[7] = 1; break;
                    case 2: cf1[7] = 2; break;
                    case 3: cf1[7] = 3; break;
                    case 4: cf1[7] = 4; break;
                    case 5: cf1[7] = 5; break;
                    case 6: cf1[7] = 6; break;
                    case 7: cf1[7] = 7; break;
                    default: cf1[7] = 0; break;
                }

                cf1[8] = lower;
                cf1[9] = upper;

                Buffer.BlockCopy(cf1, 0, Cmds, 0, cf1.Length);
                Buffer.BlockCopy(barcodeData, 0, Cmds, cf1.Length, barcodeData.Length);

                return Cmds;
            }
            public static byte[] createQRCode(int version, byte level, int module, int barcodeDataLength)
            {
                byte[] cf1 = { 0x1D, 0x5A, 0x02, 0x1B, 0x5A, 0x00, 0x00, 0x00, 0x00, 0x00 };

                byte lower = (byte)(barcodeDataLength % 256);
                byte upper = (byte)(barcodeDataLength / 256);

                if (1 <= version && version <= 40)
                    cf1[5] = (byte)version;
                else
                    cf1[5] = 0x00;

                cf1[6] = level;

                switch (module)
                {
                    case 0: cf1[7] = 0; break;
                    case 1: cf1[7] = 1; break;
                    case 2: cf1[7] = 2; break;
                    case 3: cf1[7] = 3; break;
                    case 4: cf1[7] = 4; break;
                    case 5: cf1[7] = 5; break;
                    case 6: cf1[7] = 6; break;
                    case 7: cf1[7] = 7; break;
                    default: cf1[7] = 0; break;
                }

                cf1[8] = lower;
                cf1[9] = upper;

                return cf1;
            }
        }
    }
}
