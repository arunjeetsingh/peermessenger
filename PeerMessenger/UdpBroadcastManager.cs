using System;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Collections;
using log4net;
using System.Text;
using System.IO;
using System.Xml.Serialization;

namespace PeerMessenger
{	
	/// <summary>
	/// Summary description for UdpManager.
	/// </summary>
	public class UdpManager
	{
		private int ivPort;
		private bool ivStop = false;
		UdpClient listener, listenerIp;
		IPAddress group;
		ISubscriber ivSub;
		Host ivSelf;
		Hashtable packetTracker;
		FileListener ivFileListener;
		private ILog logger = LogManager.GetLogger(typeof(UdpManager));
		private const string _PeerIP = "226.254.82.220";
		private const string _BroadcastIP = "255.255.255.255";
		private const int _IPPort = 2425;
		private Hashtable ivProfilePictureCache;

		public ISubscriber Subscriber
		{
			get
			{
				return ivSub;
			}
		}

		protected Hashtable ProfilePictureCache
		{
			get
			{
				if(ivProfilePictureCache == null)
				{
					ivProfilePictureCache = new Hashtable();
				}

				return ivProfilePictureCache;
			}
		}

		public UdpManager(int port, ISubscriber sub, Host self, FileListener fileListener)
		{
			ivPort = port;
			ivSub = sub;
			listener = new UdpClient(ivPort);
			group = IPAddress.Parse(_PeerIP);
			int ttl = 5;
			listener.JoinMulticastGroup(group, ttl);
			listenerIp = new UdpClient(_IPPort);
			ivSelf = self;
			packetTracker = new Hashtable();
			ivFileListener = fileListener;
		}

		public void Listen()
		{
			try
			{
				while(ivStop == false)
				{
					// Wait for broadcast... will block until data recv'd, 
					// or underlying socket is closed
					IPEndPoint callerEndpoint = null;
					byte[] request = listener.Receive(ref callerEndpoint);
					string p = Encoding.ASCII.GetString(request, 0, request.Length);

					logger.Debug("Broadcast from " + callerEndpoint.Address.ToString());

					string message = Encoding.ASCII.GetString(request);							
					_ProcessPeerMessage(message);
				}
			}
			catch(Exception ex)
			{
				logger.Debug(ex.Message);
			}
		}

		private void _ProcessPeerMessage(string message)
		{
			StringReader sr = new StringReader(message);
			XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
			PeerMessage msg = xs.Deserialize(sr) as PeerMessage;

			if((msg.Command & Command.Presence) == Command.Presence)
			{
				Host newClient = new Host(msg.Sender, msg.Message, msg.SenderHost);
				if(ProfilePictureCache.Contains(msg.Sender))
				{
					newClient.ProfilePicture = ProfilePictureCache[msg.Sender] as SendFileInfo;
					ProfilePictureCache.Remove(msg.Sender);
				}

				Subscriber.GetClient(newClient);
				ConfirmPresence(newClient);
			}
			else if((msg.Command & Command.AcknowledgePresence) == Command.AcknowledgePresence)
			{
				Host newClient = new Host(msg.Sender, msg.Message, msg.SenderHost);				
				if(ProfilePictureCache.Contains(msg.Sender))
				{
					newClient.ProfilePicture = ProfilePictureCache[msg.Sender] as SendFileInfo;
					ProfilePictureCache.Remove(msg.Sender);
				}

				Subscriber.GetClient(newClient);
			}							
			else if((msg.Command & Command.Absence) == Command.Absence)
			{
				Subscriber.DeleteClient(msg.Sender);
			}
			else if((msg.Command & Command.StatusMessage) == Command.StatusMessage)
			{
				Subscriber.GetStatusMessage(msg.Sender, msg.Message);
			}
			else if((msg.Command & Command.ProfilePicture) == Command.ProfilePicture)
			{
				Host h = Subscriber.IsClientKnown(msg.Sender);
				if(h != null)
				{
					h.ProfilePicture = msg.FileInfo[0];
					Subscriber.GetClient(h);
				}
				else
				{
					//Cache the profile picture for later use
					ProfilePictureCache[msg.Sender] = msg.FileInfo[0];
				}
			}
			else if((msg.Command & Command.AcknowledgeMessage) == Command.AcknowledgeMessage)
			{
				//Message acknowledged
			}
			else if((msg.Command & Command.Message) == Command.Message)
			{
				Subscriber.GetMessage(msg.Sender, msg.Message);
			}
		}

