using System;
using System.Windows.Forms;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for Host.
	/// </summary>
	public class Host : ListViewItem
	{
		private string _PreferredName;
		private string _Sender;
		private string _HostName;
		private bool _IP;
		private SendFileInfo _ProfilePicture;

		public Host(string sender, string preferredName, string hostName) : this(sender, preferredName, hostName, false)
		{			
		}

		public Host(string sender, string preferredName, string hostName, bool ip)
		{			
			_PreferredName = preferredName;
			_HostName = hostName;
			_IP = ip;
			_Sender = sender;

			Text = PreferredName;
			ImageIndex = 0;
		}

		public string Sender
		{
			get
			{
				return _Sender;
			}
			set
			{
				_Sender = value;
			}
		}

		public string PreferredName
		{
			get
			{
				return _PreferredName;
			}
			set
			{
				_PreferredName = value;
			}
		}

		public string HostName
		{
			get
			{
				return _HostName;
			}
			set
			{
				_HostName = value;
			}
		}

		public bool IP
		{
			get
			{
				return _IP;
			}
			set
			{
				_IP = value;
			}
		}

		public SendFileInfo ProfilePicture
		{
			get
			{
				return _ProfilePicture;
			}
			set
			{
				_ProfilePicture = value;
			}
		}

		public override string ToString()
		{
			string retVal = PreferredName.Replace("\0", " ");
			return retVal;
		}

		public override bool Equals(object obj)
		{
			Host h = obj as Host;
			if(h != null)
			{
				return Sender == h.Sender;
			}
			
			return false;
		}

		public override int GetHashCode()
		{
			return Sender.GetHashCode();
		}
	}
}
