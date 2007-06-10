namespace Fsxget
{
	partial class FsxgetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FsxgetForm));
			this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
			this.saveFileDialogKMLFile = new System.Windows.Forms.SaveFileDialog();
			this.enableTrackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.test2ToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.runMicrosoftFlightSimulatorXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runGoogleEarthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.createGoogleEarthKMLFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.pauseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.recreateGoogleEarthObjectsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearUserAircraftPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.contextMenuStripNotifyIcon.SuspendLayout();
			this.SuspendLayout();
			// 
			// notifyIconMain
			// 
			this.notifyIconMain.ContextMenuStrip = this.contextMenuStripNotifyIcon;
			this.notifyIconMain.Text = "notifyIcon1";
			this.notifyIconMain.Visible = true;
			// 
			// saveFileDialogKMLFile
			// 
			this.saveFileDialogKMLFile.DefaultExt = "kml";
			this.saveFileDialogKMLFile.Filter = "Google Earth Files|*.kml";
			// 
			// enableTrackerToolStripMenuItem
			// 
			this.enableTrackerToolStripMenuItem.Checked = true;
			this.enableTrackerToolStripMenuItem.CheckOnClick = true;
			this.enableTrackerToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.enableTrackerToolStripMenuItem.Name = "enableTrackerToolStripMenuItem";
			this.enableTrackerToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.enableTrackerToolStripMenuItem.Text = "&Enable Tracker";
			this.enableTrackerToolStripMenuItem.Click += new System.EventHandler(this.enableTrackerToolStripMenuItem_Click);
			// 
			// test2ToolStripMenuItem
			// 
			this.test2ToolStripMenuItem.Name = "test2ToolStripMenuItem";
			this.test2ToolStripMenuItem.Size = new System.Drawing.Size(243, 6);
			// 
			// runMicrosoftFlightSimulatorXToolStripMenuItem
			// 
			this.runMicrosoftFlightSimulatorXToolStripMenuItem.Name = "runMicrosoftFlightSimulatorXToolStripMenuItem";
			this.runMicrosoftFlightSimulatorXToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.runMicrosoftFlightSimulatorXToolStripMenuItem.Text = "Run Microsoft Flight Simulator X";
			this.runMicrosoftFlightSimulatorXToolStripMenuItem.Click += new System.EventHandler(this.runMicrosoftFlightSimulatorXToolStripMenuItem_Click);
			// 
			// runGoogleEarthToolStripMenuItem
			// 
			this.runGoogleEarthToolStripMenuItem.Name = "runGoogleEarthToolStripMenuItem";
			this.runGoogleEarthToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.runGoogleEarthToolStripMenuItem.Text = "Run Google Earth 4";
			this.runGoogleEarthToolStripMenuItem.Click += new System.EventHandler(this.runGoogleEarthToolStripMenuItem_Click);
			// 
			// toolStripMenuItem4
			// 
			this.toolStripMenuItem4.Name = "toolStripMenuItem4";
			this.toolStripMenuItem4.Size = new System.Drawing.Size(243, 6);
			// 
			// createGoogleEarthKMLFileToolStripMenuItem
			// 
			this.createGoogleEarthKMLFileToolStripMenuItem.Name = "createGoogleEarthKMLFileToolStripMenuItem";
			this.createGoogleEarthKMLFileToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.createGoogleEarthKMLFileToolStripMenuItem.Text = "Create Google Earth KML File";
			this.createGoogleEarthKMLFileToolStripMenuItem.Click += new System.EventHandler(this.createGoogleEarthKMLFileToolStripMenuItem_Click);
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(243, 6);
			// 
			// pauseToolStripMenuItem
			// 
			this.pauseToolStripMenuItem.CheckOnClick = true;
			this.pauseToolStripMenuItem.Name = "pauseToolStripMenuItem";
			this.pauseToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.pauseToolStripMenuItem.Text = "Pause";
			this.pauseToolStripMenuItem.Click += new System.EventHandler(this.pauseToolStripMenuItem_Click);
			// 
			// recreateGoogleEarthObjectsToolStripMenuItem
			// 
			this.recreateGoogleEarthObjectsToolStripMenuItem.Name = "recreateGoogleEarthObjectsToolStripMenuItem";
			this.recreateGoogleEarthObjectsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.recreateGoogleEarthObjectsToolStripMenuItem.Text = "Recreate Google Earth Objects";
			this.recreateGoogleEarthObjectsToolStripMenuItem.Click += new System.EventHandler(this.recreateGoogleEarthObjectsToolStripMenuItem_Click);
			// 
			// clearUserAircraftPathToolStripMenuItem
			// 
			this.clearUserAircraftPathToolStripMenuItem.Name = "clearUserAircraftPathToolStripMenuItem";
			this.clearUserAircraftPathToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.clearUserAircraftPathToolStripMenuItem.Text = "&Clear User Aircraft Path";
			this.clearUserAircraftPathToolStripMenuItem.Click += new System.EventHandler(this.clearUserAircraftPathToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(243, 6);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// contextMenuStripNotifyIcon
			// 
			this.contextMenuStripNotifyIcon.BindingContext = null;
			this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableTrackerToolStripMenuItem,
            this.test2ToolStripMenuItem,
            this.runMicrosoftFlightSimulatorXToolStripMenuItem,
            this.runGoogleEarthToolStripMenuItem,
            this.toolStripMenuItem4,
            this.createGoogleEarthKMLFileToolStripMenuItem,
            this.toolStripMenuItem3,
            this.pauseToolStripMenuItem,
            this.recreateGoogleEarthObjectsToolStripMenuItem,
            this.clearUserAircraftPathToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
			this.contextMenuStripNotifyIcon.Name = "contextMenuStrip1";
			this.contextMenuStripNotifyIcon.Region = null;
			this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(247, 226);
			this.contextMenuStripNotifyIcon.Text = "Test";
			// 
			// FsxgetForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(462, 501);
			this.ControlBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "FsxgetForm";
			this.Text = "FSX Google Earth Tracker";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.Load += new System.EventHandler(this.Form1_Load);
			this.contextMenuStripNotifyIcon.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIconMain;
		private System.Windows.Forms.SaveFileDialog saveFileDialogKMLFile;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
		private System.Windows.Forms.ToolStripMenuItem enableTrackerToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator test2ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runMicrosoftFlightSimulatorXToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runGoogleEarthToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem createGoogleEarthKMLFileToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.ToolStripMenuItem pauseToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem recreateGoogleEarthObjectsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearUserAircraftPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
	}
}

