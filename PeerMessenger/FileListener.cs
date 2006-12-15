using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Xml;
using log4net;
using System.IO;
using System.Text;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for FileListener.
	/// </summary>
	public class FileListener
	{
		private int ivPort;
		private bool ivStop;
		private ISubscriber ivSubcriber;
		private ArrayList ivHandlers = new ArrayList();
		private TcpListener ivListener;
		private Hashtable ivFileSendQueue;
		private ILog logger = LogManager.GetLogger(typeof(FileListener));
		byte[] packet;

		public FileListener(int port, ISubscriber sub)
		{
			ivPort = port;
			ivSubcriber = sub;
			ivStop = false;
		}

		public void ReadData(IAsyncResult result)
		{
			if(result.IsCompleted)
			{
				TcpClient client = result.AsyncState as TcpClient;
				NetworkStream stream = client.GetStream();

				string p = Encoding.ASCII.GetString(packet);
				IpMessage m = IpMessageLoader.Load(p);						

				if(m.Command == Command.IPMSG_GETFILEDATA)
				{
					logger.Debug("Got file request: " + m.ToString().Replace('\0', ' '));
					string[] parts = m.AdditionalSection.Split(':');
					uint originalPacket = 0;
					if(parts[0] != "0")
					{
						originalPacket = Convert.ToUInt32(parts[0], 16);
						originalPacket--;
					}

					int fileId = int.Parse(parts[1]);
					int offset = int.Parse(parts[2]);

					if(FileSendQueue.Contains(originalPacket))
					{
						Hashtable files = (Hashtable)FileSendQueue[originalPacket];
						SendFileInfo file = files[fileId] as SendFileInfo;
						_SendFile(file.FullName, offset, stream);

						files.Remove(fileId);
						if(files.Values.Count == 0)
						{
							FileSendQueue.Remove(originalPacket);	
						}
					}
					else if(originalPacket == 0 && fileId == 0)
					{
						//Send profile picture
						_SendFile(ConfigurationManager.ProfilePicture, 0, stream);
					}
				}

				stream.Close();
				client.Close();
			}
		}

		private void _SendFile(string fullName, int offset, NetworkStream stream)
		{
			FileStream fs = File.OpenRead(fullName);
			byte[] contents = new byte[fs.Length];
			fs.Read(contents, offset, (int)fs.Length);
			fs.Close();
			stream.Write(contents, 0, contents.Length);
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
					NetworkStream stream = client.GetStream();
					packet = new byte[1024];
					stream.BeginRead(packet, 0, packet.Length, new AsyncCallback(ReadData), client);
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

		public Hashtable FileSendQueue
		{
			get
			{
				if(ivFileSendQueue == null)
				{
					ivFileSendQueue = new Hashtable();
				}

				return ivFileSendQueue;
			}
		}
	}	
}
