using System;

namespace PeerMessenger
{
	public interface ISubscriber
	{
		void GetMessage(string sender, string message);
		void GetStatusMessage(string sender, string message);
		void GetFiles(string sender, SendFileInfo[] files, uint packet);
		void GetClient(Host h);
		void DeleteClient(string sender);
		Host IsClientKnown(string sender);
	}
}
