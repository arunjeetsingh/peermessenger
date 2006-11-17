using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace PeerMessenger
{
	[StructLayout(LayoutKind.Sequential)]
	public struct FLASHWINFO
	{
		public UInt16 cbSize;
		public IntPtr hwnd;
		public UInt32 dwFlags;
		public UInt16 uCount;
		public UInt32 dwTimeout;
	}

	public class FlashWindowHelper
	{
		//Stop flashing. The system restores the window to its original state.
		public const UInt32 FLASHW_STOP = 0;
		//Flash the window caption.
		public const UInt32 FLASHW_CAPTION = 1;
		//Flash the taskbar button.
		public const UInt32 FLASHW_TRAY = 2;
		//Flash both the window caption and taskbar button.
		//This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
		public const UInt32 FLASHW_ALL = 3;
		//Flash continuously, until the FLASHW_STOP flag is set.
		public const UInt32 FLASHW_TIMER = 4;
		//Flash continuously until the window comes to the foreground.
		public const UInt32 FLASHW_TIMERNOFG = 12; 

		[DllImport("user32.dll")]
		static extern Int16 FlashWindowEx(ref FLASHWINFO pwfi);

		/// <summary>
		/// Flashes a window
		/// </summary>
		/// <param name="hWnd">The handle to the window to flash</param>
		/// <returns>whether or not the window needed flashing</returns>
		public static bool FlashWindowEx(IntPtr hWnd)
		{
			FLASHWINFO fInfo = new FLASHWINFO();

			fInfo.cbSize = (ushort)Marshal.SizeOf(fInfo);
			fInfo.hwnd = hWnd;
			fInfo.dwFlags = FLASHW_TIMERNOFG | FLASHW_TRAY;
			fInfo.uCount = UInt16.MaxValue;
			fInfo.dwTimeout = 0;

			return (FlashWindowEx(ref fInfo) == 0);
		}
	}

	/// <summary>
	/// Summary description for Class1.
	/// </summary>
	/// 
	public class NetApi32
	{
		// constants
		public const uint ERROR_SUCCESS        = 0;
		public const uint ERROR_MORE_DATA    = 234;
		public enum SV_101_TYPES:uint
		{
			SV_TYPE_WORKSTATION= 0x00000001,
			SV_TYPE_SERVER= 0x00000002,
			SV_TYPE_SQLSERVER = 0x00000004,
			SV_TYPE_DOMAIN_CTRL= 0x00000008,
			SV_TYPE_DOMAIN_BAKCTRL= 0x00000010,
			SV_TYPE_TIME_SOURCE= 0x00000020,
			SV_TYPE_AFP= 0x00000040,
			SV_TYPE_NOVELL= 0x00000080,
			SV_TYPE_DOMAIN_MEMBER = 0x00000100,
			SV_TYPE_PRINTQ_SERVER = 0x00000200,
			SV_TYPE_DIALIN_SERVER = 0x00000400,
			SV_TYPE_XENIX_SERVER = 0x00000800,
			SV_TYPE_SERVER_UNIX = 0x00000800,
			SV_TYPE_NT = 0x00001000,
			SV_TYPE_WFW = 0x00002000,
			SV_TYPE_SERVER_MFPN= 0x00004000,
			SV_TYPE_SERVER_NT = 0x00008000,
			SV_TYPE_POTENTIAL_BROWSER = 0x00010000,
			SV_TYPE_BACKUP_BROWSER= 0x00020000,
			SV_TYPE_MASTER_BROWSER= 0x00040000,
			SV_TYPE_DOMAIN_MASTER = 0x00080000,
			SV_TYPE_SERVER_OSF= 0x00100000,
			SV_TYPE_SERVER_VMS= 0x00200000,
			SV_TYPE_WINDOWS= 0x00400000, 
			SV_TYPE_DFS= 0x00800000, 
			SV_TYPE_CLUSTER_NT= 0x01000000, 
			SV_TYPE_TERMINALSERVER= 0x02000000, 
			SV_TYPE_CLUSTER_VS_NT = 0x04000000, 
			SV_TYPE_DCE= 0x10000000, 
			SV_TYPE_ALTERNATE_XPORT= 0x20000000, 
			SV_TYPE_LOCAL_LIST_ONLY= 0x40000000, 
			SV_TYPE_DOMAIN_ENUM= 0x80000000,
			SV_TYPE_ALL= 0xFFFFFFFF 
		};



		[StructLayout(LayoutKind.Sequential)]
			public struct SERVER_INFO_101
		{
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] 
			public UInt32 sv101_platform_id;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)]
			public string sv101_name;

			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32 sv101_version_major;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32 sv101_version_minor;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.U4)] public UInt32 sv101_type;
			[MarshalAs(System.Runtime.InteropServices.UnmanagedType.LPWStr)] public string sv101_comment;
		};

		public enum PLATFORM_ID
		{
			PLATFORM_ID_DOS = 300,
			PLATFORM_ID_OS2 = 400,
			PLATFORM_ID_NT = 500,
			PLATFORM_ID_OSF = 600,
			PLATFORM_ID_VMS = 700
		}

		[DllImport("netapi32.dll",EntryPoint="NetServerEnum")]
		public static extern int NetServerEnum( [MarshalAs(UnmanagedType.LPWStr)]string servername, 
			int level, 
			out IntPtr bufptr, 
			int prefmaxlen,
			ref int entriesread, 
			ref int totalentries,
			SV_101_TYPES servertype,
			[MarshalAs(UnmanagedType.LPWStr)]string domain,
			int resume_handle);


		[DllImport("netapi32.dll",EntryPoint="NetApiBufferFree")]
		public static extern int 
			NetApiBufferFree(IntPtr buffer);

		public static ArrayList GetServerList( NetApi32.SV_101_TYPES ServerType)
		{
			int entriesread=0,totalentries=0;
			ArrayList alServers = new  ArrayList();

			do 
			{
				// Buffer to store the available servers
				// Filled by the NetServerEnum function
				IntPtr buf = new IntPtr();



				SERVER_INFO_101 server;
				int ret = NetServerEnum(null,101,out buf,-1,
					ref entriesread,ref totalentries,
					ServerType,null,0);

				// if the function returned any data, fill the tree view
				if(    ret == ERROR_SUCCESS    || 
					ret == ERROR_MORE_DATA    ||
					entriesread > 0)
				{
					Int32 ptr = buf.ToInt32();

					for(int i=0; i<entriesread; i++)
					{
						// cast pointer to a SERVER_INFO_101 structure
						server = (SERVER_INFO_101)Marshal.PtrToStructure(new IntPtr(ptr),typeof(SERVER_INFO_101));

						ptr += Marshal.SizeOf(server);

						// add the machine name and comment to the arrayList. 
						//You could return the entire structure here if desired
						alServers.Add( server);
					}
				}

				// free the buffer 
				NetApiBufferFree(buf);

			} 
			while
				(
				entriesread < totalentries && 
				entriesread != 0
				);

			return alServers;
		}
	}
}
