using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Configuration;
using System.Xml;
using System.Diagnostics;
using log4net;
using System.IO;

namespace PeerMessenger
{
	/// <summary>
	/// Main form
	/// </summary>
	public class MainForm : System.Windows.Forms.Form, ISubscriber
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button btnExit;		
		private System.Windows.Forms.Button btnChat;

		private Thread fileListenerThread;
		private FileListener fileListener;
		private UdpManager udpManager;
		private Thread udpManagerThread, udpManagerThreadIp;
		private Hashtable hosts = new Hashtable();
		private Hashtable ivConversations = new Hashtable();
		private Host self;
		private System.Windows.Forms.ContextMenu cmnuMain;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.NotifyIcon niPeerMessenger;
		private System.Windows.Forms.Timer tmrBroadcast;
		private System.Windows.Forms.MenuItem mnuExit;
		private System.Windows.Forms.TextBox txtSearch;
		private System.Windows.Forms.ImageList ilIcons;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Timer tmrSearch;
		object syncLock = new object();
		private System.Windows.Forms.Button btnRefresh;
		private System.Windows.Forms.StatusBar sbMain;
		private System.Windows.Forms.MenuItem mnuViewLog;
		private System.Windows.Forms.ListView lvHosts;
		private System.Windows.Forms.ImageList ilHosts;
		private System.Windows.Forms.ColumnHeader clmText;
		private System.Windows.Forms.PictureBox pbProfile;
		private System.Windows.Forms.Label lblPreferredName;
		private System.Windows.Forms.Timer tmrProfile;

		private ILog logger = LogManager.GetLogger(typeof(MainForm));		

