using System;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for IpMessageLoader.
	/// </summary>
	public sealed class IpMessageLoader
	{
		private IpMessageLoader()
		{
		}

		public static IpMessage Load(string msg)
		{
			IpMessage retVal = null;
			string[] messageComponents = msg.Split(new char[]{':'}, 6);
			if(messageComponents != null && messageComponents.Length > 0)
			{
				retVal = new IpMessage();
				retVal.Version = messageComponents[0].Replace("\0", string.Empty);
				retVal.Packet = ulong.Parse(messageComponents[1]);
				retVal.Sender = messageComponents[2].Replace("\0", string.Empty);
				retVal.SenderHost = messageComponents[3].Replace("\0", string.Empty);
				retVal.Command = ulong.Parse(messageComponents[4]);

				if((retVal.Command & (Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT)) == (Command.IPMSG_SENDMSG | Command.IPMSG_FILEATTACHOPT))
				{
					retVal.AdditionalSection = messageComponents[5];
				}
				else
				{
					retVal.AdditionalSection = messageComponents[5].Replace("\0", string.Empty);
				}
			}

			return retVal;
		}
	}
}
