using System;

namespace PeerMessenger
{
	public delegate void SendMessage(string sender, string message);
	public delegate void AddClient(Host host);
	public delegate void RemoveClient(string sender);
}
