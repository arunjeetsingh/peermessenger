using System;

namespace PeerMessenger
{	
	public sealed class Command
	{
		private Command()
		{
		}

		#region PeerMessenger Commands
		public const ulong Presence =		     0x00000001;
		public const ulong Absence  =            0x00000002;
		public const ulong Message  =            0x00000004;
		public const ulong ForceConfirmation =   0x00000008;
		public const ulong AcknowledgePresence = 0x00000010;
		public const ulong AcknowledgeMessage  = 0x00000020;
		public const ulong RequestMessageAck   = 0x00000040;
		#endregion

		#region IPMsg Commands
		public const ulong IPMSG_NOOPERATION = 0x00000000;
		public const ulong IPMSG_BR_ENTRY = 0x00000001;
		public const ulong IPMSG_BR_EXIT = 0x00000002;
		public const ulong IPMSG_ANSENTRY = 0x00000003;
		public const ulong IPMSG_BR_ABSENCE = 0x00000004;
		public const ulong IPMSG_SENDMSG = 0x00000020;
		public const ulong IPMSG_RECVMSG = 0x00000021;
		public const ulong IPMSG_READMSG = 0x00000030;
		public const ulong IPMSG_ANSREADMSG = 0x00000032;
		public const ulong IPMSG_FILEATTACHOPT = 0x00200000;
		public const ulong IPMSG_SENDCHECKOPT = 0x00000100;
		public const ulong IPMSG_RELEASEFILES = 0x00000061;
		public const ulong IPMSG_GETFILEDATA  = 0x00000060;
		#endregion
	}
}
