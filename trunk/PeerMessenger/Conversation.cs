using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using log4net;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for Conversation.
	/// </summary>
	public class Conversation : System.Windows.Forms.Form, ISubscriber
	{
		private System.Windows.Forms.RichTextBox rtbSend;
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button btnSend;

		private Host h, ivSelf;
		private UdpBroadcastManager ivUdpManager;
		private ISubscriber ivMainWindow;

		private LinkedList currentKeyUp = null;
		private LinkedList outgoingHead;
		private LinkedList outgoingTail;
		private System.Windows.Forms.RichTextBox rtbConversation;
		
		private ILog logger = LogManager.GetLogger(typeof(Conversation));
		private System.Windows.Forms.Timer tmrBlink;
		private ILog messageLogger = LogManager.GetLogger("MessageLogger");

		public Conversation(ISubscriber mainWindow, Host host, Host self, UdpBroadcastManager udpManager) : this()
		{
			h = host;
			ivSelf = self;
			ivUdpManager = udpManager;
			ivMainWindow = mainWindow;
		}

		private Conversation()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}
		
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.rtbSend = new System.Windows.Forms.RichTextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.rtbConversation = new System.Windows.Forms.RichTextBox();
			this.tmrBlink = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// rtbSend
			// 
			this.rtbSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbSend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.rtbSend.Location = new System.Drawing.Point(8, 216);
			this.rtbSend.Name = "rtbSend";
			this.rtbSend.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.rtbSend.Size = new System.Drawing.Size(320, 48);
			this.rtbSend.TabIndex = 1;
			this.rtbSend.Text = "";
			this.rtbSend.KeyUp += new System.Windows.Forms.KeyEventHandler(this.rtbSend_KeyUp);
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.BackColor = System.Drawing.SystemColors.Control;
			this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnSend.Location = new System.Drawing.Point(332, 216);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(40, 48);
			this.btnSend.TabIndex = 2;
			this.btnSend.Text = "Send";
			this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
			// 
			// rtbConversation
			// 
			this.rtbConversation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbConversation.AutoSize = true;
			this.rtbConversation.AutoWordSelection = true;
			this.rtbConversation.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.rtbConversation.Location = new System.Drawing.Point(8, 8);
			this.rtbConversation.MaxLength = 50;
			this.rtbConversation.Name = "rtbConversation";
			this.rtbConversation.ReadOnly = true;
			this.rtbConversation.Size = new System.Drawing.Size(360, 200);
			this.rtbConversation.TabIndex = 0;
			this.rtbConversation.Text = "";
			// 
			// tmrBlink
			// 
			this.tmrBlink.Interval = 1000;
			this.tmrBlink.Tick += new System.EventHandler(this.tmrBlink_Tick);
			// 
			// Conversation
			// 
			this.AcceptButton = this.btnSend;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(376, 269);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.rtbSend);
			this.Controls.Add(this.rtbConversation);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Conversation";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Conversation";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Conversation_Closing);
			this.Load += new System.EventHandler(this.Conversation_Load);
			this.Activated += new System.EventHandler(this.Conversation_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnSend_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(rtbSend.Text.Trim().Length > 0)
				{
					sendMessage(rtbSend.Text);
					AddOutgoing(rtbSend.Text);
					rtbSend.Text = string.Empty;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public bool IpSession
		{
			get
			{
				return h.IP;
			}
		}

		private void AddOutgoing(string message)
		{
			if(outgoingHead == null)
			{
				outgoingTail = outgoingHead = new LinkedList(null, message);
			}
			else
			{
				outgoingTail = outgoingTail.Add(message);
			}

			if(outgoingTail.Pos > 50)
			{
				outgoingHead = outgoingHead.Next;
			}
		}

		private void DecrementKeyUp()
		{
			if(currentKeyUp == null)
			{
				currentKeyUp = outgoingTail;
			}
			else
			{
				currentKeyUp = currentKeyUp.Previous;
			}
		}

		public void sendMessage(string m)
		{
			if(IpSession)
			{
				ivUdpManager.SendIP(h.HostName, m);
			}
			else
			{
				TcpClient client = new TcpClient();
				client.NoDelay = true;
				client.Connect(h.HostName, 3089);
				NetworkStream stream = client.GetStream();			
				byte[] msg = MessageFormatter.FormatMessage(ivSelf, m);
				stream.Write(msg, 0, msg.Length);
				stream.Close();
				client.Close();
			}
			
			if(rtbConversation.Text.Length > 0)
			{
				rtbConversation.AppendText("\n");
			}

			string displayMessage = ivSelf.PreferredName + ": " + m;
			messageLogger.Info("To   " + h.PreferredName + ": " + m);
			rtbConversation.AppendText(displayMessage);
			rtbConversation.Focus();
			rtbConversation.Select(rtbConversation.TextLength, 0);
			rtbConversation.ScrollToCaret();
			rtbSend.Focus();			
		}

		#region ISubscriber Members
		public void GetMessage(string sender, string message)
		{
			try
			{
				if(h.Sender == sender)
				{
					if(rtbConversation.Text.Length > 0)
					{
						rtbConversation.AppendText("\n");
					}

					string displayMessage = h.PreferredName + ": " + message;
					messageLogger.Info("From " + displayMessage);
					rtbConversation.AppendText(displayMessage);
					rtbConversation.Focus();
					rtbConversation.Select(rtbConversation.TextLength, 0);
					rtbConversation.ScrollToCaret();
					rtbSend.Focus();

					if(this.Focused == false)
					{
						tmrBlink.Start();
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public void GetClient(Host host)
		{
			try
			{
				if(host.Sender == h.Sender)
				{
					h = host;
					btnSend.Enabled = true;
					if(rtbConversation.Text.Length > 0)
					{
						rtbConversation.AppendText("\n");
					}

					string displayMessage = "*** " + h.PreferredName + " is now online.";
					messageLogger.Info(displayMessage);
					rtbConversation.AppendText(displayMessage);
					rtbConversation.Focus();
					rtbConversation.Select(rtbConversation.TextLength, 0);
					rtbConversation.ScrollToCaret();
					rtbSend.Focus();
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public void DeleteClient(string sender)
		{
			try
			{
				if(sender == h.Sender)
				{
					btnSend.Enabled = false;
					if(rtbConversation.Text.Length > 0)
					{
						rtbConversation.AppendText("\n");
					}

					string displayMessage = "*** " + h.PreferredName + " has gone offline.";
					messageLogger.Info(displayMessage);
					rtbConversation.AppendText(displayMessage);
					rtbConversation.Focus();
					rtbConversation.Select(rtbConversation.TextLength, 0);
					rtbConversation.ScrollToCaret();
					rtbSend.Focus();
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public Host IsClientKnown(string sender)
		{
			return ivMainWindow.IsClientKnown(sender);
		}

		#endregion

		private void Conversation_Load(object sender, System.EventArgs e)
		{
			try
			{
				Text = h.PreferredName;
				rtbSend.Focus();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void Conversation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				(ivMainWindow as MainForm).Conversations.Remove(h.Sender);
				this.Dispose();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void rtbSend_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				if(e.KeyCode == Keys.Up)
				{
					DecrementKeyUp();
					if(currentKeyUp != null)
					{
						rtbSend.Text = currentKeyUp.Message;
					}
				}
				else
				{
					currentKeyUp = null;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void Conversation_Activated(object sender, System.EventArgs e)
		{
			try
			{
				tmrBlink.Stop();
				rtbSend.Focus();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void tmrBlink_Tick(object sender, System.EventArgs e)
		{
		}
	}
}
