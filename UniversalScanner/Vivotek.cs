﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace UniversalScanner
{
    class Vivotek : ScanEngine
    {
        protected const int port = 10000;
        protected const UInt32 magic = 0x4a5d8f1c;
        protected byte sessionCounter;

        public override int color
        {
            get
            {
                return Color.DarkCyan.ToArgb();
            }
        }
        public override string name
        {
            get
            {
                return "Vivotek";
            }
        }

        enum VivotekValue
        {
            invalid = 0x00,
            longName = 0x01,
            macAddress = 0x02,
            IPAddress = 0x03,
            _type04= 0x04,
            _type05= 0x05,
            _type06= 0x06,
            _type07= 0x07,
            _type08= 0x08,
            shortName = 0x09,
            _type0a= 0x0a
        }

        public Vivotek()
        {
            sessionCounter = 1;

            listenUdpInterfaces();
        }

        [StructLayout(LayoutKind.Explicit, Size = 5, CharSet = CharSet.Ansi)]
        public struct VivotekHeader
        {
            [FieldOffset(0)] public byte session;
            [FieldOffset(1)] public UInt32 magic;
        }

       public override void reciever(IPEndPoint from, byte[] data)
        {
            VivotekHeader header;
            int headerSize;
            IntPtr ptr;
            int position;

            string ip, model, mac; 

            headerSize = Marshal.SizeOf(typeof(VivotekHeader));

            ptr = Marshal.AllocHGlobal(headerSize);
            try
            {
                Marshal.Copy(data, 0, ptr, headerSize);
                header = (VivotekHeader)Marshal.PtrToStructure(ptr, typeof(VivotekHeader));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            if (ntohl(header.magic) != magic)
            {
                traceWriteLine(debugLevel.Warn, "Vivotek.reciever(): Error: Wrong packet magic value");
                return;
            }

            ip = "";
            mac = "";
            model = "";

            position = headerSize;
            while (position < data.Length)
            {
                byte variable;
                byte[] value;

                variable = readNextValue(data, ref position, out value);
                switch (variable)
                {
                    case (byte)VivotekValue.invalid:
                        traceWriteLine(debugLevel.Warn, "Vivotek.reciever(): Error: Invalid packet");
                        return;
                    case (byte)VivotekValue.IPAddress:
                        ip = String.Format("{0}.{1}.{2}.{3}", value[0], value[1], value[2], value[3]);
                        break;
                    case (byte)VivotekValue.longName:
                        traceWriteLine(debugLevel.Debug, "longName");
                        traceWriteData(debugLevel.Debug, value);
                        break;
                    case (byte)VivotekValue.macAddress:
                        mac = String.Format("{0:X2}:{1:X2}:{2:X2}:{3:X2}:{4:X2}:{5:X2}", value[0], value[1], value[2], value[3], value[4], value[5]);
                        break;
                    case (byte)VivotekValue.shortName:
                        model = Encoding.UTF8.GetString(value);
                        break;
                }
            }

            if (ip != "")
            {
                viewer.deviceFound(name, 1, ip, model, mac);
            }
        }

        private byte readNextValue(byte[] data, ref int position, out byte[] value)
        {
            byte vtype;
            byte size;

            if (position +2 >= data.Length)
            {
                value = null;
                return 00;
            }
            vtype = data[position];
            position++;
            size = data[position];
            position++;

            // type 0x04 is special, unique char
            // can be also when size is >= 20
            if (vtype == (byte)(VivotekValue._type04))
            {
                value = new byte[1] { (byte)size };
                return vtype;
            }

            if (position + size > data.Length)
            {
                value = null;
                return 00;
            }
            value = new byte[size];
            Array.Copy(data, position, value, 0, size);
            position += size;
            return vtype;
        }

        public override void scan()
        {
#if DEBUG
            selfTest();
#endif
            sendBroadcast(port);
        }

        public override byte[] sender(IPEndPoint dest)
        {
            VivotekHeader header;
            int headerSize;
            IntPtr ptr;
            byte[] result;

            header = new VivotekHeader() { session = sessionCounter++, magic = htonl(magic) };
            headerSize = Marshal.SizeOf(typeof(VivotekHeader));

            ptr = Marshal.AllocHGlobal(headerSize);
            result = new byte[headerSize];
            try
            {
                Marshal.StructureToPtr<VivotekHeader>(header, ptr, false);
                Marshal.Copy(ptr, result, 0, headerSize);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return result;
        }
    }
}
