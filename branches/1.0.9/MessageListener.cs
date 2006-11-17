using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Xml;
using log4net;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for MessageListener.
	/// </summary>
	public class MessageListener
	{
		private int ivPort;
		private bool ivStop;
		private ISubscriber ivSubcriber;
		private ArrayList ivHandlers = new ArrayList();
		private TcpListener ivListener;
		private ILog logger = LogManager.GetLogger(typeof(MessageListener));

		public MessageListener(int port, ISubscriber sub)
		{
			ivPort = port;
			ivSubcriber = sub;
			ivStop = false;
		}

		public void Listen()
		{
			ivListener = new TcpListener(IPAddress.Any, ivPort);
			ivListener.Start();
			while(ivStop == false)
			{			
				try
				{
					TcpClient client = ivListener.AcceptTcpClient();
					client.NoDelay = true;
					logger.Debug("Recieved message");
					ArrayList subs = new ArrayList();
					MessageHandler handler = new MessageHandler(client, this);
					Thread t = new Thread(new ThreadStart(handler.Start));
					ivHandlers.Add(t);
					t.Start();
				}
				catch(SocketException se)
				{
					if(se.NativeErrorCode == 10004)
					{
						logger.Debug(se.Message);
					}
					else
					{
						throw se;
					}
				}				
			}

			foreach(Thread t in ivHandlers)
			{
				if(t.ThreadState == ThreadState.Running)
				{
					t.Join();
				}
			}
		}

		public void Stop()
		{
			ivStop = true;
			ivListener.Stop();
		}

		public ISubscriber Subscriber
		{
			get
			{
				return ivSubcriber;
			}
		}
	}	
}
