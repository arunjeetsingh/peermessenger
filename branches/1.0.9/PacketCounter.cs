using System;

namespace PeerMessenger
{
	/// <summary>
	/// The PacketCounter is a singleton responsible for generating packet
	/// numbers that the IP Messenger protocol uses.
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