		public void ListenIP()
		{
			try
			{
				while(ivStop == false)
				{
					// Wait for broadcast... will block until data recv'd, 
					// or underlying socket is closed
					IPEndPoint callerEndpoint = null;
					byte[] request = listenerIp.Receive(ref callerEndpoint);
					string p = Encoding.ASCII.GetString(request, 0, request.Length);
					logger.Debug("Recieved IP message " + p.Replace('\0', ' '));

					IpMessage m = IpMessageLoader.Load(p);
					bool duplicate = _IsDuplicateMessage(m);
					if(m != null && duplicate == false)
					{
						if(m.Command == Command.IPMSG_RECVMSG)
						{
							//Message acknowledged
							//Answer to IPMSG_SENDCHECKOPT
						}
						else if(m.Command == Command.IPMSG_READMSG)
						{
							//Sealed message opened
							Host h = Subscriber.IsClientKnown(m.Sender);

							if(h != null)
							{
								Subscriber.GetStatusMessage(m.Sender, h.PreferredName + " has opened your message.");
							}
						}
						else if(m.Command == Command.IPMSG_RELEASEFILES)
						{
							//they don't want your stinking files!
							Host h = Subscriber.IsClientKnown(m.Sender);
							if(h != null)
							{
								Subscriber.GetStatusMessage(m.Sender, h.PreferredName + " has declined your file(s).");
								uint originalPacket = Convert.ToUInt32(m.AdditionalSection);
								originalPacket--;
								ivFileListener.FileSendQueue.Remove(originalPacket);
							}
						}
						else if((m.Command & Command.IPMSG_ANSENTRY) == Command.IPMSG_ANSENTRY)
						{
							Host newClient = new Host(m.Sender, m.AdditionalSection, m.SenderHost, true);

							//Load up the profile pic info if we have it cached
							if(ProfilePictureCache.Contains(m.Sender))
							{
								newClient.ProfilePicture = ProfilePictureCache[m.Sender] as SendFileInfo;
								ProfilePictureCache.Remove(m.Sender);
							}

							//Presence broadcast answered. Add this peer to your list
							Subscriber.GetClient(newClient);							
						}
						else if((m.Command & Command.IPMSG_BR_ENTRY) == Command.IPMSG_BR_ENTRY && (m.Command & Command.IPMSG_FILEATTACHOPT) == Command.IPMSG_FILEATTACHOPT)
						{
							Host newClient = new Host(m.Sender, m.AdditionalSection, m.SenderHost, true);
							//Load up the profile pic info if we have it cached
							if(ProfilePictureCache.Contains(m.Sender))
							{
								newClient.ProfilePicture = ProfilePictureCache[m.Sender] as SendFileInfo;
								ProfilePictureCache.Remove(m.Sender);
							}

							//Presence broadcast from peer
							Subscriber.GetClient(newClient);							
							AnswerEntry(m);
						}
						else if((m.Command & Command.IPMSG_BR_EXIT) == Command.IPMSG_BR_EXIT && (m.Command & Command.IPMSG_FILEATTACHOPT) == Command.IPMSG_FILEATTACHOPT)
						{
							//Peer saying bye bye
							Subscriber.DeleteClient(m.Sender);
						}
						else if((m.Command & (Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT)) == (Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT))
						{
							//files coming our way
							string[] parts = m.Message.Split('\0');
							
							if(parts != null && parts.Length > 0)
							{
								//See if there's a message accompanying the files
								if(parts[0].Length > 0)
								{
									Subscriber.GetMessage(m.Sender, parts[0]);
								}

								//Parse info about the individual files
								parts = parts[1].Split('\a');
								SendFileInfo[] files = new SendFileInfo[parts.Length - 1];
								for(int i = 0; i < parts.Length - 1; i++)
								{
									string[] parts2 = parts[i].Split(':');
									files[i] = new SendFileInfo();
									files[i].ID = int.Parse(parts2[0]);
									files[i].Name = parts2[1];
									files[i].Size = Convert.ToInt64(parts2[2], 16);
									files[i].TimeModified = Convert.ToInt64(parts2[3], 16);
									files[i].Attribute = Convert.ToInt32(parts2[4]);
								}

								uint inputPacket = ((uint)m.Packet);
								//Tell subscribers that we have incoming files
								Subscriber.GetFiles(m.Sender, files, inputPacket);
							}

							//Acknowledge the message if its needed
							if((m.Command & Command.IPMSG_SENDCHECKOPT) == Command.IPMSG_SENDCHECKOPT)
							{
								AcknowledgeMessage(m);
							}
						}
						else if((m.Command & Command.IPMSG_SENDMSG) == Command.IPMSG_SENDMSG)
						{
							//Received a text message
							Subscriber.GetMessage(m.Sender, m.Message);

							//Acknowledge the message if its needed
							if((m.Command & Command.IPMSG_SENDCHECKOPT) == Command.IPMSG_SENDCHECKOPT)
							{
								AcknowledgeMessage(m);
							}
						}
						else if((m.Command & Command.IPMSG_PEERMESSAGE) == Command.IPMSG_PEERMESSAGE)
						{
							//Got a peer message over IP
							_ProcessPeerMessage(m.AdditionalSection);
						}
					}
					else
					{
						//Acknowledge any stray messages
						if((m.Command & Command.IPMSG_SENDCHECKOPT) == Command.IPMSG_SENDCHECKOPT)
						{
							AcknowledgeMessage(m);
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void AcknowledgeMessage(IpMessage m)
		{
			byte[] bMsg = MessageFormatter.FormatIpMessage(ivSelf, m.Packet.ToString(), Command.IPMSG_RECVMSG);
			listenerIp.Send(bMsg, bMsg.Length, m.SenderHost, _IPPort);
		}

		private void AnswerEntry(IpMessage m)
		{
			byte[] bMsg = MessageFormatter.FormatIpMessage(ivSelf, ivSelf.PreferredName, Command.IPMSG_ANSENTRY);
			listenerIp.Send(bMsg, bMsg.Length, m.SenderHost, _IPPort);

			if(ConfigurationManager.ProfilePicture != null && ConfigurationManager.ProfilePicture.Length > 0)
			{
				bMsg = _GetIPProfilePictureMessage(ConfigurationManager.ProfilePicture);
				listenerIp.Send(bMsg, bMsg.Length, m.SenderHost, _IPPort);
			}
		}

		private bool _IsDuplicateMessage(IpMessage m)
		{
			bool retVal = false;
			ArrayList packetQueue = null;
			if(packetTracker.Contains(m.Sender))
			{
				packetQueue = packetTracker[m.Sender] as ArrayList;
				if(packetQueue.Contains(m.Packet))
				{
					retVal = true;
				}
				else
				{
					packetQueue.Add(m.Packet);
					retVal = false;
				}
			}
			else
			{
				packetTracker.Add(m.Sender, new ArrayList());
				packetQueue = packetTracker[m.Sender] as ArrayList;
				packetQueue.Add(m.Packet);
				retVal = false;
			}

			if(packetQueue.Count > 50)
			{
				packetQueue.RemoveRange(0, packetQueue.Count - 50);
			}

			return retVal;
		}

		public void Send(string host, string message)
		{
			string m = MessageFormatter.FormatMessageAsString(ivSelf, message, Command.Message);
			byte[] broadcast = Encoding.ASCII.GetBytes(m);
			listener.Send(broadcast, broadcast.Length, host, ivPort);
		}

		public void SendPeerMessageOverIP(string host, PeerMessage msg)
		{
			XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
			StringWriter sr = new StringWriter();
			xs.Serialize(sr, msg);
			string pm = sr.GetStringBuilder().ToString();

			byte[] msgBuf = MessageFormatter.FormatIpMessage(ivSelf, pm, Command.IPMSG_PEERMESSAGE);
			listenerIp.Send(msgBuf, msgBuf.Length, host, _IPPort);
		}

		public void SendIP(string host, string message)
		{
			SendIP(host, message, false);
		}

		public void SendIP(string host, string message, bool seal)
		{
			byte[] broadCast = MessageFormatter.FormatIpMessage(ivSelf, message, seal);
			listenerIp.Send(broadCast, broadCast.Length, host, _IPPort);
		}

		public void SendIPFile(string host, Hashtable files)
		{
			IpMessage m = null;
			byte[] msg = MessageFormatter.FormatIpFileSendInitMessage(ivSelf, files, ref m);
			ivFileListener.FileSendQueue[(uint)m.Packet] = files;
			listenerIp.Send(msg, msg.Length, host, _IPPort);
		}

		public void BroadcastIPAbsence()
		{
			byte[] broadCast = MessageFormatter.GetIPAbsenceMessage(ivSelf);
			listenerIp.Send(broadCast, broadCast.Length, new IPEndPoint(IPAddress.Parse(_BroadcastIP), _IPPort));
		}

		public void BroadcastIPPresence()
		{
			byte[] msg = MessageFormatter.GetIPMsgNoOp(ivSelf);
			listenerIp.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse(_BroadcastIP), _IPPort));
			msg = MessageFormatter.GetIPMsgPresenceMessage(ivSelf);
			listenerIp.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse(_BroadcastIP), _IPPort));
		}

		public void BroadcastIPProfilePicture(string filePath)
		{			
			byte[] msgBuf = _GetIPProfilePictureMessage(filePath);
			listenerIp.Send(msgBuf, msgBuf.Length, new IPEndPoint(IPAddress.Parse(_BroadcastIP), _IPPort));
		}

		private byte[] _GetIPProfilePictureMessage(string filePath)
		{
			SendFileInfo[] fi = null;
			if(filePath != null)
			{
				fi = new SendFileInfo[1];
				fi[0] = new SendFileInfo(filePath);
			}

			PeerMessage msg = new PeerMessage(ivSelf);
			msg.Command = Command.ProfilePicture;
			msg.FileInfo = fi;

			XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
			StringWriter sr = new StringWriter();
			xs.Serialize(sr, msg);
			string pm = sr.GetStringBuilder().ToString();

			byte[] msgBuf = MessageFormatter.FormatIpMessage(ivSelf, pm, Command.IPMSG_PEERMESSAGE);
			return msgBuf;
		}

		public void BroadcastPresence()
		{
			BroadcastPresence(ivSelf, false);
		}

		public void Stop()
		{
			ivStop = true;
			listener.Close();
			listenerIp.Close();
		}

		public void BroadcastPresence(bool forceConfirm)
		{
			BroadcastPresence(ivSelf, forceConfirm);
		}

		public void BroadcastPresence(Host h, bool forceConfirm)
		{
			BroadcastPresence(h, forceConfirm, MessageFormatter.GetPresenceMessage(h, forceConfirm), new IPEndPoint(IPAddress.Parse(_PeerIP), ivPort), null);
		}

		public void BroadcastPresence(Host h, bool forceConfirm, byte[] presenceMessage, IPEndPoint to, IPEndPoint from)
		{
			logger.Debug("Broadcasting presence with preferredName: " + h.PreferredName + ", forceConfirm: " + forceConfirm);
			byte[] broadcast = presenceMessage;

			UdpClient client = null;
			if(from != null)
			{
				client = new UdpClient(from);
			}
			else
			{
				client = new UdpClient();
			}

			client.Send(broadcast, broadcast.Length, to);
			client.Close();
		}

		public void BroadcastAbsence(Host self)
		{
			logger.Debug("Broadcasting absence with preferredName: " + self.PreferredName);
			byte[] broadcast = MessageFormatter.GetAbsenceMessage(self);

			// Dynamically allocate client port
			UdpClient sender = new UdpClient();

			// Send message to the multicast group
			IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse(_PeerIP),ivPort);
			sender.Send(broadcast, broadcast.Length, groupEP);
			sender.Close();
		}

		public void ConfirmPresence(Host h)
		{
			byte[] bMsg = Encoding.ASCII.GetBytes(MessageFormatter.FormatMessageAsString(ivSelf, ivSelf.PreferredName, Command.AcknowledgePresence));
			listener.Send(bMsg, bMsg.Length, h.HostName, ivPort);
		}		
	}	
}
