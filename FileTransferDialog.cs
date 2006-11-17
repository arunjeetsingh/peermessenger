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
	public class FileTransferDialog : System.Windows.Forms.Form
	{
		private Conversation _Conversation;
		private SendFileInfo _FileInfo;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.PictureBox pictureBox2;
		private System.Windows.Forms.Label lblSaving;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.Button btnOpen;
		private System.Windows.Forms.Button btnOpenFolder;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblDownloadFolder;
		private ILog logger = LogManager.GetLogger(typeof(FileTransferDialog));
		byte[] buf;
		Host _Self, _Peer;
		uint _Packet;
		private System.Windows.Forms.ProgressBar pbDownload;
		ArrayList contents;
		private System.Windows.Forms.Label lblDownloaded;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.CheckBox chkAutoClose;
		TcpClient client;

		public FileTransferDialog(Conversation conversation, Host self, Host peer, uint packet, SendFileInfo fileInfo) : this()
		{
			_Conversation = conversation;
			_Self = self;
			_Peer = peer;
			_Packet = packet;
			_FileInfo = fileInfo;
		}

		public void ReadFile(IAsyncResult result)
		{
			if(result.IsCompleted)
			{
				object[] state = result.AsyncState as object[];
				NetworkStream stream = state[0] as NetworkStream;				
				SendFileInfo file = _FileInfo;
				int bytesRead = stream.EndRead(result);
				byte[] temp = new byte[bytesRead];
				Array.Copy(buf, temp, bytesRead);
				contents.AddRange(temp);
				int percentDone = (int)((double)contents.Count/(double)_FileInfo.Size * 100);
				logger.Debug(percentDone + "% done");
				pbDownload.Value = percentDone;
				pbDownload.Update();
				lblDownloaded.Text = _GetDisplaySize(contents.Count);

				if(stream.DataAvailable == false)
				{					
					logger.Debug("Got " + file.Name);
					byte[] bfile = (byte[])contents.ToArray(typeof(byte));
					FileStream fs = new FileStream(file.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
					fs.Write(bfile, 0, bfile.Length);
					fs.Close();
					client.Close();

					btnOpen.Enabled = true;
					btnOpenFolder.Enabled = true;
					btnCancel.Enabled = false;

					if(chkAutoClose.Checked)
					{
						_AutoExit();
					}
				}
				else
				{					
					buf = new byte[buf.Length];
					stream.BeginRead(buf, 0, buf.Length, new AsyncCallback(ReadFile), new object[] {stream});
				}
			}
		}

		private string _GetDisplaySize(int bytes)
		{
			int oneKb = 1024;
			int oneMb = oneKb * oneKb;

			if(bytes > oneMb)
			{
				return (decimal.Round((decimal)bytes/(decimal)oneMb, 2) + " MB");
			}
			else if(bytes > oneKb)
			{
				return (decimal.Round((decimal)bytes/(decimal)oneKb, 2) + " KB");
			}
			else
			{
				return (bytes + " bytes");
			}
		}

		private FileTransferDialog()
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
			client.Close();
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FileTransferDialog));
			this.pbDownload = new System.Windows.Forms.ProgressBar();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.pictureBox2 = new System.Windows.Forms.PictureBox();
			this.lblSaving = new System.Windows.Forms.Label();
			this.lblInfo = new System.Windows.Forms.Label();
			this.btnOpen = new System.Windows.Forms.Button();
			this.btnOpenFolder = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.chkAutoClose = new System.Windows.Forms.CheckBox();
			this.label1 = new System.Windows.Forms.Label();
			this.lblDownloadFolder = new System.Windows.Forms.Label();
			this.lblDownloaded = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// pbDownload
			// 
			this.pbDownload.Location = new System.Drawing.Point(16, 120);
			this.pbDownload.Name = "pbDownload";
			this.pbDownload.Size = new System.Drawing.Size(368, 16);
			this.pbDownload.TabIndex = 0;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(24, 16);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(56, 56);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox1.TabIndex = 1;
			this.pictureBox1.TabStop = false;
			// 
			// pictureBox2
			// 
			this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
			this.pictureBox2.Location = new System.Drawing.Point(320, 16);
			this.pictureBox2.Name = "pictureBox2";
			this.pictureBox2.Size = new System.Drawing.Size(56, 56);
			this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.pictureBox2.TabIndex = 2;
			this.pictureBox2.TabStop = false;
			// 
			// lblSaving
			// 
			this.lblSaving.Location = new System.Drawing.Point(16, 88);
			this.lblSaving.Name = "lblSaving";
			this.lblSaving.Size = new System.Drawing.Size(100, 16);
			this.lblSaving.TabIndex = 3;
			this.lblSaving.Text = "Saving:";
			// 
			// lblInfo
			// 
			this.lblInfo.Location = new System.Drawing.Point(16, 104);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(368, 16);
			this.lblInfo.TabIndex = 4;
			// 
			// btnOpen
			// 
			this.btnOpen.BackColor = System.Drawing.SystemColors.Control;
			this.btnOpen.Enabled = false;
			this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnOpen.Location = new System.Drawing.Point(128, 232);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(80, 23);
			this.btnOpen.TabIndex = 9;
			this.btnOpen.Text = "&Open";
			this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// btnOpenFolder
			// 
			this.btnOpenFolder.BackColor = System.Drawing.SystemColors.Control;
			this.btnOpenFolder.Enabled = false;
			this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnOpenFolder.Location = new System.Drawing.Point(216, 232);
			this.btnOpenFolder.Name = "btnOpenFolder";
			this.btnOpenFolder.Size = new System.Drawing.Size(80, 23);
			this.btnOpenFolder.TabIndex = 10;
			this.btnOpenFolder.Text = "Open &Folder";
			this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.BackColor = System.Drawing.SystemColors.Control;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
			this.btnCancel.Location = new System.Drawing.Point(304, 232);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 23);
			this.btnCancel.TabIndex = 11;
			this.btnCancel.Text = "Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// chkAutoClose
			// 
			this.chkAutoClose.Location = new System.Drawing.Point(16, 192);
			this.chkAutoClose.Name = "chkAutoClose";
			this.chkAutoClose.Size = new System.Drawing.Size(368, 24);
			this.chkAutoClose.TabIndex = 12;
			this.chkAutoClose.Text = "&Close this download box when the download completes";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 168);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 16);
			this.label1.TabIndex = 13;
			this.label1.Text = "Download to:";
			// 
			// lblDownloadFolder
			// 
			this.lblDownloadFolder.Location = new System.Drawing.Point(88, 168);
			this.lblDownloadFolder.Name = "lblDownloadFolder";
			this.lblDownloadFolder.Size = new System.Drawing.Size(296, 16);
			this.lblDownloadFolder.TabIndex = 14;
			// 
			// lblDownloaded
			// 
			this.lblDownloaded.Location = new System.Drawing.Point(88, 144);
			this.lblDownloaded.Name = "lblDownloaded";
			this.lblDownloaded.Size = new System.Drawing.Size(296, 16);
			this.lblDownloaded.TabIndex = 16;
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(16, 144);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(72, 16);
			this.label3.TabIndex = 15;
			this.label3.Text = "Downloaded:";
			// 
			// FileTransferDialog
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.White;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(394, 261);
			this.Controls.Add(this.lblDownloaded);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblDownloadFolder);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.chkAutoClose);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOpenFolder);
			this.Controls.Add(this.btnOpen);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.lblSaving);
			this.Controls.Add(this.pictureBox2);
			this.Controls.Add(this.pictureBox1);
			this.Controls.Add(this.pbDownload);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "FileTransferDialog";
			this.Text = "Downloading ";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FileTransferDialog_Closing);
			this.Load += new System.EventHandler(this.FileTransferDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion		

		private void FileTransferDialog_Load(object sender, System.EventArgs e)
		{			
			lblInfo.Text = _FileInfo.Name + " from " + _Peer.PreferredName;
			string folder = Path.GetDirectoryName(_FileInfo.FullName);

			lblDownloadFolder.Text = folder;

			client = new TcpClient();
			client.NoDelay = true;
			client.Connect(_Peer.HostName, 2425);
			NetworkStream stream = client.GetStream();
			byte[] msg = MessageFormatter.FormatIpFileReceiveMessage(_Self, _Packet, _FileInfo);
			stream.Write(msg, 0, msg.Length);
			contents = new ArrayList();
			buf = new byte[_FileInfo.Size];
			stream.BeginRead(buf, 0, buf.Length, new AsyncCallback(ReadFile), new object[] {stream});
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			try
			{
				DialogResult = DialogResult.Cancel;
				this.Dispose();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void btnOpen_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Process.Start(_FileInfo.FullName);
			this.Dispose();
		}

		private void btnOpenFolder_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Process.Start(Path.GetDirectoryName(_FileInfo.FullName));
			this.Dispose();
		}

		private void FileTransferDialog_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(btnOpen.Enabled)
			{
				DialogResult = DialogResult.OK;
			}
		}

		private void _AutoExit()
		{
			DialogResult = DialogResult.OK;
			this.Dispose();
		}
	}
}
