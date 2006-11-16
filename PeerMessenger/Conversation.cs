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
using System.Threading;
using System.Diagnostics;

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
		private UdpManager ivUdpManager;
		private ISubscriber ivMainWindow;

		private LinkedList currentKeyUp = null;
		private LinkedList outgoingHead;
		private LinkedList outgoingTail;
		private System.Windows.Forms.RichTextBox rtbConversation;
		
		private ILog logger = LogManager.GetLogger(typeof(Conversation));
		private System.Windows.Forms.SaveFileDialog sfReceive;
		private ILog messageLogger = LogManager.GetLogger("MessageLogger");
		private System.Windows.Forms.ContextMenu cmConversation;
		private System.Windows.Forms.MenuItem mnuCopy;
		private System.Windows.Forms.ContextMenu cmSend;
		private System.Windows.Forms.MenuItem mnuCut;
		private System.Windows.Forms.MenuItem mnuCopy2;
		private System.Windows.Forms.MenuItem mnuPaste;
		private System.Windows.Forms.MenuItem mnuDelete;
		private System.Windows.Forms.MenuItem mnuSelectAll2;
		private System.Windows.Forms.MenuItem mnuSelectAll;
		private System.Windows.Forms.Button btnPop;
		private System.Windows.Forms.ContextMenu cmPop;
		private System.Windows.Forms.MenuItem mnuSendSealed;
		private System.Windows.Forms.MenuItem mnuSend;

		public Conversation(ISubscriber mainWindow, Host host, Host self, UdpManager udpManager) : this()
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
			this.cmSend = new System.Windows.Forms.ContextMenu();
			this.mnuCut = new System.Windows.Forms.MenuItem();
			this.mnuCopy2 = new System.Windows.Forms.MenuItem();
			this.mnuPaste = new System.Windows.Forms.MenuItem();
			this.mnuDelete = new System.Windows.Forms.MenuItem();
			this.mnuSelectAll2 = new System.Windows.Forms.MenuItem();
			this.btnSend = new System.Windows.Forms.Button();
			this.rtbConversation = new System.Windows.Forms.RichTextBox();
			this.cmConversation = new System.Windows.Forms.ContextMenu();
			this.mnuCopy = new System.Windows.Forms.MenuItem();
			this.mnuSelectAll = new System.Windows.Forms.MenuItem();
			this.sfReceive = new System.Windows.Forms.SaveFileDialog();
			this.btnPop = new System.Windows.Forms.Button();
			this.cmPop = new System.Windows.Forms.ContextMenu();
			this.mnuSendSealed = new System.Windows.Forms.MenuItem();
			this.mnuSend = new System.Windows.Forms.MenuItem();
			this.SuspendLayout();
			// 
			// rtbSend
			// 
			this.rtbSend.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbSend.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.rtbSend.ContextMenu = this.cmSend;
			this.rtbSend.Location = new System.Drawing.Point(8, 216);
			this.rtbSend.Name = "rtbSend";
			this.rtbSend.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
			this.rtbSend.Size = new System.Drawing.Size(320, 48);
			this.rtbSend.TabIndex = 1;
			this.rtbSend.Text = "";
			this.rtbSend.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbSend_LinkClicked);
			this.rtbSend.KeyUp += new System.Windows.Forms.KeyEventHandler(this.rtbSend_KeyUp);
			// 
			// cmSend
			// 
			this.cmSend.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				   this.mnuCut,
																				   this.mnuCopy2,
																				   this.mnuPaste,
																				   this.mnuDelete,
																				   this.mnuSelectAll2});
			this.cmSend.Popup += new System.EventHandler(this.cmSend_Popup);
			// 
			// mnuCut
			// 
			this.mnuCut.Enabled = false;
			this.mnuCut.Index = 0;
			this.mnuCut.Text = "Cu&t";
			this.mnuCut.Click += new System.EventHandler(this.mnuCut_Click);
			// 
			// mnuCopy2
			// 
			this.mnuCopy2.Enabled = false;
			this.mnuCopy2.Index = 1;
			this.mnuCopy2.Text = "&Copy";
			this.mnuCopy2.Click += new System.EventHandler(this.mnuCopy2_Click);
			// 
			// mnuPaste
			// 
			this.mnuPaste.Enabled = false;
			this.mnuPaste.Index = 2;
			this.mnuPaste.Text = "&Paste";
			this.mnuPaste.Click += new System.EventHandler(this.mnuPaste_Click);
			// 
			// mnuDelete
			// 
			this.mnuDelete.Enabled = false;
			this.mnuDelete.Index = 3;
			this.mnuDelete.Text = "&Delete";
			this.mnuDelete.Click += new System.EventHandler(this.mnuDelete_Click);
			// 
			// mnuSelectAll2
			// 
			this.mnuSelectAll2.Index = 4;
			this.mnuSelectAll2.Text = "&Select All";
			// 
			// btnSend
			// 
			this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSend.BackColor = System.Drawing.SystemColors.Control;
			this.btnSend.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnSend.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnSend.Location = new System.Drawing.Point(328, 216);
			this.btnSend.Name = "btnSend";
			this.btnSend.Size = new System.Drawing.Size(48, 48);
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
			this.rtbConversation.ContextMenu = this.cmConversation;
			this.rtbConversation.Location = new System.Drawing.Point(8, 8);
			this.rtbConversation.MaxLength = 50;
			this.rtbConversation.Name = "rtbConversation";
			this.rtbConversation.ReadOnly = true;
			this.rtbConversation.Size = new System.Drawing.Size(384, 200);
			this.rtbConversation.TabIndex = 0;
			this.rtbConversation.Text = "";
			this.rtbConversation.LinkClicked += new System.Windows.Forms.LinkClickedEventHandler(this.rtbConversation_LinkClicked);
			// 
			// cmConversation
			// 
			this.cmConversation.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						   this.mnuCopy,
																						   this.mnuSelectAll});
			this.cmConversation.Popup += new System.EventHandler(this.cmConversation_Popup);
			// 
			// mnuCopy
			// 
			this.mnuCopy.Enabled = false;
			this.mnuCopy.Index = 0;
			this.mnuCopy.Text = "&Copy";
			this.mnuCopy.Click += new System.EventHandler(this.mnuCopy_Click);
			// 
			// mnuSelectAll
			// 
			this.mnuSelectAll.Index = 1;
			this.mnuSelectAll.Text = "&Select All";
			this.mnuSelectAll.Click += new System.EventHandler(this.mnuSelectAll_Click);
			// 
			// sfReceive
			// 
			this.sfReceive.AddExtension = false;
			this.sfReceive.Filter = "All files|*.*";
			this.sfReceive.RestoreDirectory = true;
			// 
			// btnPop
			// 
			this.btnPop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnPop.BackColor = System.Drawing.SystemColors.Control;
			this.btnPop.ContextMenu = this.cmPop;
			this.btnPop.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnPop.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.btnPop.Location = new System.Drawing.Point(376, 216);
			this.btnPop.Name = "btnPop";
			this.btnPop.Size = new System.Drawing.Size(16, 48);
			this.btnPop.TabIndex = 3;
			this.btnPop.Text = "▼";
			this.btnPop.Click += new System.EventHandler(this.btnPop_Click);
			// 
			// cmPop
			// 
			this.cmPop.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																				  this.mnuSendSealed,
																				  this.mnuSend});
			this.cmPop.Popup += new System.EventHandler(this.cmPop_Popup);
			// 
			// mnuSendSealed
			// 
			this.mnuSendSealed.Enabled = false;
			this.mnuSendSealed.Index = 0;
			this.mnuSendSealed.Text = "Send Sealed";
			this.mnuSendSealed.Click += new System.EventHandler(this.mnuSendSealed_Click);
			// 
			// mnuSend
			// 
			this.mnuSend.Index = 1;
			this.mnuSend.Text = "Send Unsealed";
			this.mnuSend.Click += new System.EventHandler(this.mnuSend_Click);
			// 
			// Conversation
			// 
			this.AcceptButton = this.btnSend;
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(400, 269);
			this.Controls.Add(this.btnPop);
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
					sendMessage(rtbSend.Text, ConfigurationManager.Seal);
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
			sendMessage(m, false);
		}

		public void sendMessage(string m, bool seal)
		{
			if(IpSession)
			{
				ivUdpManager.SendIP(h.HostName, m, seal);
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

		public void GetFiles(string sender, SendFileInfo[] files, uint packet)
		{
			try
			{
				if(h.Sender == sender)
				{
					if(files != null && files.Length > 0)
					{
						FlashWindowHelper.FlashWindowEx(this.Handle);

						for(int i = 0; i < files.Length; i++)
						{
							SendFileInfo file = files[i];
							logger.Debug("Requesting file " + file.Name);
							sfReceive.FileName = file.Name;
							if(sfReceive.ShowDialog() == DialogResult.OK)
							{
								file.FullName = sfReceive.FileName;
								FileTransferDialog transferDialog = new FileTransferDialog(this, ivSelf, h, packet, file);
								if(transferDialog.ShowDialog(this) == DialogResult.OK)
								{
									showMessage("\n*** " + file.Name + " received.");
								}
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
				Hashtable filesInfo = new Hashtable();
				showMessage("*** Sending file(s):");
				for(int i = 0; i < files.Length; i++)
				{
					logger.Debug(files[i]);					
					filesInfo[i] = new SendFileInfo(files[i]);
					showMessage("\n*** " + (filesInfo[i] as SendFileInfo).Name);
					(filesInfo[i] as SendFileInfo).ID = i;
				}
				
				ivUdpManager.SendIPFile(h.HostName, filesInfo);
			}
		}

		private void Conversation_DragEnter(object sender, System.Windows.Forms.DragEventArgs e)
		{
			if(IpSession && e.Data.GetDataPresent(DataFormats.FileDrop))
			{
				e.Effect = DragDropEffects.Copy;
			}
			else
			{
				e.Effect = DragDropEffects.None;
			}
		}		

		private void cmConversation_Popup(object sender, System.EventArgs e)
		{
			if(rtbConversation.SelectedText.Length > 0)
			{
				mnuCopy.Enabled = true;
			}
			else
			{
				mnuCopy.Enabled = false;
			}
		}

		private void cmSend_Popup(object sender, System.EventArgs e)
		{
			if(rtbSend.SelectedText.Length > 0)
			{
				mnuCut.Enabled = true;
				mnuCopy2.Enabled = true;				
				mnuDelete.Enabled = true;
			}
			else
			{
				mnuCut.Enabled = false;
				mnuCopy2.Enabled = false;
				mnuDelete.Enabled = false;
			}

			if(Clipboard.GetDataObject().GetDataPresent(DataFormats.Text))
			{
				mnuPaste.Enabled = true;
			}
			else
			{
				mnuPaste.Enabled = false;
			}
		}

		private void mnuCopy_Click(object sender, System.EventArgs e)
		{
			rtbConversation.Copy();
		}

		private void mnuSelectAll_Click(object sender, System.EventArgs e)
		{
			rtbConversation.SelectAll();
		}

		private void mnuCut_Click(object sender, System.EventArgs e)
		{
			rtbSend.Cut();
		}

		private void mnuCopy2_Click(object sender, System.EventArgs e)
		{
			rtbSend.Copy();
		}

		private void mnuPaste_Click(object sender, System.EventArgs e)
		{
			rtbSend.Paste();
		}

		private void mnuDelete_Click(object sender, System.EventArgs e)
		{
			rtbSend.Text = rtbSend.Text.Remove(rtbSend.SelectionStart, rtbSend.SelectionLength);
		}

		private void rtbConversation_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		private void rtbSend_LinkClicked(object sender, System.Windows.Forms.LinkClickedEventArgs e)
		{
			Process.Start(e.LinkText);
		}

		private void btnPop_Click(object sender, System.EventArgs e)
		{			
			btnPop.ContextMenu.Show(btnPop, new Point(0, btnPop.Height));
		}

		private void mnuSend_Click(object sender, System.EventArgs e)
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

		private void mnuSendSealed_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(rtbSend.Text.Trim().Length > 0)
				{
					sendMessage(rtbSend.Text, true);
					AddOutgoing(rtbSend.Text);
					rtbSend.Text = string.Empty;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void cmPop_Popup(object sender, System.EventArgs e)
		{
			mnuSendSealed.Enabled = IpSession;
		}
	}
}
