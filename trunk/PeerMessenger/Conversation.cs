using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using log4net;
using System.Text;
using System.IO;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for Conversation.
	/// </summary>
	public class Conversation : System.Windows.Forms.Form, ISubscriber
	{
		private System.Windows.Forms.RichTextBox rtbSend;
		private System.Windows.Forms.Button btnSend;

		private Host h, ivSelf;
		private UdpBroadcastManager ivUdpManager;
		private ISubscriber ivMainWindow;

		private LinkedList currentKeyUp = null;
		private LinkedList outgoingHead;
		private LinkedList outgoingTail;
		private System.Windows.Forms.RichTextBox rtbConversation;
		
		private ILog logger = LogManager.GetLogger(typeof(Conversation));
		private System.Windows.Forms.SaveFileDialog sfReceive;
		private ILog messageLogger = LogManager.GetLogger("MessageLogger");
		byte[] contents;

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
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(Conversation));
			this.rtbSend = new System.Windows.Forms.RichTextBox();
			this.btnSend = new System.Windows.Forms.Button();
			this.rtbConversation = new System.Windows.Forms.RichTextBox();
			this.sfReceive = new System.Windows.Forms.SaveFileDialog();
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
			// sfReceive
			// 
			this.sfReceive.AddExtension = false;
			this.sfReceive.Filter = "All files|*.*";
			this.sfReceive.RestoreDirectory = true;
			// 
			// Conversation
			// 
			this.AcceptButton = this.btnSend;
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(376, 269);
			this.Controls.Add(this.btnSend);
			this.Controls.Add(this.rtbSend);
			this.Controls.Add(this.rtbConversation);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "Conversation";
			this.Text = "Conversation";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.Conversation_Closing);
			this.Load += new System.EventHandler(this.Conversation_Load);
			this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Conversation_DragDrop);
			this.Activated += new System.EventHandler(this.Conversation_Activated);
			this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Conversation_DragEnter);
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
				ivUdpManager.Send(h.HostName, m);
			}
			
			if(rtbConversation.Text.Length > 0)
			{
				rtbConversation.AppendText("\n");
			}

			string displayMessage = ivSelf.PreferredName + ": " + m;
			messageLogger.Info("To   " + h.PreferredName + ": " + m);
			showMessage(displayMessage);
			
		}

		public void showMessage(string message)
		{
			rtbConversation.AppendText(message);
			rtbConversation.Focus();
			rtbConversation.Select(rtbConversation.TextLength, 0);
			rtbConversation.ScrollToCaret();
			rtbSend.Focus();
		}

		#region ISubscriber Members
		public void GetStatusMessage(string sender, string message)
		{
			GetMessage(sender, message, true);
		}

		public void GetMessage(string sender, string message)
		{
			GetMessage(sender, message, false);
		}

		public void ReadFile(IAsyncResult result)
		{
			object[] state = result.AsyncState as object[];
			TcpClient client = state[0] as TcpClient;
			SendFileInfo file = state[1] as SendFileInfo;
			FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
			string p = Encoding.ASCII.GetString(contents);
			fs.Write(contents, 0, contents.Length);
			fs.Close();

			client.Close();
		}

		public void GetFiles(string sender, SendFileInfo[] files, uint packet)
		{
			try
			{
				if(h.Sender == sender)
				{
					if(files != null && files.Length > 0)
					{
						FlashWindowHelper.FlashWindowEx(this.Handle);

						foreach(SendFileInfo file in files)
						{
							sfReceive.FileName = file.Name;
							if(sfReceive.ShowDialog() == DialogResult.OK)
							{
								TcpClient client = new TcpClient(h.HostName, 2425);
								client.NoDelay = true;
								NetworkStream stream = client.GetStream();
								byte[] msg = MessageFormatter.FormatIpFileReceiveMessage(ivSelf, packet, file);
								stream.Write(msg, 0, msg.Length);
								contents = new byte[file.Size];
								file.FullName = sfReceive.FileName;
								stream.BeginRead(contents, 0, contents.Length, new AsyncCallback(ReadFile), new object[] {client, file});
							}
						}
					}					
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public void GetMessage(string sender, string message, bool status)
		{
			try
			{
				if(h.Sender == sender)
				{
					if(rtbConversation.Text.Length > 0)
					{
						rtbConversation.AppendText("\n");
					}

					string displayMessage = null;
					if(status)
					{
						displayMessage = message;
						messageLogger.Info(displayMessage);						
					}
					else
					{
						displayMessage = h.PreferredName + ": " + message;
						messageLogger.Info("From " + displayMessage);						
					}

					rtbConversation.AppendText(displayMessage);

					rtbConversation.Focus();
					rtbConversation.Select(rtbConversation.TextLength, 0);
					rtbConversation.ScrollToCaret();
					rtbSend.Focus();

					FlashWindowHelper.FlashWindowEx(this.Handle);
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
				rtbSend.Focus();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void Conversation_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
		{			
			string[] files = e.Data.GetData(DataFormats.FileDrop) as string[];
			if(files != null && files.Length > 0)
			{
				if(rtbConversation.Text.Length > 0)
				{
					rtbConversation.AppendText("\n");
				}

				logger.Debug("Files dropped: ");
				SendFileInfo[] filesInfo = new SendFileInfo[files.Length];				
				showMessage("Sending file(s):");
				for(int i = 0; i < files.Length; i++)
				{
					logger.Debug(files[i]);
					showMessage("\n" + files[i]);
					filesInfo[i] = new SendFileInfo(files[i]);
				}
				
				ivUdpManager.SendIPFile(h.HostName, filesInfo);
			}
		}

		private void Conversation_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(IpSession && e.Data.GetDataPresent(DataFormats.FileDrop) && ((string[])e.Data.GetData(DataFormats.FileDrop)).Length == 1)
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}
	}
}
