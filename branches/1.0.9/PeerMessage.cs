using System;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for IIpMessage.
	/// </summary>
	public class PeerMessage
	{
		private string _Sender;
		private string _SenderHost;
		private ulong _CommandNumber;
		private string _Message;

		public PeerMessage()
		{
		}

		public PeerMessage(Host h)
		{
			Sender = h.Sender;
			SenderHost = h.HostName;
		}

		public string Sender
		{
			get
			{
				return _Sender;
			}
			set
			{
				_Sender = value;
			}
		}

		public string SenderHost
		{
			get
			{
				return _SenderHost;
			}
			set
			{
				_SenderHost = value;
			}
		}

		public ulong Command
		{
			get
			{
				return _CommandNumber;
			}
			set
			{
				_CommandNumber = value;
			}
		}

		public string Message
		{
			get
			{
				return _Message;
			}
			set
			{
				_Message = value;
			}
		}
	}
}
