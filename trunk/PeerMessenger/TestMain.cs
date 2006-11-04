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
using log4net;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class TestMain : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.Button btnExit;		
		private System.Windows.Forms.Button btnChat;

		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.ImageList ilContacts;
		private System.Windows.Forms.Timer tmrBlink;

		private ILog logger = LogManager.GetLogger(typeof(TestMain));		

		public TestMain()
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
			System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Deepak Nolakha", 0);
			System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("Pankaj Payal", 1);
			System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Anand", 2);
			System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Kullu", 3);
			System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Vivek - I have a really really long status message. What are we going to do about" +
				" it?", 4);
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(TestMain));
			this.btnExit = new System.Windows.Forms.Button();
			this.btnChat = new System.Windows.Forms.Button();
			this.listView1 = new System.Windows.Forms.ListView();
			this.ilContacts = new System.Windows.Forms.ImageList(this.components);
			this.tmrBlink = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// btnExit
			// 
			this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnExit.BackColor = System.Drawing.SystemColors.Control;
			this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnExit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnExit.Location = new System.Drawing.Point(152, 312);
			this.btnExit.Name = "btnExit";
			this.btnExit.TabIndex = 5;
			this.btnExit.Text = "E&xit";
			this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
			// 
			// btnChat
			// 
			this.btnChat.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnChat.BackColor = System.Drawing.SystemColors.Control;
			this.btnChat.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnChat.Enabled = false;
			this.btnChat.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnChat.Location = new System.Drawing.Point(64, 312);
			this.btnChat.Name = "btnChat";
			this.btnChat.TabIndex = 4;
			this.btnChat.Text = "&Chat";
			this.btnChat.Click += new System.EventHandler(this.btnChat_Click);
			// 
			// listView1
			// 
			this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
			listViewItem1.StateImageIndex = 0;
			listViewItem2.StateImageIndex = 0;
			listViewItem3.StateImageIndex = 0;
			listViewItem4.StateImageIndex = 0;
			listViewItem5.StateImageIndex = 0;
			this.listView1.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
																					  listViewItem1,
																					  listViewItem2,
																					  listViewItem3,
																					  listViewItem4,
																					  listViewItem5});
			this.listView1.LabelWrap = false;
			this.listView1.Location = new System.Drawing.Point(0, 0);
			this.listView1.MultiSelect = false;
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(232, 304);
			this.listView1.SmallImageList = this.ilContacts;
			this.listView1.TabIndex = 0;
			this.listView1.View = System.Windows.Forms.View.List;
			// 
			// ilContacts
			// 
			this.ilContacts.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.ilContacts.ImageSize = new System.Drawing.Size(25, 25);
			this.ilContacts.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilContacts.ImageStream")));
			this.ilContacts.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// tmrBlink
			// 
			this.tmrBlink.Interval = 5000;
			this.tmrBlink.Tick += new System.EventHandler(this.tmrBlink_Tick);
			// 
			// TestMain
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.ClientSize = new System.Drawing.Size(232, 341);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.btnChat);
			this.Controls.Add(this.btnExit);
			this.MaximizeBox = false;
			this.Name = "TestMain";
			this.Text = "Peer Messenger";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new TestMain());
		}

		#region Events
		private void MainForm_Load(object sender, System.EventArgs e)
		{
			//Start up the logger
			log4net.Config.DOMConfigurator.Configure();
			tmrBlink.Start();
		}

		private void btnExit_Click(object sender, System.EventArgs e)
		{
			Application.Exit();
		}	
	
		private void lstHosts_DoubleClick(object sender, System.EventArgs e)
		{
		}

		private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		private void lstHosts_SelectedIndexChanged(object sender, System.EventArgs e)
		{			
		}

		private void btnChat_Click(object sender, System.EventArgs e)
		{
		}
		#endregion				

		private void tmrBlink_Tick(object sender, System.EventArgs e)
		{
			if(FlashWindowHelper.FlashWindowEx(this.Handle))
			{
				logger.Debug("Flashing window");				
			}
			else
			{
				logger.Debug("Window is already selected. Don't flash.");
			}
		}
	}
}
