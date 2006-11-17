using System;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for IIpMessage.
	/// </summary>
	public class IpMessage : PeerMessage
	{
		private string _Version;
		private ulong _Packet;

		public IpMessage()
		{
		}

		public IpMessage(Host h) : base(h)
		{
		}

		public string Version
		{
			get
			{
				return _Version;
			}
			set 
			{
				_Version = value;
			}
		}

		public ulong Packet
		{
			get
			{
				return _Packet;
			}
			set
			{
				_Packet = value;
			}
		}

		public string AdditionalSection
		{
			get
			{
				return Message;
			}
			set
			{
				Message = value;
			}
		}

		public override string ToString()
		{
			string m = Version + ":" + Packet;
			m += ":" + Sender;
			m += ":" + SenderHost;
			m += ":" + Command + ":" + AdditionalSection;

			return m;
		}

	}
}
