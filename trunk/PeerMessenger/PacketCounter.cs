using System;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for PacketCounter.
	/// </summary>
	public sealed class PacketCounter
	{
		private PacketCounter()
		{
		}

		private static long _Packet = 0;
		private static object _SyncLock = new object();

		public static long Packet
		{
			get
			{
				lock(_SyncLock)
				{
					if(_Packet == 0)
					{
						_Packet = DateTime.Now.ToFileTime();
					}
					else
					{
						_Packet++;
					}
				}

				return _Packet;
			}
		}
	}
}
