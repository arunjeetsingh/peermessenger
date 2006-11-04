using System;
using System.Xml;
using log4net;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for MessageFormatter.
	/// </summary>
	internal sealed class MessageFormatter
	{
		public static readonly Guid Identifier = new Guid("{B8BFAADC-B235-4875-BE7C-6E6B7A5A04AA}");
		private ILog logger = LogManager.GetLogger(typeof(MessageFormatter));		

		private MessageFormatter()
		{
		}

		#region PeerMessenger messages
		internal static byte[] GetPresenceMessage(Host h)
		{
			return GetPresenceMessage(h, true);
		}

		internal static byte[] GetPresenceMessage(Host h, bool attachId)
		{
			return GetPresenceMessage(h, attachId, false);
		}

		internal static byte[] GetPresenceMessage(Host h, bool attachId, bool forceConfirmation)
		{
			ulong command = Command.Presence;
			if(forceConfirmation)
			{
				command |= Command.ForceConfirmation;
			}

			return GetPresenceMessage(h, attachId, command);
		}

		internal static byte[] GetPresenceMessage(Host h, bool attachId, ulong command)
		{
			string message = string.Empty;
			if(attachId)
			{
				message += Identifier.ToString();
			}

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
		/// as a byte array suitable for sending over a tcp connection.
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

		internal static byte[] FormatIpMessage(Host self, string message)
		{
			return FormatIpMessage(self, message, Command.IPMSG_SENDMSG | Command.IPMSG_SENDCHECKOPT);
		}

		internal static byte[] FormatIpMessage(Host self, string message, ulong command)
		{
			IpMessage m = new IpMessage(self);
			m.Version = "1";
			m.AdditionalSection = message;
			m.Packet = (ulong)PacketCounter.Packet;
			m.Command = command;
			byte[] msg = System.Text.Encoding.ASCII.GetBytes(m.ToString());
			return msg;
		}	
		#endregion
	}
}
