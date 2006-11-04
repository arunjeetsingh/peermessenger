using System;

namespace PeerMessenger
{
	public interface ISubscriber
	{
		void GetMessage(string sender, string message);
		void GetClient(Host h);
		void DeleteClient(string sender);
		Host IsClientKnown(string sender);
	}
}
