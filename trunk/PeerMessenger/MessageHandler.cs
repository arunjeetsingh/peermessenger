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
				byte[] packet = new byte[bytes.Count];

				for(int i = 0; i < bytes.Count; i++)
				{
					packet[i] = byte.Parse(bytes[i].ToString());
				}

				string p = Encoding.ASCII.GetString(packet);
				StringReader sr = new StringReader(p);

				XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
				PeerMessage msg = xs.Deserialize(sr) as PeerMessage;
			
				if((msg.Command & Command.Presence) == Command.Presence)
				{
					Host existingEntry = ivListener.Subscriber.IsClientKnown(msg.Sender);
					if(existingEntry != null && existingEntry.IP)
					{
						ivListener.Subscriber.DeleteClient(existingEntry.Sender);
					}

					string preferredName = msg.Message;
					logger.Debug("Message sender's preferred name is " + preferredName);
					ivListener.Subscriber.GetClient(new Host(msg.Sender, preferredName, msg.SenderHost));
				}		
				else if((msg.Command & Command.Message) == Command.Message)
				{
					logger.Debug(msg.Sender + " sent " + msg.Message);
					ivListener.Subscriber.GetMessage(msg.Sender, msg.Message);
				}				
			}

			ivIncoming.Close();
		}
	}
}
