using System;
using System.IO;
using Microsoft.Win32;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Net;

// Derived from code kindly donated by Ivo Smits <ivo@ucis.nl>

namespace Underlink
{
    public class NetworkAdapterWin
    {
        public static IList<NetworkAdapterWin> GetAdapters()
        {
            const string AdapterKey = "SYSTEM\\CurrentControlSet\\Control\\Class\\{4D36E972-E325-11CE-BFC1-08002BE10318}";
            RegistryKey regAdapters = Registry.LocalMachine.OpenSubKey(AdapterKey, false);

            List<NetworkAdapterWin> adapters = new List<NetworkAdapterWin>();

            /*
            foreach (System.Net.NetworkInformation.NetworkInterface ni in
                     System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                Console.WriteLine("Description={0}", ni.Description);
                Console.WriteLine("PhysicalAddress={0}", ni.GetPhysicalAddress());
                Console.WriteLine("ID={0}", ni.Id);
                Console.WriteLine("IsReceiveOnly={0}", ni.IsReceiveOnly);
                Console.WriteLine("Name={0}", ni.Name);
                Console.WriteLine("NICType={0}", ni.NetworkInterfaceType);
                Console.WriteLine("OperationalStatus={0}", ni.OperationalStatus);
                Console.WriteLine("NICSpeed={0}", ni.Speed);
                Console.WriteLine();
            }
             */

            foreach (string x in regAdapters.GetSubKeyNames())
            {
                try
                {
                    RegistryKey regAdapter = regAdapters.OpenSubKey(x, false);
                    object id = regAdapter.GetValue("ComponentId");

                    if (id != null && (id.ToString().StartsWith("tap0801") ||
                        id.ToString().StartsWith("tap0901")))
                    {
                        string devGuid = regAdapter.GetValue("NetCfgInstanceId").ToString();

                        foreach (String n in regAdapter.GetValueNames())
                            Console.WriteLine(n + "=" + regAdapter.GetValue(n).ToString());

                        adapters.Add(new NetworkAdapterWin(devGuid));
                    }
                }
                catch (Exception ThrownException)
                {
                   System.Console.WriteLine(ThrownException.StackTrace.ToString());
                }
            }

            return adapters;
        }

        public string DeviceGuid { get; private set; }

        private NetworkAdapterWin(string guid)
        {
            DeviceGuid = guid;
        }

        public string Name
        {
            get
            {
                const string ConnectionKey = "SYSTEM\\CurrentControlSet\\Control\\Network\\{4D36E972-E325-11CE-BFC1-08002BE10318}";
                RegistryKey regConnection = Registry.LocalMachine.OpenSubKey(ConnectionKey + "\\" + DeviceGuid + "\\Connection", true);
                object id = regConnection.GetValue("Name");

                if (id != null)
                    return id.ToString();
                return "";
            }
        }

        public LocalEndpointTunTap Open()
        {
            return new LocalEndpointTunTap(DeviceGuid);
        }
    }

    public class LocalEndpointTunTap : LocalEndpoint
    {
        private const string UsermodeDeviceSpace = "\\\\.\\Global\\";

        enum IoControlCodes : uint
        {
            METHOD_BUFFERED = 0,
            FILE_ANY_ACCESS = 0,
            FILE_DEVICE_UNKNOWN = 0x00000022,
            TAP_CONTROL_CODE = (FILE_DEVICE_UNKNOWN << 16) | (FILE_ANY_ACCESS << 14) | METHOD_BUFFERED,

            GET_MAC = TAP_CONTROL_CODE | (1 << 2),
            GET_VERSION = TAP_CONTROL_CODE | (2 << 2),
            GET_MTU = TAP_CONTROL_CODE | (3 << 2),
            GET_INFO = TAP_CONTROL_CODE | (4 << 2),
            CONFIG_POINT_TO_POINT = TAP_CONTROL_CODE | (5 << 2),
            SET_MEDIA_STATUS = TAP_CONTROL_CODE | (6 << 2),
            CONFIG_DHCP_MASQ = TAP_CONTROL_CODE | (7 << 2),
            GET_LOG_LINE = TAP_CONTROL_CODE | (8 << 2),
            CONFIG_DHCP_SET_OPT = TAP_CONTROL_CODE | (9 << 2),
            CONFIG_TUN = TAP_CONTROL_CODE | (10 << 2)
        }

