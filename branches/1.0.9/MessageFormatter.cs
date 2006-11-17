using System;
using System.Xml;
using log4net;
using System.Text;
using System.Xml.Serialization;
using System.IO;
using System.Collections;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for MessageFormatter.
	/// </summary>
	internal sealed class MessageFormatter
	{
		private static ILog logger = LogManager.GetLogger(typeof(MessageFormatter));		

		private MessageFormatter()
		{
		}

		#region PeerMessenger messages
		internal static byte[] GetPresenceMessage(Host h)
		{
			return GetPresenceMessage(h, false);
		}

		internal static byte[] GetPresenceMessage(Host h, bool forceConfirmation)
		{
			ulong command = Command.Presence;
			if(forceConfirmation)
			{
				command |= Command.ForceConfirmation;
			}

			return GetPresenceMessage(h, command);
		}

		internal static byte[] GetPresenceMessage(Host h, ulong command)
		{
			string message = string.Empty;
			message += FormatMessageAsString(h, h.PreferredName, command);

			byte[] msg = Encoding.ASCII.GetBytes(message);
			return msg;
		}

		internal static byte[] GetAbsenceMessage(Host self)
		{
			return FormatMessage(self, self.PreferredName, Command.Absence);
		}

		internal static byte[] FormatMessage(Host h, string message)
		{
			return FormatMessage(h, message, Command.Message);
		}
		
		/// <summary>
		/// Formats a message per the peer messenger protocol and returns it
		/// as a byte array suitable for sending over a network connection.
		/// </summary>
		/// <param name="h">Sending host</param>
		/// <param name="message">Message in string form</param>
		/// <returns>The message protocol-bound and formatted as a byte array</returns>
		internal static byte[] FormatMessage(Host h, string message, ulong command)
		{
			byte[] msg = Encoding.ASCII.GetBytes(FormatMessageAsString(h, message, command));
			return msg;
		}

		internal static string FormatMessageAsString(Host h, string message, ulong command)
		{
			PeerMessage m = new PeerMessage(h);
			m.Command = command;
			m.Message = message;
			XmlSerializer xs = new XmlSerializer(typeof(PeerMessage));
			StringWriter sr = new StringWriter();
			xs.Serialize(sr, m);
			return sr.GetStringBuilder().ToString();
		}
		#endregion

		#region IPMsg Messages
		internal static byte[] GetIPMsgNoOp(Host self)
		{
			return FormatIpMessage(self, null, Command.IPMSG_NOOPERATION);
		}

		internal static byte[] GetIPMsgPresenceMessage(Host self)
		{
			return FormatIpMessage(self, self.PreferredName, Command.IPMSG_BR_ENTRY | Command.IPMSG_FILEATTACHOPT);
		}

		internal static byte[] GetIPAbsenceMessage(Host self)
		{
			return FormatIpMessage(self, self.PreferredName, Command.IPMSG_BR_EXIT | Command.IPMSG_FILEATTACHOPT);
		}

		internal static byte[] FormatIpMessage(Host self, string message, bool seal)
		{
			ulong cmd = Command.IPMSG_SENDMSG | Command.IPMSG_SENDCHECKOPT;
			if(seal)
			{
				cmd |= Command.IPMSG_SECRETOPT;
			}

			return FormatIpMessage(self, message, cmd);
		}

		internal static byte[] FormatIpFileSendInitMessage(Host self, Hashtable files, ref IpMessage m)
		{
			string nullChar = '\0'.ToString();
			string alert = '\a'.ToString();
			string additionalSection = string.Empty;
			if(files != null && files.Values.Count > 0)
			{
				additionalSection += nullChar;

				foreach(int id in files.Keys)
				{					
					additionalSection += id + ":";
					additionalSection += (files[id] as SendFileInfo).Name + ":";
					additionalSection += Convert.ToString((files[id] as SendFileInfo).Size, 16) + ":";
					additionalSection += Convert.ToString((files[id] as SendFileInfo).TimeModified, 16) + ":";
					additionalSection += (files[id] as SendFileInfo).Attribute + ":";
					additionalSection += alert;
				}

				additionalSection += (nullChar);
			}

			m = FormatIpMessageAsObject(self, additionalSection, Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT | Command.IPMSG_SENDCHECKOPT);
			logger.Debug("Sending file message: " + m.ToString().Replace('\0', ' '));
			return FormatIpMessage(self, additionalSection, Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT | Command.IPMSG_SENDCHECKOPT);
		}

		internal static byte[] FormatIpFileReceiveMessage(Host self, uint originalRequestPacket, SendFileInfo file)
		{
			string additionalSection = Convert.ToString(originalRequestPacket, 16) + ":" + Convert.ToString(file.ID, 16) + ":0";
			return FormatIpMessage(self, additionalSection, Command.IPMSG_GETFILEDATA);
		}

		internal static byte[] FormatIpMessage(Host self, string message, ulong command)
		{
			byte[] msg = System.Text.Encoding.ASCII.GetBytes(FormatIpMessageAsObject(self, message, command).ToString());
			return msg;
		}

		internal static IpMessage FormatIpMessageAsObject(Host self, string message, ulong command)
		{
			IpMessage m = new IpMessage(self);
			m.Version = "1";
			m.AdditionalSection = message;
			m.Packet = (ulong)PacketCounter.Packet;
			m.Command = command;
			logger.Debug("Prepared IP message: " + m.ToString().Replace('\0', ' '));

			return m;
		}	
		#endregion
	}
}
