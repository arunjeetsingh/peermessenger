using System;
using System.IO;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for SendFileInfo.
	/// </summary>
	public class SendFileInfo
	{
		private string _FullName;
		private string _Name;
		private long _Size;
		private long _TimeModified;
		private int _Attribute;
		private int _ID;
		private uint _IncomingPacket;

		public SendFileInfo(string fullName)
		{
			_FullName = fullName;
			Name = Path.GetFileName(FullName);
			FileInfo f = new FileInfo(FullName);
			Size = f.Length;
			TimeModified = f.LastWriteTime.ToFileTime();
			Attribute = 1;
		}

		public SendFileInfo()
		{
		}

		public string FullName
		{
			get
			{
				return _FullName;
			}
			set
			{
				_FullName = value;
			}
		}

		public string Name
		{
			get
			{
				return _Name;
			}
			set
			{
				_Name = value;
			}
		}

		public long Size
		{
			get
			{
				return _Size;
			}
			set
			{
				_Size = value;
			}
		}

		public long TimeModified
		{
			get
			{
				return _TimeModified;
			}
			set
			{
				_TimeModified = value;
			}
		}

		public int Attribute
		{
			get
			{
				return _Attribute;
			}
			set
			{
				_Attribute = value;
			}
		}

		public int ID
		{
			get
			{
				return _ID;
			}
			set
			{
				_ID = value;
			}
		}

		public uint IncomingPacket
		{
			get
			{
				return _IncomingPacket;
			}
			set
			{
				_IncomingPacket = value;
			}
		}
	}
}
