using System;
using System.Xml.Serialization;
using System.IO;

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

		public SendFileInfo[] FileInfo
		{
			get
			{
				if((Command & PeerMessenger.Command.ProfilePicture) == PeerMessenger.Command.ProfilePicture && Message != null)
				{
					StringReader sr = new StringReader(Message);
					XmlSerializer xs = new XmlSerializer(typeof(SendFileInfo[]));
					SendFileInfo[] fi = (SendFileInfo[])xs.Deserialize(sr);
					return fi;
				}
				else
				{
					return null;
				}
			}
			set
			{
				if(value != null)
				{
					StringWriter sw = new StringWriter();
					XmlSerializer xs = new XmlSerializer(typeof(SendFileInfo[]));
					xs.Serialize(sw, value);
					Message = sw.ToString();
				}
			}
		}
	}
}