        const int FILE_ATTRIBUTE_SYSTEM = 0x4;
        const int FILE_FLAG_OVERLAPPED = 0x40000000;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr CreateFile(
            string filename,
            [MarshalAs(UnmanagedType.U4)]FileAccess fileaccess,
            [MarshalAs(UnmanagedType.U4)]FileShare fileshare,
            int securityattributes,
            [MarshalAs(UnmanagedType.U4)]FileMode creationdisposition,
            int flags,
            IntPtr template);

        [DllImport("kernel32.dll", ExactSpelling = true, SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool DeviceIoControl(IntPtr hDevice, IoControlCodes dwIoControlCode,
            IntPtr lpInBuffer, uint nInBufferSize,
            IntPtr lpOutBuffer, uint nOutBufferSize,
            out int lpBytesReturned, IntPtr lpOverlapped);

        IntPtr _devPtr;
        FileStream _devStream;

        public LocalEndpointTunTap(string guid)
        {
            _devPtr = CreateFile(UsermodeDeviceSpace + guid + ".tap", FileAccess.ReadWrite,
                FileShare.ReadWrite, 0, FileMode.Open, FILE_ATTRIBUTE_SYSTEM | FILE_FLAG_OVERLAPPED, IntPtr.Zero);

            SetStatus(true);

            SafeFileHandle safeHandle = new SafeFileHandle(_devPtr, true);
            _devStream = new FileStream(safeHandle, FileAccess.ReadWrite, 10000, true);
        }

        public void SetStatus(bool online)
        {
            int dummy;
            IntPtr pstatus = Marshal.AllocHGlobal(4);
            Marshal.WriteInt32(pstatus, online ? 1 : 0);
            DeviceIoControl(_devPtr, IoControlCodes.SET_MEDIA_STATUS, pstatus, 4, pstatus, 4, out dummy, IntPtr.Zero);
            Marshal.FreeHGlobal(pstatus);
        }

        /*
		public void SetTunMode(IP4Address localIP, IP4Address remoteNetwork, IP4Address networkMask)
        {
			int dummy;
			IntPtr ptun = Marshal.AllocHGlobal(12);
			Marshal.WriteInt32(ptun, 0, (int)localIP);
			Marshal.WriteInt32(ptun, 4, (int)remoteNetwork);
			Marshal.WriteInt32(ptun, 8, (int)networkMask);
			DeviceIoControl(_devPtr, IoControlCodes.CONFIG_TUN, ptun, 12, ptun, 12, out dummy, IntPtr.Zero);
			Marshal.FreeHGlobal(ptun);
		}

		public void SetDhcpMode(IP4Address hostAddress, IP4Address networkMask, IP4Address serverAddress, int leaseTime)
        {
			int dummy;
			IntPtr buff = Marshal.AllocHGlobal(16);
			Marshal.WriteInt32(buff, 0, (int)hostAddress); // 0x0100030a
			Marshal.WriteInt32(buff, 4, (int)networkMask); // unchecked((int)0x00ffffff)
			Marshal.WriteInt32(buff, 8, (int)serverAddress); // 0x0200030a
			Marshal.WriteInt32(buff, 12, leaseTime);
			DeviceIoControl(_devPtr, IoControlCodes.CONFIG_DHCP_MASQ, buff, 16, buff, 16, out dummy, IntPtr.Zero);
			Marshal.FreeHGlobal(buff);
		}
         */

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _devStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _devStream.Write(buffer, offset, count);
            _devStream.Flush();
        }

        public void Write(Message packet)
        {
            // packet.WriteTo(_devStream);
            _devStream.Flush();
        }

        public override void Close()
        {
            _devStream.Close();
            // base.Close();
        }

        public override bool CanRead { get { return _devStream.CanRead; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanTimeout { get { return _devStream.CanTimeout; } }
        public override bool CanWrite { get { return _devStream.CanWrite; } }

        public override int ReadTimeout
        {
            get { return _devStream.ReadTimeout; }
            set { _devStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _devStream.WriteTimeout; }
            set { _devStream.WriteTimeout = value; }
        }
    }
}