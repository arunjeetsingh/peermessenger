using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using log4net;

namespace PeerMessenger
{
	/// <summary>
	/// Summary description for OptionsDialog.
	/// </summary>
	public class OptionsDialog : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label lblUserName;
		private System.Windows.Forms.TextBox txtUserName;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.SaveFileDialog sfLog;

		private string _UserName;
		private System.Windows.Forms.Button btnBrowse;
		private System.Windows.Forms.Label lblLogFile;
		private System.Windows.Forms.TextBox txtLogFile;
		private string _LogFile;
		private System.Windows.Forms.CheckBox chkPeer;
		private ILog logger = LogManager.GetLogger(typeof(OptionsDialog));
		private bool _DisablePeerMessengerSupport;

		public OptionsDialog()
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
			this.txtUserName = new System.Windows.Forms.TextBox();
			this.lblUserName = new System.Windows.Forms.Label();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblLogFile = new System.Windows.Forms.Label();
			this.txtLogFile = new System.Windows.Forms.TextBox();
			this.btnBrowse = new System.Windows.Forms.Button();
			this.sfLog = new System.Windows.Forms.SaveFileDialog();
			this.chkPeer = new System.Windows.Forms.CheckBox();
			this.SuspendLayout();
			// 
			// txtUserName
			// 
			this.txtUserName.Location = new System.Drawing.Point(96, 16);
			this.txtUserName.Name = "txtUserName";
			this.txtUserName.Size = new System.Drawing.Size(184, 20);
			this.txtUserName.TabIndex = 0;
			this.txtUserName.Text = "";
			// 
			// lblUserName
			// 
			this.lblUserName.Location = new System.Drawing.Point(8, 18);
			this.lblUserName.Name = "lblUserName";
			this.lblUserName.Size = new System.Drawing.Size(80, 16);
			this.lblUserName.TabIndex = 5;
			this.lblUserName.Text = "Display name";
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(224, 136);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(64, 24);
			this.btnOk.TabIndex = 3;
			this.btnOk.Text = "&OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(296, 136);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(64, 24);
			this.btnCancel.TabIndex = 4;
			this.btnCancel.Text = "&Cancel";
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblLogFile
			// 
			this.lblLogFile.Location = new System.Drawing.Point(8, 48);
			this.lblLogFile.Name = "lblLogFile";
			this.lblLogFile.Size = new System.Drawing.Size(80, 16);
			this.lblLogFile.TabIndex = 6;
			this.lblLogFile.Text = "Log file";
			// 
			// txtLogFile
			// 
			this.txtLogFile.Location = new System.Drawing.Point(96, 48);
			this.txtLogFile.Name = "txtLogFile";
			this.txtLogFile.Size = new System.Drawing.Size(184, 20);
			this.txtLogFile.TabIndex = 1;
			this.txtLogFile.Text = "";
			// 
			// btnBrowse
			// 
			this.btnBrowse.Location = new System.Drawing.Point(288, 48);
			this.btnBrowse.Name = "btnBrowse";
			this.btnBrowse.TabIndex = 2;
			this.btnBrowse.Text = "&Browse...";
			this.btnBrowse.Click += new System.EventHandler(this.button1_Click);
			// 
			// sfLog
			// 
			this.sfLog.DefaultExt = "log";
			this.sfLog.FileName = "PeerMessenger";
			this.sfLog.Filter = "Log files|*.log";
			this.sfLog.RestoreDirectory = true;
			this.sfLog.Title = "Select log file location";
			// 
			// chkPeer
			// 
			this.chkPeer.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
			this.chkPeer.Location = new System.Drawing.Point(8, 80);
			this.chkPeer.Name = "chkPeer";
			this.chkPeer.Size = new System.Drawing.Size(360, 40);
			this.chkPeer.TabIndex = 7;
			this.chkPeer.Text = "Disable Peer Messenger protocol support (Warning: only turn off if you are having" +
				" trouble communicating with other Peer Messenger clients)";
			// 
			// OptionsDialog
			// 
			this.AcceptButton = this.btnOk;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(368, 167);
			this.ControlBox = false;
			this.Controls.Add(this.chkPeer);
			this.Controls.Add(this.txtLogFile);
			this.Controls.Add(this.txtUserName);
			this.Controls.Add(this.btnBrowse);
			this.Controls.Add(this.lblLogFile);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.lblUserName);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "OptionsDialog";
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Text = "Peer Messenger - Options";
			this.Load += new System.EventHandler(this.OptionsDialog_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			try
			{
				UserName = txtUserName.Text;
				LogFile = txtLogFile.Text;
				DisablePeerMessengerSupport = chkPeer.Checked;
				DialogResult= DialogResult.OK;
				this.Dispose();
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		private void OptionsDialog_Load(object sender, System.EventArgs e)
		{
			try
			{
				if(UserName != null)
				{
					txtUserName.Text = UserName;
				}

				if(LogFile != null)
				{
					txtLogFile.Text = LogFile;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
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

		private void button1_Click(object sender, System.EventArgs e)
		{
			try
			{
				if(sfLog.ShowDialog() == DialogResult.OK)
				{
					txtLogFile.Text = sfLog.FileName;
				}
			}
			catch(Exception ex)
			{
				logger.Error(ex.Message, ex);
			}
		}

		public string UserName
		{
			get
			{
				return _UserName;
			}
			set
			{
				_UserName = value;
			}
		}

		public string LogFile
		{
			get
			{
				return _LogFile;
			}
			set
			{
				_LogFile = value;
			}
		}

		public bool DisablePeerMessengerSupport
		{
			get
			{
				return _DisablePeerMessengerSupport;
			}
			set
			{
				_DisablePeerMessengerSupport = value;
			}
		}
	}
}
