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
	/// Summary description for UdpBroadcastManager.
	/// </summary>
	public class UdpBroadcastManager
	{
		private int ivPort;
		private bool ivStop = false;
		UdpClient listener, listenerIp;
		IPAddress group;
		ISubscriber ivSub;
		Host ivSelf;
		Hashtable packetTracker;
		private ILog logger = LogManager.GetLogger(typeof(UdpBroadcastManager));

		public ISubscriber Subscriber
		{
			get
			{
				return ivSub;
			}
		}

		public UdpBroadcastManager(int port, ISubscriber sub, Host self)
		{
			ivPort = port;
			ivSub = sub;
			listener = new UdpClient(ivPort);
			group = IPAddress.Parse("226.254.82.220");
			int ttl = 5;
			listener.JoinMulticastGroup(group, ttl);
			listenerIp = new UdpClient(2425);
			ivSelf = self;
			packetTracker = new Hashtable();
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

					// Verify first 128 bits are indeed our guid
					if (request.Length >= MessageFormatter.Identifier.ToString().Length)
					{
						logger.Debug("Broadcast from " + callerEndpoint.Address.ToString());
						string poo = Encoding.ASCII.GetString(request);

						if (p.StartsWith(MessageFormatter.Identifier.ToString()))
						{
							string message = poo.Substring(MessageFormatter.Identifier.ToString().Length);
							StringReader sr = new StringReader(message);
							XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
							PeerMessage msg = xs.Deserialize(sr) as PeerMessage;
							
							if((msg.Command & Command.Presence) == Command.Presence)
							{								
								string preferredName = msg.Message;
								logger.Debug("Message is a presence message from " + preferredName);

								if(preferredName != ivSelf.PreferredName)
								{
									bool forceConfirm = ((msg.Command & Command.ForceConfirmation) == Command.ForceConfirmation);
									logger.Debug("forceConfirm is " + forceConfirm);

									Host existingEntry = ivSub.IsClientKnown(msg.Sender);
									if(existingEntry != null && existingEntry.IP)
									{
										ivSub.DeleteClient(existingEntry.Sender);
									}

									if(ivSub.IsClientKnown(msg.Sender) == null || forceConfirm)
									{
										if(ivSub.IsClientKnown(msg.Sender) == null)
										{
											ivSub.GetClient(new Host(msg.Sender, preferredName, msg.SenderHost));
										}
								
										// Send response (our guid, followed by serialized endpoint info)
										ConfirmPresence(ivSelf, callerEndpoint);
									}		
								}
								else
								{
									logger.Debug("Message to self. Ignored.");
								}
							}
							else if((msg.Command & Command.Absence) == Command.Absence)
							{
								string preferredName = msg.Message;
								logger.Debug("Message is an absence message from " + preferredName);
								if(ivSub.IsClientKnown(msg.Sender) != null)
								{
									ivSub.DeleteClient(msg.Sender);
								}
							}
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Debug(ex.Message);
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
					logger.Debug("Recieved IP message " + p);

					IpMessage m = IpMessageLoader.Load(p);
					bool duplicate = _IsDuplicateMessage(m);
					if(m != null && duplicate == false)
					{
						if((m.Command & Command.IPMSG_ANSENTRY) == Command.IPMSG_ANSENTRY)
						{
							Subscriber.GetClient(new Host(m.Sender, m.AdditionalSection, m.SenderHost, true));
						}
						else if((m.Command & Command.IPMSG_BR_ENTRY) == Command.IPMSG_BR_ENTRY && (m.Command & Command.IPMSG_FILEATTACHOPT) == Command.IPMSG_FILEATTACHOPT)
						{
							Subscriber.GetClient(new Host(m.Sender, m.AdditionalSection, m.SenderHost, true));
							AnswerEntry(m);
						}
						else if((m.Command & Command.IPMSG_BR_EXIT) == Command.IPMSG_BR_EXIT && (m.Command & Command.IPMSG_FILEATTACHOPT) == Command.IPMSG_FILEATTACHOPT)
						{
							Subscriber.DeleteClient(m.Sender);
						}
						else if(m.Command == Command.IPMSG_RECVMSG)
						{
							//Message acknowledged
						}
						else if((m.Command & Command.IPMSG_SENDMSG) == Command.IPMSG_SENDMSG)
						{
							Subscriber.GetMessage(m.Sender, m.Message);
							if((m.Command & Command.IPMSG_SENDCHECKOPT) == Command.IPMSG_SENDCHECKOPT)
							{
								AcknowledgeMessage(m);
							}
						}
					}
					else
					{
						if((m.Command & Command.IPMSG_SENDCHECKOPT) == Command.IPMSG_SENDCHECKOPT)
						{
							AcknowledgeMessage(m);
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Debug(ex.Message);
			}
		}

		private void AcknowledgeMessage(IpMessage m)
		{
			byte[] bMsg = MessageFormatter.FormatIpMessage(ivSelf, m.Packet.ToString(), Command.IPMSG_RECVMSG);
			listenerIp.Send(bMsg, bMsg.Length, m.SenderHost, 2425);
		}

		private void AnswerEntry(IpMessage m)
		{
			byte[] bMsg = MessageFormatter.FormatIpMessage(ivSelf, ivSelf.PreferredName, Command.IPMSG_ANSENTRY);
			listenerIp.Send(bMsg, bMsg.Length, m.SenderHost, 2425);
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

		public void SendIP(string host, string message)
		{
			byte[] broadCast = MessageFormatter.FormatIpMessage(ivSelf, message);
			listenerIp.Send(broadCast, broadCast.Length, host, 2425);
		}

		public void BroadcastIPAbsence()
		{
			byte[] broadCast = MessageFormatter.GetIPAbsenceMessage(ivSelf);
			listenerIp.Send(broadCast, broadCast.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 2425));
		}

		public void BroadcastIPPresence()
		{
			byte[] msg = MessageFormatter.GetIPMsgNoOp(ivSelf);
			listenerIp.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 2425));
			msg = MessageFormatter.GetIPMsgPresenceMessage(ivSelf);
			listenerIp.Send(msg, msg.Length, new IPEndPoint(IPAddress.Parse("255.255.255.255"), 2425));
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
			BroadcastPresence(h, forceConfirm, MessageFormatter.GetPresenceMessage(h, true, forceConfirm), new IPEndPoint(IPAddress.Parse("226.254.82.220"), ivPort), null);
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
			IPEndPoint groupEP = new IPEndPoint(IPAddress.Parse("226.254.82.220"),ivPort);
			sender.Send(broadcast, broadcast.Length, groupEP);
			sender.Close();
		}

		public void ConfirmPresence(Host h, IPEndPoint callerEndPoint)
		{
			logger.Debug("Confirming presence of " + h.PreferredName + " to " + callerEndPoint.Address);
			TcpClient client = new TcpClient();
			client.NoDelay = true;
			client.Connect(callerEndPoint.Address, 3089);
			byte[] broadcast = MessageFormatter.GetPresenceMessage(h, false);

			NetworkStream stream = client.GetStream();
			stream.Write(broadcast, 0, broadcast.Length);
			stream.Close();
			client.Close();
		}		
	}	
}
