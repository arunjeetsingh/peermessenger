using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Xml;
using log4net;
using System.Xml.Serialization;
using System.Text;
using System.IO;

namespace PeerMessenger
{
	public class MessageHandler
	{
		private TcpClient ivIncoming;
		private MessageListener ivListener;
		private ILog logger = LogManager.GetLogger(typeof(MessageHandler));

		public MessageHandler(TcpClient incoming, MessageListener listener)
		{
			ivIncoming = incoming;
			ivListener = listener;
		}

		public void Start()
		{
			NetworkStream stream = ivIncoming.GetStream();
			if(stream.DataAvailable)
			{
				ArrayList bytes = new ArrayList();
				while(stream.DataAvailable)
				{
					bytes.Add(stream.ReadByte());
				}
				stream.Close();

				byte[] packet = new byte[bytes.Count];

				for(int i = 0; i < bytes.Count; i++)
				{
					packet[i] = byte.Parse(bytes[i].ToString());
				}

				string p = Encoding.ASCII.GetString(packet);
				IpMessage m = IpMessageLoader.Load(p);
			}

			ivIncoming.Close();
		}
	}
}
