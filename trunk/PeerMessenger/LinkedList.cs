using System;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for LinkedList.
	/// </summary>
	public class LinkedList
	{
		LinkedList ivNext;
		LinkedList ivPrevious;
		string ivMessage;

		public LinkedList(LinkedList previous, string message) : this()
		{
			ivMessage = message;
			ivPrevious = previous;
		}

		public LinkedList()
		{
		}

		public LinkedList Add(string message)
		{
			LinkedList tail = new LinkedList(this, message);
			return tail;
		}

		public string Message
		{
			get
			{
				return ivMessage;
			}
			set
			{
				ivMessage = value;
			}
		}

		public LinkedList Next
		{
			get
			{
				return ivNext;
			}
			set
			{
				ivNext = value;
			}
		}

		public LinkedList Previous
		{
			get
			{
				return ivPrevious;
			}
			set
			{
				ivPrevious = value;
			}
		}

		public int Pos
		{
			get
			{
				if(Previous != null)
				{
					return Previous.Pos + 1;
				}
				else
				{
					return 0;
				}
			}
		}
	}
}