		public MainForm()
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
				if (components != null) 
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.btnExit = new System.Windows.Forms.Button();
			this.btnChat = new System.Windows.Forms.Button();
			this.cmnuMain = new System.Windows.Forms.ContextMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuViewLog = new System.Windows.Forms.MenuItem();
			this.mnuExit = new System.Windows.Forms.MenuItem();
			this.niPeerMessenger = new System.Windows.Forms.NotifyIcon(this.components);
			this.tmrBroadcast = new System.Windows.Forms.Timer(this.components);
			this.txtSearch = new System.Windows.Forms.TextBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.ilIcons = new System.Windows.Forms.ImageList(this.components);
			this.tmrSearch = new System.Windows.Forms.Timer(this.components);
			this.btnRefresh = new System.Windows.Forms.Button();
			this.sbMain = new System.Windows.Forms.StatusBar();
			this.lvHosts = new System.Windows.Forms.ListView();
			this.clmText = new System.Windows.Forms.ColumnHeader();
			this.ilHosts = new System.Windows.Forms.ImageList(this.components);
			this.pbProfile = new System.Windows.Forms.PictureBox();
			this.lblPreferredName = new System.Windows.Forms.Label();
			this.tmrProfile = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// btnExit
			// 
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.BackColor = System.Drawing.Color.Beige;
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnExit.Location = new System.Drawing.Point(168, 344);
			this.btnExit.Name = "btnExit";
			this.btnExit.Size = new System.Drawing.Size(56, 23);
			this.btnExit.TabIndex = 5;
			this.btnExit.Text = "E&xit";
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// btnChat
			// 
			this.btnChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnChat.BackColor = System.Drawing.Color.Beige;
			this.btnChat.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnChat.Enabled = false;
			this.btnChat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnChat.Location = new System.Drawing.Point(104, 344);
			this.btnChat.Name = "btnChat";
			this.btnChat.Size = new System.Drawing.Size(56, 23);
			this.btnChat.TabIndex = 4;
			this.btnChat.Text = "&Chat";
			this.btnChat.Click += new System.EventHandler(this.btnChat_Click);
			// 
			// cmnuMain
			// 
			this.cmnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					 this.menuItem1,
																					 this.mnuViewLog,
																					 this.mnuExit});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.Text = "&Options...";
			this.menuItem1.Click += new System.EventHandler(this.menuItem1_Click);
			// 
			// mnuViewLog
			// 
			this.mnuViewLog.Index = 1;
			this.mnuViewLog.Text = "&View Log";
			this.mnuViewLog.Click += new System.EventHandler(this.mnuViewLog_Click);
			// 
			// mnuExit
			// 
			this.mnuExit.Index = 2;
			this.mnuExit.Text = "E&xit";
			this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
			// 
			// niPeerMessenger
			// 
			this.niPeerMessenger.ContextMenu = this.cmnuMain;
			this.niPeerMessenger.Icon = ((System.Drawing.Icon)(resources.GetObject("niPeerMessenger.Icon")));
			this.niPeerMessenger.Text = "Peer Messenger";
			this.niPeerMessenger.DoubleClick += new System.EventHandler(this.niPeerMessenger_DoubleClick);
			// 
			// tmrBroadcast
			// 
			this.tmrBroadcast.Interval = 5000;
			this.tmrBroadcast.Tick += new System.EventHandler(this.tmrBroadcast_Tick);
			// 
			// txtSearch
			// 
			this.txtSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtSearch.BackColor = System.Drawing.Color.Beige;
			this.txtSearch.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.txtSearch.Location = new System.Drawing.Point(8, 64);
			this.txtSearch.Name = "txtSearch";
			this.txtSearch.Size = new System.Drawing.Size(200, 20);
			this.txtSearch.TabIndex = 6;
			this.txtSearch.Text = "";
			this.txtSearch.TextChanged += new System.EventHandler(this.txtSearch_TextChanged);
			this.txtSearch.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSearch_KeyUp);
			// 
			// btnSearch
			// 
			this.btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnSearch.BackColor = System.Drawing.Color.Ivory;
			this.btnSearch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSearch.ImageIndex = 0;
			this.btnSearch.ImageList = this.ilIcons;
			this.btnSearch.Location = new System.Drawing.Point(204, 64);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(20, 20);
			this.btnSearch.TabIndex = 7;
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// ilIcons
			// 
			this.ilIcons.ImageSize = new System.Drawing.Size(20, 20);
			this.ilIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilIcons.ImageStream")));
			this.ilIcons.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// tmrSearch
			// 
			this.tmrSearch.Interval = 1000;
			this.tmrSearch.Tick += new System.EventHandler(this.tmrSearch_Tick);
			// 
			// btnRefresh
			// 
			this.btnRefresh.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRefresh.BackColor = System.Drawing.Color.Beige;
			this.btnRefresh.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnRefresh.Location = new System.Drawing.Point(40, 344);
			this.btnRefresh.Name = "btnRefresh";
			this.btnRefresh.Size = new System.Drawing.Size(56, 23);
			this.btnRefresh.TabIndex = 8;
			this.btnRefresh.Text = "&Refresh";
			this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
			// 
			// sbMain
			// 
			this.sbMain.Location = new System.Drawing.Point(0, 381);
			this.sbMain.Name = "sbMain";
			this.sbMain.Size = new System.Drawing.Size(232, 16);
			this.sbMain.TabIndex = 9;
			// 
			// lvHosts
			// 
			this.lvHosts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lvHosts.BackColor = System.Drawing.Color.Beige;
			this.lvHosts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lvHosts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																					  this.clmText});
			this.lvHosts.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.lvHosts.Location = new System.Drawing.Point(8, 88);
			this.lvHosts.MultiSelect = false;
			this.lvHosts.Name = "lvHosts";
			this.lvHosts.Size = new System.Drawing.Size(216, 248);
			this.lvHosts.SmallImageList = this.ilHosts;
			this.lvHosts.Sorting = System.Windows.Forms.SortOrder.Ascending;
			this.lvHosts.TabIndex = 10;
			this.lvHosts.View = System.Windows.Forms.View.Details;
			this.lvHosts.DoubleClick += new System.EventHandler(this.lvHosts_DoubleClick);
			this.lvHosts.SelectedIndexChanged += new System.EventHandler(this.lvHosts_SelectedIndexChanged);
			// 
			// clmText
			// 
			this.clmText.Text = "";
			this.clmText.Width = 210;
			// 
			// ilHosts
			// 
			this.ilHosts.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.ilHosts.ImageSize = new System.Drawing.Size(30, 30);
			this.ilHosts.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilHosts.ImageStream")));
			this.ilHosts.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// pbProfile
			// 
			this.pbProfile.Image = ((System.Drawing.Image)(resources.GetObject("pbProfile.Image")));
			this.pbProfile.Location = new System.Drawing.Point(8, 8);
			this.pbProfile.Name = "pbProfile";
			this.pbProfile.Size = new System.Drawing.Size(50, 50);
			this.pbProfile.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pbProfile.TabIndex = 11;
			this.pbProfile.TabStop = false;
			// 
			// lblPreferredName
			// 
			this.lblPreferredName.AutoSize = true;
			this.lblPreferredName.BackColor = System.Drawing.Color.Transparent;
			this.lblPreferredName.Font = new System.Drawing.Font("Times New Roman", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblPreferredName.Location = new System.Drawing.Point(64, 16);
			this.lblPreferredName.Name = "lblPreferredName";
			this.lblPreferredName.Size = new System.Drawing.Size(153, 28);
			this.lblPreferredName.TabIndex = 12;
			this.lblPreferredName.Text = "Peer Messenger";
			// 
			// tmrProfile
			// 
			this.tmrProfile.Interval = 5000;
			this.tmrProfile.Tick += new System.EventHandler(this.tmrProfile_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.LightYellow;
			this.ClientSize = new System.Drawing.Size(232, 397);
			this.ContextMenu = this.cmnuMain;
			this.Controls.Add(this.lblPreferredName);
			this.Controls.Add(this.pbProfile);
			this.Controls.Add(this.lvHosts);
			this.Controls.Add(this.sbMain);
			this.Controls.Add(this.btnRefresh);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.txtSearch);
			this.Controls.Add(this.btnChat);
			this.Controls.Add(this.btnExit);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Peer Messenger";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.Activated += new System.EventHandler(this.MainForm_Activated);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		#region Events
		private void MainForm_Activated(object sender, System.EventArgs e)
		{
			try
			{
				_FocusSearch();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void txtSearch_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			try
			{
				if(e.KeyCode == Keys.Enter)
				{
					btnSearch_Click(null, null);
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			try
			{
				ArrayList items = new ArrayList();
				if(txtSearch.Text.Length > 0)
				{
					foreach(Host h in hosts.Values)
					{
						if(h.PreferredName.ToLower().StartsWith(txtSearch.Text.ToLower()))
						{
							items.Add(h);
						}
					}
				}
				else
				{
					items.AddRange(hosts.Values);
				}

				lvHosts.BeginUpdate();
				lvHosts.Items.Clear();
				lvHosts.Items.AddRange((Host[])items.ToArray(typeof(Host)));
				lvHosts.EndUpdate();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void mnuExit_Click(object sender, System.EventArgs e)
		{
			try
			{
				_ExitHandler();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
			try
			{
				//Start up the logger
				log4net.Config.DOMConfigurator.Configure();

				//Get the username out of the application configuration
				string userName = Environment.UserName;
				if(ConfigurationManager.UserName != null)
				{
					userName = ConfigurationManager.UserName;
				}
				this.Text += " - " + userName;
				lblPreferredName.Text = userName;

				//Set up host settings for self			
				self = new Host(Environment.UserName, userName, Environment.MachineName);
				if(ConfigurationManager.ProfilePicture != null && ConfigurationManager.ProfilePicture.Length > 0)
				{
					try
					{
						pbProfile.Image = Image.FromFile(ConfigurationManager.ProfilePicture);
					}
					catch(Exception ex)
					{
						logger.Error(ex.Message, ex);
					}
				}

				_StartListening();

				//Broadcast client presence over IPMsg
				udpManager.BroadcastIPPresence();

				//Send out info about profile picture
				if(ConfigurationManager.ProfilePicture != null && ConfigurationManager.ProfilePicture.Length > 0)
				{
					udpManager.BroadcastIPProfilePicture(ConfigurationManager.ProfilePicture);
				}

				//Add yourself to the host list
				hosts.Add(self.Sender, self);

				tmrBroadcast.Enabled = true;
				tmrBroadcast.Start();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
				MessageBox.Show(this, "Could not start Peer Messenger due to the following reason: " + ex.Message + ". Please ensure you do not have IP Messenger running.", "Peer Messenger - Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1);
				Application.Exit();
			}
		}

		private void btnExit_Click(object sender, System.EventArgs e)
		{
			try
			{
				_ExitHandler();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}	
	
		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			try
			{
				e.Cancel = true;
				niPeerMessenger.Text = self.PreferredName;
				niPeerMessenger.Visible = true;
				this.ShowInTaskbar = false;
				this.Hide();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void btnChat_Click(object sender, System.EventArgs e)
		{
			try
			{
				_StartConversation();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void menuItem1_Click(object sender, System.EventArgs e)
		{
			try
			{
				OptionsDialog o = new OptionsDialog();
				o.UserName = self.PreferredName;
				string logFile = ConfigurationManager.LogFile.Replace("\\\\", "\\");
				o.LogFile = logFile;
				o.DisablePeerMessengerSupport = ConfigurationManager.DisablePeerMessengerSupport;
				o.Seal = ConfigurationManager.Seal;
				o.ProfilePicture = ConfigurationManager.ProfilePicture;

				if(o.ShowDialog() == DialogResult.OK)
				{
					if(o.UserName != self.PreferredName)
					{
						ConfigurationManager.SetUserName(o.UserName);
					}

					if(o.ProfilePicture != ConfigurationManager.ProfilePicture)
					{
						ConfigurationManager.SetProfilePicture(o.ProfilePicture);
					}

					if(o.LogFile != logFile)
					{
						ConfigurationManager.SetLogFile(o.LogFile);
					}

					if(o.DisablePeerMessengerSupport != ConfigurationManager.DisablePeerMessengerSupport)
					{
						ConfigurationManager.SetDisablePeerMessengerSupport(o.DisablePeerMessengerSupport);
					}

					if(o.Seal != ConfigurationManager.Seal)
					{
						ConfigurationManager.SetSeal(o.Seal);
					}

					MessageBox.Show(this, "Please restart Peer Messenger for the changes to take effect.", "Peer Messenger", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void tmrBroadcast_Tick(object sender, System.EventArgs e)
		{
			try
			{
				tmrBroadcast.Stop();
				tmrBroadcast.Enabled = false;

				//Broadcast presence over PeerMessenger network. 
				//The IPMsg listener does its own broadcasting.
				if(ConfigurationManager.DisablePeerMessengerSupport == false)
				{
					udpManager.BroadcastPresence();
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void txtSearch_TextChanged(object sender, System.EventArgs e)
		{
			try
			{
				tmrSearch.Stop();
				tmrSearch.Start();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void tmrSearch_Tick(object sender, System.EventArgs e)
		{
			try
			{
				tmrSearch.Stop();
				btnSearch_Click(null, null);
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void btnRefresh_Click(object sender, System.EventArgs e)
		{
			try
			{
				_Refresh();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void mnuViewLog_Click(object sender, System.EventArgs e)
		{
			try
			{
				Process.Start("notepad.exe", ConfigurationManager.LogFile.Replace("\\\\", "\\"));
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void niPeerMessenger_DoubleClick(object sender, System.EventArgs e)
		{
			try
			{
				niPeerMessenger.Visible = false;
				this.ShowInTaskbar = true;
				this.Show();
				_FocusSearch();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}
		#endregion
	
		#region Public properties
		public Hashtable Conversations
		{
			get
			{
				return ivConversations;
			}
		}
		#endregion

		#region ISubscriber Members
		public void GetStatusMessage(string sender, string message)
		{
			if(this.InvokeRequired == false)
			{
				GetMessage(sender, message, true);
			}
			else
			{
				this.BeginInvoke(new SendMessage(GetStatusMessage), new object[] {sender, message});
			}
		}

		public void GetMessage(string sender, string message)
		{
			if(this.InvokeRequired == false)
			{
				GetMessage(sender, message, false);
			}
			else
			{
				this.BeginInvoke(new SendMessage(GetMessage), new object[] {sender, message});
			}
		}

		public void GetFiles(string sender, SendFileInfo[] files, uint packet)
		{
			if(this.InvokeRequired == false)
			{
				lock(syncLock)
				{
					Conversation c = null;
					Host host = hosts[sender] as Host;
					if(!Conversations.Contains(sender))
					{
						c = new Conversation(this, host, self, udpManager);
						Conversations.Add(host.Sender, c);					
						c.Show();
					}
				
					c = Conversations[host.Sender] as Conversation;
					c.GetFiles(sender, files, packet);
				}
			}
			else
			{
				this.BeginInvoke(new SendFiles(GetFiles), new object[] {sender, files, packet});
			}
		}

		public void GetMessage(string sender, string message, bool status)
		{
			try
			{
				lock(syncLock)
				{
					Conversation c = null;
					Host host = hosts[sender] as Host;
					//Set up a new conversation if its not a status message
					if(Conversations.Contains(sender) == false && status == false)
					{
						c = new Conversation(this, host, self, udpManager);
						Conversations.Add(host.Sender, c);					
						c.Show();
					}
				
					c = Conversations[host.Sender] as Conversation;

					if(c != null)
					{
						if(status)
						{
							c.GetStatusMessage(sender, message);
						}
						else
						{
							c.GetMessage(sender, message);
						}
					}
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}	

		public void GetClient(Host h)
		{
			try
			{
				if(this.InvokeRequired == false)
				{
					lock(syncLock)
					{
						if(!hosts.Contains(h.Sender))
						{						
							hosts.Add(h.Sender, h);
							lvHosts.Items.Add(h);
						}
						else
						{
							Host toRemove = hosts[h.Sender] as Host;
							hosts[h.Sender] = h;
							int idx = lvHosts.Items.IndexOf(h);
							lvHosts.Items.Remove(toRemove);
							lvHosts.Items.Add(h);
						}

						if(Conversations.Contains(h.Sender))
						{
							(Conversations[h.Sender] as Conversation).GetClient(h);
						}

						_UpdateStatus();
					}
				}
				else
				{
					this.BeginInvoke(new AddClient(GetClient), new object[] {h});
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
				if(this.InvokeRequired == false)
				{
					lock(syncLock)
					{
						if(hosts.Contains(sender))
						{
							Host toRemove = hosts[sender] as Host;
							lvHosts.Items.Remove(toRemove);
							hosts.Remove(sender);

							if(Conversations.Contains(sender))
							{			
								(Conversations[sender] as Conversation).DeleteClient(sender);
							}
						}

						_UpdateStatus();
					}
				}
				else
				{
					this.BeginInvoke(new RemoveClient(DeleteClient), new object[] {sender});
				}			
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public Host IsClientKnown(string sender)
		{
			return hosts[sender] as Host;
		}
		#endregion		

		#region Private methods
		/// <summary>
		/// Set focus to the search box
		/// </summary>
		private void _FocusSearch()
		{
			txtSearch.Focus();
			txtSearch.SelectAll();
		}

		/// <summary>
		/// Start a conversation with the currently selected host
		/// </summary>
		private void _StartConversation()
		{
			if(lvHosts.SelectedIndices.Count > 0)
			{
				Conversation c = null;
				Host selectedHost = lvHosts.SelectedItems[0] as Host;
				if(!Conversations.Contains(selectedHost.Sender))
				{
					c = new Conversation(this, selectedHost, self, udpManager);
					Conversations.Add(selectedHost.Sender, c);					
					c.Show();
				}
				
				c = Conversations[selectedHost.Sender] as Conversation;
				c.Show();
				c.Select();
				c.Activate();
			}
		}

		/// <summary>
		/// Stop listeners and exit the application
		/// </summary>
		private void _ExitHandler()
		{			
			_StopListening();
			//The tray icon would hang around even after the application closed.
			niPeerMessenger.Visible = false;
			Application.Exit();
		}

		/// <summary>
		/// Stop listener threads
		/// </summary>
		private void _StopListening()
		{
			if(ConfigurationManager.DisablePeerMessengerSupport == false)
			{
				//listener.Stop();
				//listenerThread.Join();
				udpManager.BroadcastAbsence(self);
			}

			udpManager.BroadcastIPAbsence();
			udpManager.Stop();

			fileListener.Stop();

			if(ConfigurationManager.DisablePeerMessengerSupport == false)
			{
				udpManagerThread.Join();
			}

			udpManagerThreadIp.Join();
			fileListenerThread.Join();
		}

		/// <summary>
		/// Start listener threads
		/// </summary>
		private void _StartListening()
		{
			if(ConfigurationManager.DisablePeerMessengerSupport == false)
			{
				//listener = new MessageListener(3089, this);
				//listenerThread = new Thread(new ThreadStart(listener.Listen));
				//listenerThread.Start();
			}

			fileListener = new FileListener(2425, this);
			fileListenerThread = new Thread(new ThreadStart(fileListener.Listen));
			fileListenerThread.Start();

			udpManager = new UdpManager(3089, this, self, fileListener);
			udpManagerThreadIp = new Thread(new ThreadStart(udpManager.ListenIP));
			udpManagerThreadIp.Start();

			if(ConfigurationManager.DisablePeerMessengerSupport == false)
			{
				udpManagerThread = new Thread(new ThreadStart(udpManager.Listen));
				udpManagerThread.Start();
			}
		}

		private void _Reconnect()
		{
			_StopListening();
			_StartListening();
		}

		private void _Refresh()
		{
			udpManager.BroadcastIPPresence();
			if(ConfigurationManager.ProfilePicture != null && ConfigurationManager.ProfilePicture.Length > 0)
			{
				udpManager.BroadcastIPProfilePicture(ConfigurationManager.ProfilePicture);
			}
			tmrBroadcast.Enabled = true;
			tmrBroadcast.Start();
		}

		private void _UpdateStatus()
		{
			sbMain.Text = hosts.Values.Count - 1 + " users online.";
		}
		#endregion															

		private void lvHosts_DoubleClick(object sender, System.EventArgs e)
		{
			try
			{
				_StartConversation();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void lvHosts_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			try
			{
				if(lvHosts.SelectedIndices.Count > 0)
				{
					btnChat.Enabled = true;
					Host h = lvHosts.SelectedItems[0] as Host;
					if(h != null && h.ImageIndex == 0 && h.ProfilePicture != null)
					{
						string path = h.Sender + "\\" + h.ProfilePicture.Name;
						if(File.Exists(path))
						{
							try
							{
								ilHosts.Images.Add(Image.FromFile(path));
							}
							catch(Exception ex)
							{
								logger.Error(ex.Message, ex);
							}

							h.ImageIndex = ilHosts.Images.Count - 1;
						}
						else
						{
							tmrProfile.Stop();
							tmrProfile.Start();
						}
					}
				}
				else
				{
					btnChat.Enabled = false;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void ftDialog_DownloadComplete(object sender, EventArgs e)
		{
			FileTransferDialog ft = sender as FileTransferDialog;
			string path = ft.Peer.Sender + "\\" + ft.FileInfo.Name;
			_SetProfileImage(ft.Peer, path);
			ft.Dispose();
		}

		private void _SetProfileImage(Host h, string path)
		{
			try
			{
				ilHosts.Images.Add(Image.FromFile(path));
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}

			Host toUpdate = hosts[h.Sender] as Host;
			int idx = lvHosts.Items.IndexOf(toUpdate);
			if(idx > -1)
			{
				toUpdate = lvHosts.Items[idx] as Host;			
				toUpdate.ImageIndex = ilHosts.Images.Count - 1;
			}
		}

		private void tmrProfile_Tick(object sender, System.EventArgs e)
		{
			if(lvHosts.SelectedIndices.Count > 0)
			{
				Host h = lvHosts.SelectedItems[0] as Host;				
				if(h != null && h.ProfilePicture != null)
				{
					string path = h.Sender + "\\" + h.ProfilePicture.Name;
					if(!Directory.Exists(h.Sender))
					{
						Directory.CreateDirectory(h.Sender);
					}

					SendFileInfo fi = h.ProfilePicture;
					fi.FullName = path;
					FileTransferDialog ftDialog = new FileTransferDialog(self, h, 0, fi);
					ftDialog.DownloadComplete += new EventHandler(ftDialog_DownloadComplete);
					ftDialog.GetFile();
				}
			}

			tmrProfile.Stop();
		}
	}
}