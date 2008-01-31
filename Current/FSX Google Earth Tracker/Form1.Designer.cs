namespace FSX_Google_Earth_Tracker
{
	partial class Form1
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.notifyIconMain = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStripNotifyIcon = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.enableTrackerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.showBalloonTipsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.test2ToolStripMenuItem = new System.Windows.Forms.ToolStripSeparator();
			this.runMicrosoftFlightSimulatorXToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.runGoogleEarthToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem4 = new System.Windows.Forms.ToolStripSeparator();
			this.createGoogleEarthKMLFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
			this.pauseRecordingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.clearUserAircraftPathToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
			this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
			this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.buttonCancel = new System.Windows.Forms.Button();
			this.buttonOK = new System.Windows.Forms.Button();
			this.timerFSXConnect = new System.Windows.Forms.Timer(this.components);
			this.timerQueryUserAircraft = new System.Windows.Forms.Timer(this.components);
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox8 = new System.Windows.Forms.GroupBox();
			this.checkBoxLoadKMLFile = new System.Windows.Forms.CheckBox();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.checkBoxUpdateCheck = new System.Windows.Forms.CheckBox();
			this.checkShowInfoBalloons = new System.Windows.Forms.CheckBox();
			this.checkEnableOnStartup = new System.Windows.Forms.CheckBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.radioButton10 = new System.Windows.Forms.RadioButton();
			this.checkBox1 = new System.Windows.Forms.CheckBox();
			this.radioButton9 = new System.Windows.Forms.RadioButton();
			this.radioButton8 = new System.Windows.Forms.RadioButton();
			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.tabControl2 = new System.Windows.Forms.TabControl();
			this.tabPage6 = new System.Windows.Forms.TabPage();
			this.groupBoxUserAircraftPathPrediction = new System.Windows.Forms.GroupBox();
			this.button2 = new System.Windows.Forms.Button();
			this.button1 = new System.Windows.Forms.Button();
			this.listBoxPathPrediction = new System.Windows.Forms.ListBox();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.label26 = new System.Windows.Forms.Label();
			this.numericUpDownUserPathPrediction = new System.Windows.Forms.NumericUpDown();
			this.checkBoxUserPathPrediction = new System.Windows.Forms.CheckBox();
			this.label10 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.numericUpDownQueryUserPath = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownQueryUserAircraft = new System.Windows.Forms.NumericUpDown();
			this.checkQueryUserPath = new System.Windows.Forms.CheckBox();
			this.checkQueryUserAircraft = new System.Windows.Forms.CheckBox();
			this.tabPage7 = new System.Windows.Forms.TabPage();
			this.groupBox9 = new System.Windows.Forms.GroupBox();
			this.checkBoxAIGroundPredictPoints = new System.Windows.Forms.CheckBox();
			this.checkBoxAIGroundPredict = new System.Windows.Forms.CheckBox();
			this.checkBoxAIBoatsPredictPoints = new System.Windows.Forms.CheckBox();
			this.checkBoxAIBoatsPredict = new System.Windows.Forms.CheckBox();
			this.checkBoxAIHelicoptersPredictPoints = new System.Windows.Forms.CheckBox();
			this.checkBoxAIHelicoptersPredict = new System.Windows.Forms.CheckBox();
			this.checkBoxAIAircraftsPredictPoints = new System.Windows.Forms.CheckBox();
			this.checkBoxAIAircraftsPredict = new System.Windows.Forms.CheckBox();
			this.label22 = new System.Windows.Forms.Label();
			this.numericUpDownQueryAIGroudUnitsRadius = new System.Windows.Forms.NumericUpDown();
			this.label23 = new System.Windows.Forms.Label();
			this.label24 = new System.Windows.Forms.Label();
			this.label25 = new System.Windows.Forms.Label();
			this.numericUpDownQueryAIBoatsRadius = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownQueryAIHelicoptersRadius = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownQueryAIAircraftsRadius = new System.Windows.Forms.NumericUpDown();
			this.label21 = new System.Windows.Forms.Label();
			this.numericUpDownQueryAIGroudUnitsInterval = new System.Windows.Forms.NumericUpDown();
			this.checkBoxQueryAIGroudUnits = new System.Windows.Forms.CheckBox();
			this.label13 = new System.Windows.Forms.Label();
			this.label12 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.numericUpDownQueryAIBoatsInterval = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownQueryAIHelicoptersInterval = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownQueryAIAircraftsInterval = new System.Windows.Forms.NumericUpDown();
			this.checkBoxQueryAIBoats = new System.Windows.Forms.CheckBox();
			this.checkBoxQueryAIHelicopters = new System.Windows.Forms.CheckBox();
			this.checkBoxQueryAIAircrafts = new System.Windows.Forms.CheckBox();
			this.checkBoxQueryAIObjects = new System.Windows.Forms.CheckBox();
			this.tabPage9 = new System.Windows.Forms.TabPage();
			this.groupBox10 = new System.Windows.Forms.GroupBox();
			this.textBox3 = new System.Windows.Forms.TextBox();
			this.label33 = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label32 = new System.Windows.Forms.Label();
			this.label31 = new System.Windows.Forms.Label();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.label30 = new System.Windows.Forms.Label();
			this.radioButton7 = new System.Windows.Forms.RadioButton();
			this.label29 = new System.Windows.Forms.Label();
			this.radioButton6 = new System.Windows.Forms.RadioButton();
			this.tabPage4 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.label20 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshUserPrediction = new System.Windows.Forms.NumericUpDown();
			this.label27 = new System.Windows.Forms.Label();
			this.label19 = new System.Windows.Forms.Label();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.label16 = new System.Windows.Forms.Label();
			this.label15 = new System.Windows.Forms.Label();
			this.label14 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshAIGroundUnits = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshAIBoats = new System.Windows.Forms.NumericUpDown();
			this.label8 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshAIHelicopter = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshAIAircrafts = new System.Windows.Forms.NumericUpDown();
			this.label5 = new System.Windows.Forms.Label();
			this.numericUpDownRefreshUserPath = new System.Windows.Forms.NumericUpDown();
			this.numericUpDownRefreshUserAircraft = new System.Windows.Forms.NumericUpDown();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.tabPage8 = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.panel2 = new System.Windows.Forms.Panel();
			this.radioButtonAccessLocalOnly = new System.Windows.Forms.RadioButton();
			this.radioButtonAccessRemote = new System.Windows.Forms.RadioButton();
			this.panel1 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.textBoxLocalPubPath = new System.Windows.Forms.TextBox();
			this.radioButton5 = new System.Windows.Forms.RadioButton();
			this.radioButton4 = new System.Windows.Forms.RadioButton();
			this.radioButton1 = new System.Windows.Forms.RadioButton();
			this.radioButton2 = new System.Windows.Forms.RadioButton();
			this.radioButton3 = new System.Windows.Forms.RadioButton();
			this.label28 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.numericUpDownServerPort = new System.Windows.Forms.NumericUpDown();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox6 = new System.Windows.Forms.GroupBox();
			this.checkBoxSubFoldersForLog = new System.Windows.Forms.CheckBox();
			this.checkBoxSaveLog = new System.Windows.Forms.CheckBox();
			this.tabPage5 = new System.Windows.Forms.TabPage();
			this.groupBox7 = new System.Windows.Forms.GroupBox();
			this.linkLabel1 = new System.Windows.Forms.LinkLabel();
			this.labelVersion = new System.Windows.Forms.Label();
			this.labelCopyright = new System.Windows.Forms.Label();
			this.labelCompanyName = new System.Windows.Forms.Label();
			this.labelProductName = new System.Windows.Forms.Label();
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			this.timerQueryUserPath = new System.Windows.Forms.Timer(this.components);
			this.timerQueryAIAircrafts = new System.Windows.Forms.Timer(this.components);
			this.timerQueryAIHelicopters = new System.Windows.Forms.Timer(this.components);
			this.timerQueryAIBoats = new System.Windows.Forms.Timer(this.components);
			this.timerQueryAIGroundUnits = new System.Windows.Forms.Timer(this.components);
			this.timerUserPrediction = new System.Windows.Forms.Timer(this.components);
			this.timerIPAddressRefresh = new System.Windows.Forms.Timer(this.components);
			this.saveFileDialogKMLFile = new System.Windows.Forms.SaveFileDialog();
			this.contextMenuStripNotifyIcon.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox8.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.tabControl2.SuspendLayout();
			this.tabPage6.SuspendLayout();
			this.groupBoxUserAircraftPathPrediction.SuspendLayout();
			this.groupBox5.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownUserPathPrediction)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryUserPath)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryUserAircraft)).BeginInit();
			this.tabPage7.SuspendLayout();
			this.groupBox9.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIGroudUnitsRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIBoatsRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIHelicoptersRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIAircraftsRadius)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIGroudUnitsInterval)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIBoatsInterval)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIHelicoptersInterval)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIAircraftsInterval)).BeginInit();
			this.tabPage9.SuspendLayout();
			this.groupBox10.SuspendLayout();
			this.tabPage4.SuspendLayout();
			this.groupBox1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserPrediction)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIGroundUnits)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIBoats)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIHelicopter)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIAircrafts)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserPath)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserAircraft)).BeginInit();
			this.tabPage8.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel1.SuspendLayout();
			this.panel3.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).BeginInit();
			this.tabPage2.SuspendLayout();
			this.groupBox6.SuspendLayout();
			this.tabPage5.SuspendLayout();
			this.groupBox7.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// notifyIconMain
			// 
			this.notifyIconMain.ContextMenuStrip = this.contextMenuStripNotifyIcon;
			this.notifyIconMain.Text = "notifyIcon1";
			this.notifyIconMain.Visible = true;
			this.notifyIconMain.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseClick);
			// 
			// contextMenuStripNotifyIcon
			// 
			this.contextMenuStripNotifyIcon.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableTrackerToolStripMenuItem,
            this.showBalloonTipsToolStripMenuItem,
            this.test2ToolStripMenuItem,
            this.runMicrosoftFlightSimulatorXToolStripMenuItem,
            this.runGoogleEarthToolStripMenuItem,
            this.toolStripMenuItem4,
            this.createGoogleEarthKMLFileToolStripMenuItem,
            this.toolStripMenuItem3,
            this.pauseRecordingToolStripMenuItem,
            this.clearUserAircraftPathToolStripMenuItem,
            this.toolStripMenuItem2,
            this.optionsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.aboutToolStripMenuItem,
            this.exitToolStripMenuItem});
			this.contextMenuStripNotifyIcon.Name = "contextMenuStrip1";
			this.contextMenuStripNotifyIcon.Size = new System.Drawing.Size(247, 254);
			this.contextMenuStripNotifyIcon.Text = "Test";
			this.contextMenuStripNotifyIcon.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStripNotifyIcon_Opening);
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
			// showBalloonTipsToolStripMenuItem
			// 
			this.showBalloonTipsToolStripMenuItem.Checked = true;
			this.showBalloonTipsToolStripMenuItem.CheckOnClick = true;
			this.showBalloonTipsToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
			this.showBalloonTipsToolStripMenuItem.Name = "showBalloonTipsToolStripMenuItem";
			this.showBalloonTipsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.showBalloonTipsToolStripMenuItem.Text = "Show Balloon &Tips";
			this.showBalloonTipsToolStripMenuItem.Click += new System.EventHandler(this.showBalloonTipsToolStripMenuItem_Click);
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
			// 
			// toolStripMenuItem3
			// 
			this.toolStripMenuItem3.Name = "toolStripMenuItem3";
			this.toolStripMenuItem3.Size = new System.Drawing.Size(243, 6);
			// 
			// pauseRecordingToolStripMenuItem
			// 
			this.pauseRecordingToolStripMenuItem.CheckOnClick = true;
			this.pauseRecordingToolStripMenuItem.Enabled = false;
			this.pauseRecordingToolStripMenuItem.Name = "pauseRecordingToolStripMenuItem";
			this.pauseRecordingToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.pauseRecordingToolStripMenuItem.Text = "&Pause Tracking";
			// 
			// clearUserAircraftPathToolStripMenuItem
			// 
			this.clearUserAircraftPathToolStripMenuItem.Name = "clearUserAircraftPathToolStripMenuItem";
			this.clearUserAircraftPathToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.clearUserAircraftPathToolStripMenuItem.Text = "&Clear User Aircraft Path";
			this.clearUserAircraftPathToolStripMenuItem.Click += new System.EventHandler(this.clearUserAircraftPathToolStripMenuItem_Click);
			// 
			// toolStripMenuItem2
			// 
			this.toolStripMenuItem2.Name = "toolStripMenuItem2";
			this.toolStripMenuItem2.Size = new System.Drawing.Size(243, 6);
			// 
			// optionsToolStripMenuItem
			// 
			this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
			this.optionsToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.optionsToolStripMenuItem.Text = "&Options";
			this.optionsToolStripMenuItem.Click += new System.EventHandler(this.optionsToolStripMenuItem_Click);
			// 
			// toolStripMenuItem1
			// 
			this.toolStripMenuItem1.Name = "toolStripMenuItem1";
			this.toolStripMenuItem1.Size = new System.Drawing.Size(243, 6);
			// 
			// aboutToolStripMenuItem
			// 
			this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
			this.aboutToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.aboutToolStripMenuItem.Text = "&About";
			this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
			// 
			// exitToolStripMenuItem
			// 
			this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
			this.exitToolStripMenuItem.Size = new System.Drawing.Size(246, 22);
			this.exitToolStripMenuItem.Text = "E&xit";
			this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
			// 
			// buttonCancel
			// 
			this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.buttonCancel.Location = new System.Drawing.Point(375, 466);
			this.buttonCancel.Name = "buttonCancel";
			this.buttonCancel.Size = new System.Drawing.Size(75, 23);
			this.buttonCancel.TabIndex = 2;
			this.buttonCancel.Text = "Cancel";
			this.buttonCancel.UseVisualStyleBackColor = true;
			this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
			// 
			// buttonOK
			// 
			this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.buttonOK.Location = new System.Drawing.Point(294, 466);
			this.buttonOK.Name = "buttonOK";
			this.buttonOK.Size = new System.Drawing.Size(75, 23);
			this.buttonOK.TabIndex = 1;
			this.buttonOK.Text = "OK";
			this.buttonOK.UseVisualStyleBackColor = true;
			this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
			// 
			// timerFSXConnect
			// 
			this.timerFSXConnect.Tick += new System.EventHandler(this.timerFSXConnect_Tick);
			// 
			// timerQueryUserAircraft
			// 
			this.timerQueryUserAircraft.Tick += new System.EventHandler(this.timerQueryUserAircraft_Tick);
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Controls.Add(this.tabPage4);
			this.tabControl1.Controls.Add(this.tabPage8);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage5);
			this.tabControl1.Location = new System.Drawing.Point(12, 12);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(438, 448);
			this.tabControl1.TabIndex = 3;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox8);
			this.tabPage1.Controls.Add(this.groupBox4);
			this.tabPage1.Controls.Add(this.groupBox3);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(430, 422);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "General";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// groupBox8
			// 
			this.groupBox8.Controls.Add(this.checkBoxLoadKMLFile);
			this.groupBox8.Location = new System.Drawing.Point(12, 325);
			this.groupBox8.Name = "groupBox8";
			this.groupBox8.Size = new System.Drawing.Size(402, 67);
			this.groupBox8.TabIndex = 3;
			this.groupBox8.TabStop = false;
			this.groupBox8.Text = "Misc";
			// 
			// checkBoxLoadKMLFile
			// 
			this.checkBoxLoadKMLFile.AutoSize = true;
			this.checkBoxLoadKMLFile.Checked = true;
			this.checkBoxLoadKMLFile.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxLoadKMLFile.Location = new System.Drawing.Point(16, 29);
			this.checkBoxLoadKMLFile.Name = "checkBoxLoadKMLFile";
			this.checkBoxLoadKMLFile.Size = new System.Drawing.Size(313, 17);
			this.checkBoxLoadKMLFile.TabIndex = 0;
			this.checkBoxLoadKMLFile.Text = "Load KML file when running Google Earth from context menu";
			this.checkBoxLoadKMLFile.UseVisualStyleBackColor = true;
			this.checkBoxLoadKMLFile.CheckedChanged += new System.EventHandler(this.checkBoxLoadKMLFile_CheckedChanged);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.Add(this.checkBoxUpdateCheck);
			this.groupBox4.Controls.Add(this.checkShowInfoBalloons);
			this.groupBox4.Controls.Add(this.checkEnableOnStartup);
			this.groupBox4.Location = new System.Drawing.Point(12, 177);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(402, 132);
			this.groupBox4.TabIndex = 2;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Program Start";
			// 
			// checkBoxUpdateCheck
			// 
			this.checkBoxUpdateCheck.AutoSize = true;
			this.checkBoxUpdateCheck.Enabled = false;
			this.checkBoxUpdateCheck.Location = new System.Drawing.Point(16, 94);
			this.checkBoxUpdateCheck.Name = "checkBoxUpdateCheck";
			this.checkBoxUpdateCheck.Size = new System.Drawing.Size(185, 17);
			this.checkBoxUpdateCheck.TabIndex = 2;
			this.checkBoxUpdateCheck.Text = "Check for program updates online";
			this.checkBoxUpdateCheck.UseVisualStyleBackColor = true;
			// 
			// checkShowInfoBalloons
			// 
			this.checkShowInfoBalloons.AutoSize = true;
			this.checkShowInfoBalloons.Checked = true;
			this.checkShowInfoBalloons.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkShowInfoBalloons.Location = new System.Drawing.Point(16, 52);
			this.checkShowInfoBalloons.Name = "checkShowInfoBalloons";
			this.checkShowInfoBalloons.Size = new System.Drawing.Size(292, 17);
			this.checkShowInfoBalloons.TabIndex = 1;
			this.checkShowInfoBalloons.Text = "Show information balloons on FSX connect / disconnect";
			this.checkShowInfoBalloons.UseVisualStyleBackColor = true;
			this.checkShowInfoBalloons.CheckedChanged += new System.EventHandler(this.checkShowInfoBalloons_CheckedChanged);
			// 
			// checkEnableOnStartup
			// 
			this.checkEnableOnStartup.AutoSize = true;
			this.checkEnableOnStartup.Checked = true;
			this.checkEnableOnStartup.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkEnableOnStartup.Location = new System.Drawing.Point(16, 29);
			this.checkEnableOnStartup.Name = "checkEnableOnStartup";
			this.checkEnableOnStartup.Size = new System.Drawing.Size(109, 17);
			this.checkEnableOnStartup.TabIndex = 0;
			this.checkEnableOnStartup.Text = "Enable on startup";
			this.checkEnableOnStartup.UseVisualStyleBackColor = true;
			this.checkEnableOnStartup.CheckedChanged += new System.EventHandler(this.checkEnableOnStartup_CheckedChanged);
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.radioButton10);
			this.groupBox3.Controls.Add(this.checkBox1);
			this.groupBox3.Controls.Add(this.radioButton9);
			this.groupBox3.Controls.Add(this.radioButton8);
			this.groupBox3.Location = new System.Drawing.Point(12, 15);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(402, 146);
			this.groupBox3.TabIndex = 1;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "StartUp Options";
			// 
			// radioButton10
			// 
			this.radioButton10.AutoSize = true;
			this.radioButton10.Checked = true;
			this.radioButton10.Location = new System.Drawing.Point(16, 112);
			this.radioButton10.Name = "radioButton10";
			this.radioButton10.Size = new System.Drawing.Size(91, 17);
			this.radioButton10.TabIndex = 4;
			this.radioButton10.TabStop = true;
			this.radioButton10.Text = "Start manually";
			this.radioButton10.UseVisualStyleBackColor = true;
			this.radioButton10.CheckedChanged += new System.EventHandler(this.radioButton10_CheckedChanged);
			// 
			// checkBox1
			// 
			this.checkBox1.AutoSize = true;
			this.checkBox1.Location = new System.Drawing.Point(36, 89);
			this.checkBox1.Name = "checkBox1";
			this.checkBox1.Size = new System.Drawing.Size(182, 17);
			this.checkBox1.TabIndex = 3;
			this.checkBox1.Text = "Close FSXGET when exiting FSX";
			this.checkBox1.UseVisualStyleBackColor = true;
			this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
			// 
			// radioButton9
			// 
			this.radioButton9.AutoSize = true;
			this.radioButton9.Location = new System.Drawing.Point(16, 52);
			this.radioButton9.Name = "radioButton9";
			this.radioButton9.Size = new System.Drawing.Size(296, 30);
			this.radioButton9.TabIndex = 2;
			this.radioButton9.Text = "Start when Flight Simulator starts\r\n(only when running FSX and FSXGET on same com" +
				"puter)";
			this.radioButton9.UseVisualStyleBackColor = true;
			this.radioButton9.CheckedChanged += new System.EventHandler(this.radioButton9_CheckedChanged);
			// 
			// radioButton8
			// 
			this.radioButton8.AutoSize = true;
			this.radioButton8.Location = new System.Drawing.Point(16, 29);
			this.radioButton8.Name = "radioButton8";
			this.radioButton8.Size = new System.Drawing.Size(151, 17);
			this.radioButton8.TabIndex = 1;
			this.radioButton8.Text = "Start when Windows starts";
			this.radioButton8.UseVisualStyleBackColor = true;
			this.radioButton8.CheckedChanged += new System.EventHandler(this.radioButton8_CheckedChanged);
			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.tabControl2);
			this.tabPage3.Location = new System.Drawing.Point(4, 22);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(430, 422);
			this.tabPage3.TabIndex = 1;
			this.tabPage3.Text = "Flight Simulator";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// tabControl2
			// 
			this.tabControl2.Controls.Add(this.tabPage6);
			this.tabControl2.Controls.Add(this.tabPage7);
			this.tabControl2.Controls.Add(this.tabPage9);
			this.tabControl2.Location = new System.Drawing.Point(12, 12);
			this.tabControl2.Name = "tabControl2";
			this.tabControl2.SelectedIndex = 0;
			this.tabControl2.Size = new System.Drawing.Size(405, 398);
			this.tabControl2.TabIndex = 8;
			// 
			// tabPage6
			// 
			this.tabPage6.Controls.Add(this.groupBoxUserAircraftPathPrediction);
			this.tabPage6.Controls.Add(this.groupBox5);
			this.tabPage6.Location = new System.Drawing.Point(4, 22);
			this.tabPage6.Name = "tabPage6";
			this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage6.Size = new System.Drawing.Size(397, 372);
			this.tabPage6.TabIndex = 0;
			this.tabPage6.Text = "User Aircraft";
			this.tabPage6.UseVisualStyleBackColor = true;
			// 
			// groupBoxUserAircraftPathPrediction
			// 
			this.groupBoxUserAircraftPathPrediction.Controls.Add(this.button2);
			this.groupBoxUserAircraftPathPrediction.Controls.Add(this.button1);
			this.groupBoxUserAircraftPathPrediction.Controls.Add(this.listBoxPathPrediction);
			this.groupBoxUserAircraftPathPrediction.Enabled = false;
			this.groupBoxUserAircraftPathPrediction.Location = new System.Drawing.Point(14, 155);
			this.groupBoxUserAircraftPathPrediction.Name = "groupBoxUserAircraftPathPrediction";
			this.groupBoxUserAircraftPathPrediction.Size = new System.Drawing.Size(367, 144);
			this.groupBoxUserAircraftPathPrediction.TabIndex = 8;
			this.groupBoxUserAircraftPathPrediction.TabStop = false;
			this.groupBoxUserAircraftPathPrediction.Text = "Course Prediction Settings";
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(271, 57);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(75, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Remove";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(271, 28);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(75, 23);
			this.button1.TabIndex = 1;
			this.button1.Text = "Add";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// listBoxPathPrediction
			// 
			this.listBoxPathPrediction.FormattingEnabled = true;
			this.listBoxPathPrediction.Location = new System.Drawing.Point(15, 28);
			this.listBoxPathPrediction.Name = "listBoxPathPrediction";
			this.listBoxPathPrediction.Size = new System.Drawing.Size(250, 95);
			this.listBoxPathPrediction.TabIndex = 0;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.Add(this.label26);
			this.groupBox5.Controls.Add(this.numericUpDownUserPathPrediction);
			this.groupBox5.Controls.Add(this.checkBoxUserPathPrediction);
			this.groupBox5.Controls.Add(this.label10);
			this.groupBox5.Controls.Add(this.label9);
			this.groupBox5.Controls.Add(this.numericUpDownQueryUserPath);
			this.groupBox5.Controls.Add(this.numericUpDownQueryUserAircraft);
			this.groupBox5.Controls.Add(this.checkQueryUserPath);
			this.groupBox5.Controls.Add(this.checkQueryUserAircraft);
			this.groupBox5.Location = new System.Drawing.Point(14, 15);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(367, 125);
			this.groupBox5.TabIndex = 7;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Query Settings";
			// 
			// label26
			// 
			this.label26.AutoSize = true;
			this.label26.Location = new System.Drawing.Point(326, 86);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(20, 13);
			this.label26.TabIndex = 21;
			this.label26.Text = "ms";
			// 
			// numericUpDownUserPathPrediction
			// 
			this.numericUpDownUserPathPrediction.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownUserPathPrediction.Location = new System.Drawing.Point(267, 84);
			this.numericUpDownUserPathPrediction.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownUserPathPrediction.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownUserPathPrediction.Name = "numericUpDownUserPathPrediction";
			this.numericUpDownUserPathPrediction.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownUserPathPrediction.TabIndex = 20;
			this.numericUpDownUserPathPrediction.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericUpDownUserPathPrediction.ValueChanged += new System.EventHandler(this.numericUpDownUserPathPrediction_ValueChanged);
			// 
			// checkBoxUserPathPrediction
			// 
			this.checkBoxUserPathPrediction.AutoSize = true;
			this.checkBoxUserPathPrediction.Checked = true;
			this.checkBoxUserPathPrediction.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxUserPathPrediction.Location = new System.Drawing.Point(15, 88);
			this.checkBoxUserPathPrediction.Name = "checkBoxUserPathPrediction";
			this.checkBoxUserPathPrediction.Size = new System.Drawing.Size(175, 17);
			this.checkBoxUserPathPrediction.TabIndex = 19;
			this.checkBoxUserPathPrediction.Text = "Calculate path prediction, every";
			this.checkBoxUserPathPrediction.UseVisualStyleBackColor = true;
			this.checkBoxUserPathPrediction.CheckedChanged += new System.EventHandler(this.checkBoxUserPathPrediction_CheckedChanged);
			// 
			// label10
			// 
			this.label10.AutoSize = true;
			this.label10.Location = new System.Drawing.Point(326, 63);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(20, 13);
			this.label10.TabIndex = 18;
			this.label10.Text = "ms";
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(326, 29);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(20, 13);
			this.label9.TabIndex = 17;
			this.label9.Text = "ms";
			// 
			// numericUpDownQueryUserPath
			// 
			this.numericUpDownQueryUserPath.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryUserPath.Location = new System.Drawing.Point(267, 61);
			this.numericUpDownQueryUserPath.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryUserPath.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryUserPath.Name = "numericUpDownQueryUserPath";
			this.numericUpDownQueryUserPath.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryUserPath.TabIndex = 13;
			this.numericUpDownQueryUserPath.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericUpDownQueryUserPath.ValueChanged += new System.EventHandler(this.numericUpDownQueryUserPath_ValueChanged);
			// 
			// numericUpDownQueryUserAircraft
			// 
			this.numericUpDownQueryUserAircraft.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryUserAircraft.Location = new System.Drawing.Point(267, 24);
			this.numericUpDownQueryUserAircraft.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryUserAircraft.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryUserAircraft.Name = "numericUpDownQueryUserAircraft";
			this.numericUpDownQueryUserAircraft.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryUserAircraft.TabIndex = 12;
			this.numericUpDownQueryUserAircraft.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
			this.numericUpDownQueryUserAircraft.ValueChanged += new System.EventHandler(this.numericUpDownQueryUserAircraft_ValueChanged);
			// 
			// checkQueryUserPath
			// 
			this.checkQueryUserPath.AutoSize = true;
			this.checkQueryUserPath.Checked = true;
			this.checkQueryUserPath.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkQueryUserPath.Location = new System.Drawing.Point(15, 65);
			this.checkQueryUserPath.Name = "checkQueryUserPath";
			this.checkQueryUserPath.Size = new System.Drawing.Size(133, 17);
			this.checkQueryUserPath.TabIndex = 7;
			this.checkQueryUserPath.Text = "Query user path, every";
			this.checkQueryUserPath.UseVisualStyleBackColor = true;
			this.checkQueryUserPath.CheckedChanged += new System.EventHandler(this.checkQueryUserPath_CheckedChanged);
			// 
			// checkQueryUserAircraft
			// 
			this.checkQueryUserAircraft.AutoSize = true;
			this.checkQueryUserAircraft.Checked = true;
			this.checkQueryUserAircraft.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkQueryUserAircraft.Location = new System.Drawing.Point(15, 28);
			this.checkQueryUserAircraft.Name = "checkQueryUserAircraft";
			this.checkQueryUserAircraft.Size = new System.Drawing.Size(144, 17);
			this.checkQueryUserAircraft.TabIndex = 6;
			this.checkQueryUserAircraft.Text = "Query user aircraft, every";
			this.checkQueryUserAircraft.UseVisualStyleBackColor = true;
			this.checkQueryUserAircraft.CheckedChanged += new System.EventHandler(this.checkQueryUserAircraft_CheckedChanged);
			// 
			// tabPage7
			// 
			this.tabPage7.Controls.Add(this.groupBox9);
			this.tabPage7.Location = new System.Drawing.Point(4, 22);
			this.tabPage7.Name = "tabPage7";
			this.tabPage7.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage7.Size = new System.Drawing.Size(397, 372);
			this.tabPage7.TabIndex = 1;
			this.tabPage7.Text = "AI Objects";
			this.tabPage7.UseVisualStyleBackColor = true;
			// 
			// groupBox9
			// 
			this.groupBox9.Controls.Add(this.checkBoxAIGroundPredictPoints);
			this.groupBox9.Controls.Add(this.checkBoxAIGroundPredict);
			this.groupBox9.Controls.Add(this.checkBoxAIBoatsPredictPoints);
			this.groupBox9.Controls.Add(this.checkBoxAIBoatsPredict);
			this.groupBox9.Controls.Add(this.checkBoxAIHelicoptersPredictPoints);
			this.groupBox9.Controls.Add(this.checkBoxAIHelicoptersPredict);
			this.groupBox9.Controls.Add(this.checkBoxAIAircraftsPredictPoints);
			this.groupBox9.Controls.Add(this.checkBoxAIAircraftsPredict);
			this.groupBox9.Controls.Add(this.label22);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIGroudUnitsRadius);
			this.groupBox9.Controls.Add(this.label23);
			this.groupBox9.Controls.Add(this.label24);
			this.groupBox9.Controls.Add(this.label25);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIBoatsRadius);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIHelicoptersRadius);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIAircraftsRadius);
			this.groupBox9.Controls.Add(this.label21);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIGroudUnitsInterval);
			this.groupBox9.Controls.Add(this.checkBoxQueryAIGroudUnits);
			this.groupBox9.Controls.Add(this.label13);
			this.groupBox9.Controls.Add(this.label12);
			this.groupBox9.Controls.Add(this.label11);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIBoatsInterval);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIHelicoptersInterval);
			this.groupBox9.Controls.Add(this.numericUpDownQueryAIAircraftsInterval);
			this.groupBox9.Controls.Add(this.checkBoxQueryAIBoats);
			this.groupBox9.Controls.Add(this.checkBoxQueryAIHelicopters);
			this.groupBox9.Controls.Add(this.checkBoxQueryAIAircrafts);
			this.groupBox9.Controls.Add(this.checkBoxQueryAIObjects);
			this.groupBox9.Location = new System.Drawing.Point(14, 12);
			this.groupBox9.Name = "groupBox9";
			this.groupBox9.Size = new System.Drawing.Size(367, 354);
			this.groupBox9.TabIndex = 0;
			this.groupBox9.TabStop = false;
			this.groupBox9.Text = "Query Settings";
			// 
			// checkBoxAIGroundPredictPoints
			// 
			this.checkBoxAIGroundPredictPoints.AutoSize = true;
			this.checkBoxAIGroundPredictPoints.Location = new System.Drawing.Point(55, 321);
			this.checkBoxAIGroundPredictPoints.Name = "checkBoxAIGroundPredictPoints";
			this.checkBoxAIGroundPredictPoints.Size = new System.Drawing.Size(133, 17);
			this.checkBoxAIGroundPredictPoints.TabIndex = 65;
			this.checkBoxAIGroundPredictPoints.Text = "Show prediction points";
			this.checkBoxAIGroundPredictPoints.UseVisualStyleBackColor = true;
			this.checkBoxAIGroundPredictPoints.CheckedChanged += new System.EventHandler(this.checkBoxAIGroundPredictPoints_CheckedChanged);
			// 
			// checkBoxAIGroundPredict
			// 
			this.checkBoxAIGroundPredict.AutoSize = true;
			this.checkBoxAIGroundPredict.Location = new System.Drawing.Point(34, 298);
			this.checkBoxAIGroundPredict.Name = "checkBoxAIGroundPredict";
			this.checkBoxAIGroundPredict.Size = new System.Drawing.Size(154, 17);
			this.checkBoxAIGroundPredict.TabIndex = 64;
			this.checkBoxAIGroundPredict.Text = "Calculate course prediction";
			this.checkBoxAIGroundPredict.UseVisualStyleBackColor = true;
			this.checkBoxAIGroundPredict.CheckedChanged += new System.EventHandler(this.checkBoxAIGroundPredict_CheckedChanged);
			this.checkBoxAIGroundPredict.EnabledChanged += new System.EventHandler(this.checkBoxAIGroundPredict_EnabledChanged);
			// 
			// checkBoxAIBoatsPredictPoints
			// 
			this.checkBoxAIBoatsPredictPoints.AutoSize = true;
			this.checkBoxAIBoatsPredictPoints.Location = new System.Drawing.Point(55, 250);
			this.checkBoxAIBoatsPredictPoints.Name = "checkBoxAIBoatsPredictPoints";
			this.checkBoxAIBoatsPredictPoints.Size = new System.Drawing.Size(133, 17);
			this.checkBoxAIBoatsPredictPoints.TabIndex = 63;
			this.checkBoxAIBoatsPredictPoints.Text = "Show prediction points";
			this.checkBoxAIBoatsPredictPoints.UseVisualStyleBackColor = true;
			this.checkBoxAIBoatsPredictPoints.CheckedChanged += new System.EventHandler(this.checkBoxAIBoatsPredictPoints_CheckedChanged);
			// 
			// checkBoxAIBoatsPredict
			// 
			this.checkBoxAIBoatsPredict.AutoSize = true;
			this.checkBoxAIBoatsPredict.Location = new System.Drawing.Point(34, 227);
			this.checkBoxAIBoatsPredict.Name = "checkBoxAIBoatsPredict";
			this.checkBoxAIBoatsPredict.Size = new System.Drawing.Size(154, 17);
			this.checkBoxAIBoatsPredict.TabIndex = 62;
			this.checkBoxAIBoatsPredict.Text = "Calculate course prediction";
			this.checkBoxAIBoatsPredict.UseVisualStyleBackColor = true;
			this.checkBoxAIBoatsPredict.CheckedChanged += new System.EventHandler(this.checkBoxAIBoatsPredict_CheckedChanged);
			this.checkBoxAIBoatsPredict.EnabledChanged += new System.EventHandler(this.checkBoxAIBoatsPredict_EnabledChanged);
			// 
			// checkBoxAIHelicoptersPredictPoints
			// 
			this.checkBoxAIHelicoptersPredictPoints.AutoSize = true;
			this.checkBoxAIHelicoptersPredictPoints.Location = new System.Drawing.Point(55, 179);
			this.checkBoxAIHelicoptersPredictPoints.Name = "checkBoxAIHelicoptersPredictPoints";
			this.checkBoxAIHelicoptersPredictPoints.Size = new System.Drawing.Size(133, 17);
			this.checkBoxAIHelicoptersPredictPoints.TabIndex = 61;
			this.checkBoxAIHelicoptersPredictPoints.Text = "Show prediction points";
			this.checkBoxAIHelicoptersPredictPoints.UseVisualStyleBackColor = true;
			this.checkBoxAIHelicoptersPredictPoints.CheckedChanged += new System.EventHandler(this.checkBoxAIHelicoptersPredictPoints_CheckedChanged);
			// 
			// checkBoxAIHelicoptersPredict
			// 
			this.checkBoxAIHelicoptersPredict.AutoSize = true;
			this.checkBoxAIHelicoptersPredict.Location = new System.Drawing.Point(34, 156);
			this.checkBoxAIHelicoptersPredict.Name = "checkBoxAIHelicoptersPredict";
			this.checkBoxAIHelicoptersPredict.Size = new System.Drawing.Size(154, 17);
			this.checkBoxAIHelicoptersPredict.TabIndex = 60;
			this.checkBoxAIHelicoptersPredict.Text = "Calculate course prediction";
			this.checkBoxAIHelicoptersPredict.UseVisualStyleBackColor = true;
			this.checkBoxAIHelicoptersPredict.CheckedChanged += new System.EventHandler(this.checkBoxAIHelicoptersPredict_CheckedChanged);
			this.checkBoxAIHelicoptersPredict.EnabledChanged += new System.EventHandler(this.checkBoxAIHelicoptersPredict_EnabledChanged);
			// 
			// checkBoxAIAircraftsPredictPoints
			// 
			this.checkBoxAIAircraftsPredictPoints.AutoSize = true;
			this.checkBoxAIAircraftsPredictPoints.Location = new System.Drawing.Point(55, 108);
			this.checkBoxAIAircraftsPredictPoints.Name = "checkBoxAIAircraftsPredictPoints";
			this.checkBoxAIAircraftsPredictPoints.Size = new System.Drawing.Size(133, 17);
			this.checkBoxAIAircraftsPredictPoints.TabIndex = 59;
			this.checkBoxAIAircraftsPredictPoints.Text = "Show prediction points";
			this.checkBoxAIAircraftsPredictPoints.UseVisualStyleBackColor = true;
			this.checkBoxAIAircraftsPredictPoints.CheckedChanged += new System.EventHandler(this.checkBoxAIAircraftsPredictPoints_CheckedChanged);
			// 
			// checkBoxAIAircraftsPredict
			// 
			this.checkBoxAIAircraftsPredict.AutoSize = true;
			this.checkBoxAIAircraftsPredict.Location = new System.Drawing.Point(34, 85);
			this.checkBoxAIAircraftsPredict.Name = "checkBoxAIAircraftsPredict";
			this.checkBoxAIAircraftsPredict.Size = new System.Drawing.Size(154, 17);
			this.checkBoxAIAircraftsPredict.TabIndex = 58;
			this.checkBoxAIAircraftsPredict.Text = "Calculate course prediction";
			this.checkBoxAIAircraftsPredict.UseVisualStyleBackColor = true;
			this.checkBoxAIAircraftsPredict.CheckedChanged += new System.EventHandler(this.checkBoxAIAircraftsPredict_CheckedChanged);
			this.checkBoxAIAircraftsPredict.EnabledChanged += new System.EventHandler(this.checkBoxAIAircraftsPredict_EnabledChanged);
			// 
			// label22
			// 
			this.label22.AutoSize = true;
			this.label22.Location = new System.Drawing.Point(300, 274);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(49, 13);
			this.label22.TabIndex = 57;
			this.label22.Text = "m radius.";
			// 
			// numericUpDownQueryAIGroudUnitsRadius
			// 
			this.numericUpDownQueryAIGroudUnitsRadius.Location = new System.Drawing.Point(231, 272);
			this.numericUpDownQueryAIGroudUnitsRadius.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsRadius.Name = "numericUpDownQueryAIGroudUnitsRadius";
			this.numericUpDownQueryAIGroudUnitsRadius.Size = new System.Drawing.Size(66, 20);
			this.numericUpDownQueryAIGroudUnitsRadius.TabIndex = 56;
			this.numericUpDownQueryAIGroudUnitsRadius.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsRadius.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIGroudUnitsRadius_ValueChanged);
			// 
			// label23
			// 
			this.label23.AutoSize = true;
			this.label23.Location = new System.Drawing.Point(300, 203);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(49, 13);
			this.label23.TabIndex = 55;
			this.label23.Text = "m radius.";
			// 
			// label24
			// 
			this.label24.AutoSize = true;
			this.label24.Location = new System.Drawing.Point(301, 132);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(49, 13);
			this.label24.TabIndex = 54;
			this.label24.Text = "m radius.";
			// 
			// label25
			// 
			this.label25.AutoSize = true;
			this.label25.Location = new System.Drawing.Point(301, 61);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(49, 13);
			this.label25.TabIndex = 53;
			this.label25.Text = "m radius.";
			// 
			// numericUpDownQueryAIBoatsRadius
			// 
			this.numericUpDownQueryAIBoatsRadius.Location = new System.Drawing.Point(231, 201);
			this.numericUpDownQueryAIBoatsRadius.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsRadius.Name = "numericUpDownQueryAIBoatsRadius";
			this.numericUpDownQueryAIBoatsRadius.Size = new System.Drawing.Size(66, 20);
			this.numericUpDownQueryAIBoatsRadius.TabIndex = 52;
			this.numericUpDownQueryAIBoatsRadius.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsRadius.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIBoatsRadius_ValueChanged);
			// 
			// numericUpDownQueryAIHelicoptersRadius
			// 
			this.numericUpDownQueryAIHelicoptersRadius.Location = new System.Drawing.Point(232, 130);
			this.numericUpDownQueryAIHelicoptersRadius.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersRadius.Name = "numericUpDownQueryAIHelicoptersRadius";
			this.numericUpDownQueryAIHelicoptersRadius.Size = new System.Drawing.Size(66, 20);
			this.numericUpDownQueryAIHelicoptersRadius.TabIndex = 51;
			this.numericUpDownQueryAIHelicoptersRadius.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersRadius.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIHelicoptersRadius_ValueChanged);
			// 
			// numericUpDownQueryAIAircraftsRadius
			// 
			this.numericUpDownQueryAIAircraftsRadius.Location = new System.Drawing.Point(232, 59);
			this.numericUpDownQueryAIAircraftsRadius.Maximum = new decimal(new int[] {
            200000,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsRadius.Name = "numericUpDownQueryAIAircraftsRadius";
			this.numericUpDownQueryAIAircraftsRadius.Size = new System.Drawing.Size(66, 20);
			this.numericUpDownQueryAIAircraftsRadius.TabIndex = 50;
			this.numericUpDownQueryAIAircraftsRadius.Value = new decimal(new int[] {
            100000,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsRadius.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIAircraftsRadius_ValueChanged);
			// 
			// label21
			// 
			this.label21.AutoSize = true;
			this.label21.Location = new System.Drawing.Point(193, 274);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(34, 13);
			this.label21.TabIndex = 49;
			this.label21.Text = "ms, in";
			// 
			// numericUpDownQueryAIGroudUnitsInterval
			// 
			this.numericUpDownQueryAIGroudUnitsInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsInterval.Location = new System.Drawing.Point(134, 272);
			this.numericUpDownQueryAIGroudUnitsInterval.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsInterval.Name = "numericUpDownQueryAIGroudUnitsInterval";
			this.numericUpDownQueryAIGroudUnitsInterval.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryAIGroudUnitsInterval.TabIndex = 48;
			this.numericUpDownQueryAIGroudUnitsInterval.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
			this.numericUpDownQueryAIGroudUnitsInterval.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIGroudUnitsInterval_ValueChanged);
			// 
			// checkBoxQueryAIGroudUnits
			// 
			this.checkBoxQueryAIGroudUnits.AutoSize = true;
			this.checkBoxQueryAIGroudUnits.Checked = true;
			this.checkBoxQueryAIGroudUnits.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxQueryAIGroudUnits.Location = new System.Drawing.Point(13, 273);
			this.checkBoxQueryAIGroudUnits.Name = "checkBoxQueryAIGroudUnits";
			this.checkBoxQueryAIGroudUnits.Size = new System.Drawing.Size(120, 17);
			this.checkBoxQueryAIGroudUnits.TabIndex = 47;
			this.checkBoxQueryAIGroudUnits.Text = "Ground Units, every";
			this.checkBoxQueryAIGroudUnits.UseVisualStyleBackColor = true;
			this.checkBoxQueryAIGroudUnits.CheckedChanged += new System.EventHandler(this.checkBoxQueryAIGroudUnits_CheckedChanged);
			this.checkBoxQueryAIGroudUnits.EnabledChanged += new System.EventHandler(this.checkBoxQueryAIGroudUnits_EnabledChanged);
			// 
			// label13
			// 
			this.label13.AutoSize = true;
			this.label13.Location = new System.Drawing.Point(193, 203);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(34, 13);
			this.label13.TabIndex = 46;
			this.label13.Text = "ms, in";
			// 
			// label12
			// 
			this.label12.AutoSize = true;
			this.label12.Location = new System.Drawing.Point(194, 132);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(34, 13);
			this.label12.TabIndex = 45;
			this.label12.Text = "ms, in";
			// 
			// label11
			// 
			this.label11.AutoSize = true;
			this.label11.Location = new System.Drawing.Point(194, 61);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(34, 13);
			this.label11.TabIndex = 44;
			this.label11.Text = "ms, in";
			// 
			// numericUpDownQueryAIBoatsInterval
			// 
			this.numericUpDownQueryAIBoatsInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsInterval.Location = new System.Drawing.Point(134, 201);
			this.numericUpDownQueryAIBoatsInterval.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsInterval.Name = "numericUpDownQueryAIBoatsInterval";
			this.numericUpDownQueryAIBoatsInterval.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryAIBoatsInterval.TabIndex = 43;
			this.numericUpDownQueryAIBoatsInterval.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
			this.numericUpDownQueryAIBoatsInterval.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIBoatsInterval_ValueChanged);
			// 
			// numericUpDownQueryAIHelicoptersInterval
			// 
			this.numericUpDownQueryAIHelicoptersInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersInterval.Location = new System.Drawing.Point(135, 130);
			this.numericUpDownQueryAIHelicoptersInterval.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersInterval.Name = "numericUpDownQueryAIHelicoptersInterval";
			this.numericUpDownQueryAIHelicoptersInterval.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryAIHelicoptersInterval.TabIndex = 42;
			this.numericUpDownQueryAIHelicoptersInterval.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
			this.numericUpDownQueryAIHelicoptersInterval.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIHelicoptersInterval_ValueChanged);
			// 
			// numericUpDownQueryAIAircraftsInterval
			// 
			this.numericUpDownQueryAIAircraftsInterval.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsInterval.Location = new System.Drawing.Point(135, 59);
			this.numericUpDownQueryAIAircraftsInterval.Maximum = new decimal(new int[] {
            3600000,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsInterval.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsInterval.Name = "numericUpDownQueryAIAircraftsInterval";
			this.numericUpDownQueryAIAircraftsInterval.Size = new System.Drawing.Size(59, 20);
			this.numericUpDownQueryAIAircraftsInterval.TabIndex = 41;
			this.numericUpDownQueryAIAircraftsInterval.Value = new decimal(new int[] {
            2500,
            0,
            0,
            0});
			this.numericUpDownQueryAIAircraftsInterval.ValueChanged += new System.EventHandler(this.numericUpDownQueryAIAircraftsInterval_ValueChanged);
			// 
			// checkBoxQueryAIBoats
			// 
			this.checkBoxQueryAIBoats.AutoSize = true;
			this.checkBoxQueryAIBoats.Checked = true;
			this.checkBoxQueryAIBoats.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxQueryAIBoats.Location = new System.Drawing.Point(13, 202);
			this.checkBoxQueryAIBoats.Name = "checkBoxQueryAIBoats";
			this.checkBoxQueryAIBoats.Size = new System.Drawing.Size(85, 17);
			this.checkBoxQueryAIBoats.TabIndex = 40;
			this.checkBoxQueryAIBoats.Text = "Boats, every";
			this.checkBoxQueryAIBoats.UseVisualStyleBackColor = true;
			this.checkBoxQueryAIBoats.CheckedChanged += new System.EventHandler(this.checkBoxQueryAIBoats_CheckedChanged);
			this.checkBoxQueryAIBoats.EnabledChanged += new System.EventHandler(this.checkBoxQueryAIBoats_EnabledChanged);
			// 
			// checkBoxQueryAIHelicopters
			// 
			this.checkBoxQueryAIHelicopters.AutoSize = true;
			this.checkBoxQueryAIHelicopters.Checked = true;
			this.checkBoxQueryAIHelicopters.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxQueryAIHelicopters.Location = new System.Drawing.Point(14, 131);
			this.checkBoxQueryAIHelicopters.Name = "checkBoxQueryAIHelicopters";
			this.checkBoxQueryAIHelicopters.Size = new System.Drawing.Size(111, 17);
			this.checkBoxQueryAIHelicopters.TabIndex = 39;
			this.checkBoxQueryAIHelicopters.Text = "Helicopters, every";
			this.checkBoxQueryAIHelicopters.UseVisualStyleBackColor = true;
			this.checkBoxQueryAIHelicopters.CheckedChanged += new System.EventHandler(this.checkBoxQueryAIHelicopters_CheckedChanged);
			this.checkBoxQueryAIHelicopters.EnabledChanged += new System.EventHandler(this.checkBoxQueryAIHelicopters_EnabledChanged);
			// 
			// checkBoxQueryAIAircrafts
			// 
			this.checkBoxQueryAIAircrafts.AutoSize = true;
			this.checkBoxQueryAIAircrafts.Checked = true;
			this.checkBoxQueryAIAircrafts.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxQueryAIAircrafts.Location = new System.Drawing.Point(14, 60);
			this.checkBoxQueryAIAircrafts.Name = "checkBoxQueryAIAircrafts";
			this.checkBoxQueryAIAircrafts.Size = new System.Drawing.Size(96, 17);
			this.checkBoxQueryAIAircrafts.TabIndex = 38;
			this.checkBoxQueryAIAircrafts.Text = "Aircrafts, every";
			this.checkBoxQueryAIAircrafts.UseVisualStyleBackColor = true;
			this.checkBoxQueryAIAircrafts.CheckedChanged += new System.EventHandler(this.checkBoxQueryAIAircrafts_CheckedChanged);
			this.checkBoxQueryAIAircrafts.EnabledChanged += new System.EventHandler(this.checkBoxQueryAIAircrafts_EnabledChanged);
			// 
			// checkBoxQueryAIObjects
			// 
			this.checkBoxQueryAIObjects.AutoSize = true;
			this.checkBoxQueryAIObjects.Checked = true;
			this.checkBoxQueryAIObjects.CheckState = System.Windows.Forms.CheckState.Checked;
			this.checkBoxQueryAIObjects.Location = new System.Drawing.Point(15, 28);
			this.checkBoxQueryAIObjects.Name = "checkBoxQueryAIObjects";
			this.checkBoxQueryAIObjects.Size = new System.Drawing.Size(104, 17);
			this.checkBoxQueryAIObjects.TabIndex = 37;
			this.checkBoxQueryAIObjects.Text = "Query AI objects";
			this.checkBoxQueryAIObjects.UseVisualStyleBackColor = true;
			this.checkBoxQueryAIObjects.CheckedChanged += new System.EventHandler(this.checkBoxQueryAIObjects_CheckedChanged);
			// 
			// tabPage9
			// 
			this.tabPage9.Controls.Add(this.groupBox10);
			this.tabPage9.Location = new System.Drawing.Point(4, 22);
			this.tabPage9.Name = "tabPage9";
			this.tabPage9.Size = new System.Drawing.Size(397, 372);
			this.tabPage9.TabIndex = 2;
			this.tabPage9.Text = "Connection";
			this.tabPage9.UseVisualStyleBackColor = true;
			// 
			// groupBox10
			// 
			this.groupBox10.Controls.Add(this.textBox3);
			this.groupBox10.Controls.Add(this.label33);
			this.groupBox10.Controls.Add(this.textBox1);
			this.groupBox10.Controls.Add(this.label32);
			this.groupBox10.Controls.Add(this.label31);
			this.groupBox10.Controls.Add(this.comboBox1);
			this.groupBox10.Controls.Add(this.label30);
			this.groupBox10.Controls.Add(this.radioButton7);
			this.groupBox10.Controls.Add(this.label29);
			this.groupBox10.Controls.Add(this.radioButton6);
			this.groupBox10.Location = new System.Drawing.Point(12, 12);
			this.groupBox10.Name = "groupBox10";
			this.groupBox10.Size = new System.Drawing.Size(367, 349);
			this.groupBox10.TabIndex = 8;
			this.groupBox10.TabStop = false;
			this.groupBox10.Text = "FSX Connection Settings";
			// 
			// textBox3
			// 
			this.textBox3.Location = new System.Drawing.Point(30, 272);
			this.textBox3.Name = "textBox3";
			this.textBox3.Size = new System.Drawing.Size(298, 20);
			this.textBox3.TabIndex = 13;
			this.textBox3.Text = "9017";
			this.textBox3.TextChanged += new System.EventHandler(this.textBox3_TextChanged);
			// 
			// label33
			// 
			this.label33.AutoSize = true;
			this.label33.Location = new System.Drawing.Point(27, 255);
			this.label33.Name = "label33";
			this.label33.Size = new System.Drawing.Size(195, 13);
			this.label33.TabIndex = 12;
			this.label33.Text = "Port the remote computer is listening on:";
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(30, 221);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(298, 20);
			this.textBox1.TabIndex = 11;
			this.textBox1.Text = "192.168.0.1";
			this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label32
			// 
			this.label32.AutoSize = true;
			this.label32.Location = new System.Drawing.Point(27, 204);
			this.label32.Name = "label32";
			this.label32.Size = new System.Drawing.Size(301, 13);
			this.label32.TabIndex = 10;
			this.label32.Text = "Address or host name of the computer running Flight Simulator:";
			// 
			// label31
			// 
			this.label31.AutoSize = true;
			this.label31.Location = new System.Drawing.Point(27, 155);
			this.label31.Name = "label31";
			this.label31.Size = new System.Drawing.Size(49, 13);
			this.label31.TabIndex = 9;
			this.label31.Text = "Protocol:";
			// 
			// comboBox1
			// 
			this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Items.AddRange(new object[] {
            "IPv4",
            "IPv6",
            "Pipe"});
			this.comboBox1.Location = new System.Drawing.Point(30, 171);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(298, 21);
			this.comboBox1.TabIndex = 8;
			this.comboBox1.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
			// 
			// label30
			// 
			this.label30.AutoSize = true;
			this.label30.Location = new System.Drawing.Point(16, 132);
			this.label30.Name = "label30";
			this.label30.Size = new System.Drawing.Size(163, 13);
			this.label30.TabIndex = 7;
			this.label30.Text = "Specify your connection settings:";
			// 
			// radioButton7
			// 
			this.radioButton7.AutoSize = true;
			this.radioButton7.Checked = true;
			this.radioButton7.Location = new System.Drawing.Point(30, 51);
			this.radioButton7.Name = "radioButton7";
			this.radioButton7.Size = new System.Drawing.Size(245, 30);
			this.radioButton7.TabIndex = 6;
			this.radioButton7.TabStop = true;
			this.radioButton7.Text = "FSXGET and Flight Simulator will BOTH run on\r\nTHIS COMPUTER";
			this.radioButton7.UseVisualStyleBackColor = true;
			this.radioButton7.CheckedChanged += new System.EventHandler(this.radioButton7_CheckedChanged);
			// 
			// label29
			// 
			this.label29.AutoSize = true;
			this.label29.Location = new System.Drawing.Point(16, 26);
			this.label29.Name = "label29";
			this.label29.Size = new System.Drawing.Size(127, 13);
			this.label29.TabIndex = 5;
			this.label29.Text = "Select your configuration:";
			// 
			// radioButton6
			// 
			this.radioButton6.AutoSize = true;
			this.radioButton6.Location = new System.Drawing.Point(30, 87);
			this.radioButton6.Name = "radioButton6";
			this.radioButton6.Size = new System.Drawing.Size(272, 30);
			this.radioButton6.TabIndex = 0;
			this.radioButton6.Text = "FSXGET and Google Earth will run on this computer,\r\nFlight Simulator will run on " +
				"ANOTHER COMPUTER";
			this.radioButton6.UseVisualStyleBackColor = true;
			this.radioButton6.CheckedChanged += new System.EventHandler(this.radioButton6_CheckedChanged);
			// 
			// tabPage4
			// 
			this.tabPage4.Controls.Add(this.groupBox1);
			this.tabPage4.Location = new System.Drawing.Point(4, 22);
			this.tabPage4.Name = "tabPage4";
			this.tabPage4.Size = new System.Drawing.Size(430, 422);
			this.tabPage4.TabIndex = 2;
			this.tabPage4.Text = "Google Earth";
			this.tabPage4.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.label20);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshUserPrediction);
			this.groupBox1.Controls.Add(this.label27);
			this.groupBox1.Controls.Add(this.label19);
			this.groupBox1.Controls.Add(this.label18);
			this.groupBox1.Controls.Add(this.label17);
			this.groupBox1.Controls.Add(this.label16);
			this.groupBox1.Controls.Add(this.label15);
			this.groupBox1.Controls.Add(this.label14);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshAIGroundUnits);
			this.groupBox1.Controls.Add(this.label7);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshAIBoats);
			this.groupBox1.Controls.Add(this.label8);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshAIHelicopter);
			this.groupBox1.Controls.Add(this.label6);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshAIAircrafts);
			this.groupBox1.Controls.Add(this.label5);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshUserPath);
			this.groupBox1.Controls.Add(this.numericUpDownRefreshUserAircraft);
			this.groupBox1.Controls.Add(this.label4);
			this.groupBox1.Controls.Add(this.label3);
			this.groupBox1.Location = new System.Drawing.Point(12, 15);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(406, 247);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Refresh Rates";
			// 
			// label20
			// 
			this.label20.AutoSize = true;
			this.label20.Location = new System.Drawing.Point(369, 91);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(12, 13);
			this.label20.TabIndex = 21;
			this.label20.Text = "s";
			// 
			// numericUpDownRefreshUserPrediction
			// 
			this.numericUpDownRefreshUserPrediction.Location = new System.Drawing.Point(300, 89);
			this.numericUpDownRefreshUserPrediction.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshUserPrediction.Name = "numericUpDownRefreshUserPrediction";
			this.numericUpDownRefreshUserPrediction.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshUserPrediction.TabIndex = 20;
			this.numericUpDownRefreshUserPrediction.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.numericUpDownRefreshUserPrediction.ValueChanged += new System.EventHandler(this.numericUpDownRefreshUserPrediction_ValueChanged);
			// 
			// label27
			// 
			this.label27.AutoSize = true;
			this.label27.Location = new System.Drawing.Point(18, 91);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(113, 13);
			this.label27.TabIndex = 19;
			this.label27.Text = "User course prediction";
			// 
			// label19
			// 
			this.label19.AutoSize = true;
			this.label19.Location = new System.Drawing.Point(367, 206);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(12, 13);
			this.label19.TabIndex = 18;
			this.label19.Text = "s";
			// 
			// label18
			// 
			this.label18.AutoSize = true;
			this.label18.Location = new System.Drawing.Point(367, 180);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(12, 13);
			this.label18.TabIndex = 17;
			this.label18.Text = "s";
			// 
			// label17
			// 
			this.label17.AutoSize = true;
			this.label17.Location = new System.Drawing.Point(369, 154);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(12, 13);
			this.label17.TabIndex = 16;
			this.label17.Text = "s";
			// 
			// label16
			// 
			this.label16.AutoSize = true;
			this.label16.Location = new System.Drawing.Point(369, 128);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(12, 13);
			this.label16.TabIndex = 15;
			this.label16.Text = "s";
			// 
			// label15
			// 
			this.label15.AutoSize = true;
			this.label15.Location = new System.Drawing.Point(369, 65);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(12, 13);
			this.label15.TabIndex = 14;
			this.label15.Text = "s";
			// 
			// label14
			// 
			this.label14.AutoSize = true;
			this.label14.Location = new System.Drawing.Point(369, 28);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(12, 13);
			this.label14.TabIndex = 13;
			this.label14.Text = "s";
			// 
			// numericUpDownRefreshAIGroundUnits
			// 
			this.numericUpDownRefreshAIGroundUnits.Location = new System.Drawing.Point(300, 204);
			this.numericUpDownRefreshAIGroundUnits.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshAIGroundUnits.Name = "numericUpDownRefreshAIGroundUnits";
			this.numericUpDownRefreshAIGroundUnits.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshAIGroundUnits.TabIndex = 12;
			this.numericUpDownRefreshAIGroundUnits.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numericUpDownRefreshAIGroundUnits.ValueChanged += new System.EventHandler(this.numericUpDownRefreshAIGroundUnits_ValueChanged);
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(18, 206);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(95, 13);
			this.label7.TabIndex = 11;
			this.label7.Text = "AI ground vehicles";
			// 
			// numericUpDownRefreshAIBoats
			// 
			this.numericUpDownRefreshAIBoats.Location = new System.Drawing.Point(300, 178);
			this.numericUpDownRefreshAIBoats.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshAIBoats.Name = "numericUpDownRefreshAIBoats";
			this.numericUpDownRefreshAIBoats.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshAIBoats.TabIndex = 10;
			this.numericUpDownRefreshAIBoats.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numericUpDownRefreshAIBoats.ValueChanged += new System.EventHandler(this.numericUpDownRefreshAIBoats_ValueChanged);
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(18, 180);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(46, 13);
			this.label8.TabIndex = 9;
			this.label8.Text = "AI boats";
			// 
			// numericUpDownRefreshAIHelicopter
			// 
			this.numericUpDownRefreshAIHelicopter.Location = new System.Drawing.Point(300, 152);
			this.numericUpDownRefreshAIHelicopter.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshAIHelicopter.Name = "numericUpDownRefreshAIHelicopter";
			this.numericUpDownRefreshAIHelicopter.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshAIHelicopter.TabIndex = 8;
			this.numericUpDownRefreshAIHelicopter.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numericUpDownRefreshAIHelicopter.ValueChanged += new System.EventHandler(this.numericUpDownRefreshAIHelicopter_ValueChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(18, 154);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(66, 13);
			this.label6.TabIndex = 7;
			this.label6.Text = "AI helicopter";
			// 
			// numericUpDownRefreshAIAircrafts
			// 
			this.numericUpDownRefreshAIAircrafts.Location = new System.Drawing.Point(300, 126);
			this.numericUpDownRefreshAIAircrafts.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshAIAircrafts.Name = "numericUpDownRefreshAIAircrafts";
			this.numericUpDownRefreshAIAircrafts.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshAIAircrafts.TabIndex = 6;
			this.numericUpDownRefreshAIAircrafts.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
			this.numericUpDownRefreshAIAircrafts.ValueChanged += new System.EventHandler(this.numericUpDownRefreshAIAircrafts_ValueChanged);
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(18, 128);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(52, 13);
			this.label5.TabIndex = 5;
			this.label5.Text = "AI aircraft";
			// 
			// numericUpDownRefreshUserPath
			// 
			this.numericUpDownRefreshUserPath.Location = new System.Drawing.Point(300, 63);
			this.numericUpDownRefreshUserPath.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshUserPath.Name = "numericUpDownRefreshUserPath";
			this.numericUpDownRefreshUserPath.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshUserPath.TabIndex = 4;
			this.numericUpDownRefreshUserPath.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.numericUpDownRefreshUserPath.ValueChanged += new System.EventHandler(this.numericUpDownRefreshUserPath_ValueChanged);
			// 
			// numericUpDownRefreshUserAircraft
			// 
			this.numericUpDownRefreshUserAircraft.Location = new System.Drawing.Point(300, 26);
			this.numericUpDownRefreshUserAircraft.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
			this.numericUpDownRefreshUserAircraft.Name = "numericUpDownRefreshUserAircraft";
			this.numericUpDownRefreshUserAircraft.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownRefreshUserAircraft.TabIndex = 3;
			this.numericUpDownRefreshUserAircraft.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
			this.numericUpDownRefreshUserAircraft.ValueChanged += new System.EventHandler(this.numericUpDownRefreshUserAircraft_ValueChanged);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(18, 65);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(88, 13);
			this.label4.TabIndex = 1;
			this.label4.Text = "User aircraft path";
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(18, 28);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "User aircraft";
			// 
			// tabPage8
			// 
			this.tabPage8.Controls.Add(this.groupBox2);
			this.tabPage8.Location = new System.Drawing.Point(4, 22);
			this.tabPage8.Name = "tabPage8";
			this.tabPage8.Size = new System.Drawing.Size(430, 422);
			this.tabPage8.TabIndex = 5;
			this.tabPage8.Text = "Server";
			this.tabPage8.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.panel2);
			this.groupBox2.Controls.Add(this.panel1);
			this.groupBox2.Controls.Add(this.label28);
			this.groupBox2.Controls.Add(this.label2);
			this.groupBox2.Controls.Add(this.label1);
			this.groupBox2.Controls.Add(this.numericUpDownServerPort);
			this.groupBox2.Location = new System.Drawing.Point(12, 14);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(406, 328);
			this.groupBox2.TabIndex = 1;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Server Settings";
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.radioButtonAccessLocalOnly);
			this.panel2.Controls.Add(this.radioButtonAccessRemote);
			this.panel2.Location = new System.Drawing.Point(28, 267);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(364, 51);
			this.panel2.TabIndex = 12;
			// 
			// radioButtonAccessLocalOnly
			// 
			this.radioButtonAccessLocalOnly.AutoSize = true;
			this.radioButtonAccessLocalOnly.Checked = true;
			this.radioButtonAccessLocalOnly.Location = new System.Drawing.Point(3, 3);
			this.radioButtonAccessLocalOnly.Name = "radioButtonAccessLocalOnly";
			this.radioButtonAccessLocalOnly.Size = new System.Drawing.Size(210, 17);
			this.radioButtonAccessLocalOnly.TabIndex = 9;
			this.radioButtonAccessLocalOnly.TabStop = true;
			this.radioButtonAccessLocalOnly.Text = "Accept only connections from localhost";
			this.radioButtonAccessLocalOnly.UseVisualStyleBackColor = true;
			this.radioButtonAccessLocalOnly.CheckedChanged += new System.EventHandler(this.radioButtonAccessLocalOnly_CheckedChanged);
			// 
			// radioButtonAccessRemote
			// 
			this.radioButtonAccessRemote.AutoSize = true;
			this.radioButtonAccessRemote.Location = new System.Drawing.Point(3, 26);
			this.radioButtonAccessRemote.Name = "radioButtonAccessRemote";
			this.radioButtonAccessRemote.Size = new System.Drawing.Size(216, 17);
			this.radioButtonAccessRemote.TabIndex = 10;
			this.radioButtonAccessRemote.Text = "Accept all incoming connection requests";
			this.radioButtonAccessRemote.UseVisualStyleBackColor = true;
			this.radioButtonAccessRemote.CheckedChanged += new System.EventHandler(this.radioButtonAccessRemote_CheckedChanged);
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.panel3);
			this.panel1.Controls.Add(this.radioButton1);
			this.panel1.Controls.Add(this.radioButton2);
			this.panel1.Controls.Add(this.radioButton3);
			this.panel1.Location = new System.Drawing.Point(28, 70);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(364, 176);
			this.panel1.TabIndex = 11;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.textBox2);
			this.panel3.Controls.Add(this.textBoxLocalPubPath);
			this.panel3.Controls.Add(this.radioButton5);
			this.panel3.Controls.Add(this.radioButton4);
			this.panel3.Enabled = false;
			this.panel3.Location = new System.Drawing.Point(20, 71);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(327, 102);
			this.panel3.TabIndex = 8;
			// 
			// textBox2
			// 
			this.textBox2.Location = new System.Drawing.Point(23, 73);
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(301, 20);
			this.textBox2.TabIndex = 3;
			// 
			// textBoxLocalPubPath
			// 
			this.textBoxLocalPubPath.Location = new System.Drawing.Point(23, 25);
			this.textBoxLocalPubPath.Name = "textBoxLocalPubPath";
			this.textBoxLocalPubPath.ReadOnly = true;
			this.textBoxLocalPubPath.Size = new System.Drawing.Size(301, 20);
			this.textBoxLocalPubPath.TabIndex = 2;
			// 
			// radioButton5
			// 
			this.radioButton5.AutoSize = true;
			this.radioButton5.Location = new System.Drawing.Point(4, 53);
			this.radioButton5.Name = "radioButton5";
			this.radioButton5.Size = new System.Drawing.Size(245, 17);
			this.radioButton5.TabIndex = 1;
			this.radioButton5.Text = "Use user defined path (e.g. Network Share, ...)";
			this.radioButton5.UseVisualStyleBackColor = true;
			// 
			// radioButton4
			// 
			this.radioButton4.AutoSize = true;
			this.radioButton4.Checked = true;
			this.radioButton4.Location = new System.Drawing.Point(4, 4);
			this.radioButton4.Name = "radioButton4";
			this.radioButton4.Size = new System.Drawing.Size(193, 17);
			this.radioButton4.TabIndex = 0;
			this.radioButton4.TabStop = true;
			this.radioButton4.Text = "Use local file path to ./pub directory";
			this.radioButton4.UseVisualStyleBackColor = true;
			// 
			// radioButton1
			// 
			this.radioButton1.AutoSize = true;
			this.radioButton1.Checked = true;
			this.radioButton1.Location = new System.Drawing.Point(3, 3);
			this.radioButton1.Name = "radioButton1";
			this.radioButton1.Size = new System.Drawing.Size(142, 17);
			this.radioButton1.TabIndex = 5;
			this.radioButton1.TabStop = true;
			this.radioButton1.Text = "Cache images in memory";
			this.radioButton1.UseVisualStyleBackColor = true;
			// 
			// radioButton2
			// 
			this.radioButton2.AutoSize = true;
			this.radioButton2.Enabled = false;
			this.radioButton2.Location = new System.Drawing.Point(3, 25);
			this.radioButton2.Name = "radioButton2";
			this.radioButton2.Size = new System.Drawing.Size(198, 17);
			this.radioButton2.TabIndex = 6;
			this.radioButton2.Text = "Load images from disc when needed";
			this.radioButton2.UseVisualStyleBackColor = true;
			// 
			// radioButton3
			// 
			this.radioButton3.AutoSize = true;
			this.radioButton3.Enabled = false;
			this.radioButton3.Location = new System.Drawing.Point(3, 47);
			this.radioButton3.Name = "radioButton3";
			this.radioButton3.Size = new System.Drawing.Size(311, 17);
			this.radioButton3.TabIndex = 7;
			this.radioButton3.Text = "Let Google Earth load the images itself, just pass the file path";
			this.radioButton3.UseVisualStyleBackColor = true;
			// 
			// label28
			// 
			this.label28.AutoSize = true;
			this.label28.Location = new System.Drawing.Point(15, 249);
			this.label28.Name = "label28";
			this.label28.Size = new System.Drawing.Size(74, 13);
			this.label28.TabIndex = 8;
			this.label28.Text = "Access Level:";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(15, 50);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(108, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "Image Serving Mode:";
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(15, 25);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(63, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "Server Port:";
			// 
			// numericUpDownServerPort
			// 
			this.numericUpDownServerPort.Location = new System.Drawing.Point(124, 23);
			this.numericUpDownServerPort.Maximum = new decimal(new int[] {
            65535,
            0,
            0,
            0});
			this.numericUpDownServerPort.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
			this.numericUpDownServerPort.Name = "numericUpDownServerPort";
			this.numericUpDownServerPort.Size = new System.Drawing.Size(67, 20);
			this.numericUpDownServerPort.TabIndex = 2;
			this.numericUpDownServerPort.Value = new decimal(new int[] {
            8080,
            0,
            0,
            0});
			this.numericUpDownServerPort.ValueChanged += new System.EventHandler(this.numericUpDownServerPort_ValueChanged);
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox6);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(430, 422);
			this.tabPage2.TabIndex = 3;
			this.tabPage2.Text = "Logging";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// groupBox6
			// 
			this.groupBox6.Controls.Add(this.checkBoxSubFoldersForLog);
			this.groupBox6.Controls.Add(this.checkBoxSaveLog);
			this.groupBox6.Location = new System.Drawing.Point(12, 13);
			this.groupBox6.Name = "groupBox6";
			this.groupBox6.Size = new System.Drawing.Size(404, 89);
			this.groupBox6.TabIndex = 2;
			this.groupBox6.TabStop = false;
			this.groupBox6.Text = "Log File Settings";
			// 
			// checkBoxSubFoldersForLog
			// 
			this.checkBoxSubFoldersForLog.AutoSize = true;
			this.checkBoxSubFoldersForLog.Enabled = false;
			this.checkBoxSubFoldersForLog.Location = new System.Drawing.Point(36, 51);
			this.checkBoxSubFoldersForLog.Name = "checkBoxSubFoldersForLog";
			this.checkBoxSubFoldersForLog.Size = new System.Drawing.Size(146, 17);
			this.checkBoxSubFoldersForLog.TabIndex = 1;
			this.checkBoxSubFoldersForLog.Text = "Create subfolders by date";
			this.checkBoxSubFoldersForLog.UseVisualStyleBackColor = true;
			// 
			// checkBoxSaveLog
			// 
			this.checkBoxSaveLog.AutoSize = true;
			this.checkBoxSaveLog.Enabled = false;
			this.checkBoxSaveLog.Location = new System.Drawing.Point(15, 28);
			this.checkBoxSaveLog.Name = "checkBoxSaveLog";
			this.checkBoxSaveLog.Size = new System.Drawing.Size(264, 17);
			this.checkBoxSaveLog.TabIndex = 0;
			this.checkBoxSaveLog.Text = "Save recorded data to file on disconnect from FSX";
			this.checkBoxSaveLog.UseVisualStyleBackColor = true;
			this.checkBoxSaveLog.CheckedChanged += new System.EventHandler(this.checkBoxSaveLog_CheckedChanged);
			// 
			// tabPage5
			// 
			this.tabPage5.Controls.Add(this.groupBox7);
			this.tabPage5.Location = new System.Drawing.Point(4, 22);
			this.tabPage5.Name = "tabPage5";
			this.tabPage5.Size = new System.Drawing.Size(430, 422);
			this.tabPage5.TabIndex = 4;
			this.tabPage5.Text = "About";
			this.tabPage5.UseVisualStyleBackColor = true;
			// 
			// groupBox7
			// 
			this.groupBox7.Controls.Add(this.linkLabel1);
			this.groupBox7.Controls.Add(this.labelVersion);
			this.groupBox7.Controls.Add(this.labelCopyright);
			this.groupBox7.Controls.Add(this.labelCompanyName);
			this.groupBox7.Controls.Add(this.labelProductName);
			this.groupBox7.Controls.Add(this.pictureBox1);
			this.groupBox7.Location = new System.Drawing.Point(12, 13);
			this.groupBox7.Name = "groupBox7";
			this.groupBox7.Size = new System.Drawing.Size(403, 396);
			this.groupBox7.TabIndex = 3;
			this.groupBox7.TabStop = false;
			// 
			// linkLabel1
			// 
			this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(6, 33);
			this.linkLabel1.Location = new System.Drawing.Point(16, 348);
			this.linkLabel1.Name = "linkLabel1";
			this.linkLabel1.Size = new System.Drawing.Size(372, 35);
			this.linkLabel1.TabIndex = 4;
			this.linkLabel1.TabStop = true;
			this.linkLabel1.Text = "Visit http://www.juergentreml.de/fsxget for more information.";
			this.linkLabel1.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
			this.linkLabel1.UseCompatibleTextRendering = true;
			this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
			// 
			// labelVersion
			// 
			this.labelVersion.AutoSize = true;
			this.labelVersion.Location = new System.Drawing.Point(107, 50);
			this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelVersion.Name = "labelVersion";
			this.labelVersion.Size = new System.Drawing.Size(42, 13);
			this.labelVersion.TabIndex = 23;
			this.labelVersion.Text = "Version";
			this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCopyright
			// 
			this.labelCopyright.AutoSize = true;
			this.labelCopyright.Location = new System.Drawing.Point(107, 83);
			this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCopyright.Name = "labelCopyright";
			this.labelCopyright.Size = new System.Drawing.Size(51, 13);
			this.labelCopyright.TabIndex = 24;
			this.labelCopyright.Text = "Copyright";
			this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelCompanyName
			// 
			this.labelCompanyName.AutoSize = true;
			this.labelCompanyName.Location = new System.Drawing.Point(107, 114);
			this.labelCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelCompanyName.Name = "labelCompanyName";
			this.labelCompanyName.Size = new System.Drawing.Size(82, 13);
			this.labelCompanyName.TabIndex = 25;
			this.labelCompanyName.Text = "Company Name";
			this.labelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// labelProductName
			// 
			this.labelProductName.AutoSize = true;
			this.labelProductName.Location = new System.Drawing.Point(107, 22);
			this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
			this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
			this.labelProductName.Name = "labelProductName";
			this.labelProductName.Size = new System.Drawing.Size(75, 13);
			this.labelProductName.TabIndex = 20;
			this.labelProductName.Text = "Product Name";
			this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pictureBox1
			// 
			this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
			this.pictureBox1.Location = new System.Drawing.Point(16, 22);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(69, 69);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// timerQueryUserPath
			// 
			this.timerQueryUserPath.Tick += new System.EventHandler(this.timerQueryUserPath_Tick);
			// 
			// timerQueryAIAircrafts
			// 
			this.timerQueryAIAircrafts.Tick += new System.EventHandler(this.timerQueryAIAircrafts_Tick);
			// 
			// timerQueryAIHelicopters
			// 
			this.timerQueryAIHelicopters.Tick += new System.EventHandler(this.timerQueryAIHelicopters_Tick);
			// 
			// timerQueryAIBoats
			// 
			this.timerQueryAIBoats.Tick += new System.EventHandler(this.timerQueryAIBoats_Tick);
			// 
			// timerQueryAIGroundUnits
			// 
			this.timerQueryAIGroundUnits.Tick += new System.EventHandler(this.timerQueryAIGroundUnits_Tick);
			// 
			// timerUserPrediction
			// 
			this.timerUserPrediction.Tick += new System.EventHandler(this.timerUserPrediction_Tick);
			// 
			// timerIPAddressRefresh
			// 
			this.timerIPAddressRefresh.Tick += new System.EventHandler(this.timerIPAddressRefresh_Tick);
			// 
			// saveFileDialogKMLFile
			// 
			this.saveFileDialogKMLFile.DefaultExt = "kml";
			this.saveFileDialogKMLFile.Filter = "Google Earth Files|*.kml";
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(462, 501);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.buttonCancel);
			this.Controls.Add(this.buttonOK);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "Form1";
			this.Text = "FSX Google Earth Tracker";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Shown += new System.EventHandler(this.Form1_Shown);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
			this.contextMenuStripNotifyIcon.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox8.ResumeLayout(false);
			this.groupBox8.PerformLayout();
			this.groupBox4.ResumeLayout(false);
			this.groupBox4.PerformLayout();
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.tabPage3.ResumeLayout(false);
			this.tabControl2.ResumeLayout(false);
			this.tabPage6.ResumeLayout(false);
			this.groupBoxUserAircraftPathPrediction.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.groupBox5.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownUserPathPrediction)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryUserPath)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryUserAircraft)).EndInit();
			this.tabPage7.ResumeLayout(false);
			this.groupBox9.ResumeLayout(false);
			this.groupBox9.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIGroudUnitsRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIBoatsRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIHelicoptersRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIAircraftsRadius)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIGroudUnitsInterval)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIBoatsInterval)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIHelicoptersInterval)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownQueryAIAircraftsInterval)).EndInit();
			this.tabPage9.ResumeLayout(false);
			this.groupBox10.ResumeLayout(false);
			this.groupBox10.PerformLayout();
			this.tabPage4.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.groupBox1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserPrediction)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIGroundUnits)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIBoats)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIHelicopter)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshAIAircrafts)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserPath)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownRefreshUserAircraft)).EndInit();
			this.tabPage8.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();
			this.panel2.ResumeLayout(false);
			this.panel2.PerformLayout();
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.panel3.ResumeLayout(false);
			this.panel3.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.numericUpDownServerPort)).EndInit();
			this.tabPage2.ResumeLayout(false);
			this.groupBox6.ResumeLayout(false);
			this.groupBox6.PerformLayout();
			this.tabPage5.ResumeLayout(false);
			this.groupBox7.ResumeLayout(false);
			this.groupBox7.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.NotifyIcon notifyIconMain;
		private System.Windows.Forms.ContextMenuStrip contextMenuStripNotifyIcon;
		private System.Windows.Forms.ToolStripMenuItem enableTrackerToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem showBalloonTipsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator test2ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
		private System.Windows.Forms.Button buttonCancel;
		private System.Windows.Forms.Button buttonOK;
		private System.Windows.Forms.Timer timerFSXConnect;
		private System.Windows.Forms.Timer timerQueryUserAircraft;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.TabPage tabPage4;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.NumericUpDown numericUpDownServerPort;
		private System.Windows.Forms.RadioButton radioButton3;
		private System.Windows.Forms.RadioButton radioButton2;
		private System.Windows.Forms.RadioButton radioButton1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshUserAircraft;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshUserPath;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshAIGroundUnits;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshAIBoats;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshAIHelicopter;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshAIAircrafts;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.CheckBox checkShowInfoBalloons;
		private System.Windows.Forms.CheckBox checkEnableOnStartup;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.ToolStripMenuItem pauseRecordingToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem clearUserAircraftPathToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
		private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
		private System.Windows.Forms.TabPage tabPage5;
		private System.Windows.Forms.GroupBox groupBox7;
		private System.Windows.Forms.PictureBox pictureBox1;
		private System.Windows.Forms.Label labelProductName;
		private System.Windows.Forms.Label labelVersion;
		private System.Windows.Forms.Label labelCopyright;
		private System.Windows.Forms.Label labelCompanyName;
		private System.Windows.Forms.LinkLabel linkLabel1;
		private System.Windows.Forms.CheckBox checkBoxUpdateCheck;
		private System.Windows.Forms.Timer timerQueryUserPath;
		private System.Windows.Forms.Timer timerQueryAIAircrafts;
		private System.Windows.Forms.Timer timerQueryAIHelicopters;
		private System.Windows.Forms.Timer timerQueryAIBoats;
		private System.Windows.Forms.Timer timerQueryAIGroundUnits;
		private System.Windows.Forms.ToolStripMenuItem runMicrosoftFlightSimulatorXToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem runGoogleEarthToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
		private System.Windows.Forms.GroupBox groupBox8;
		private System.Windows.Forms.CheckBox checkBoxLoadKMLFile;
		private System.Windows.Forms.TabControl tabControl2;
		private System.Windows.Forms.TabPage tabPage6;
		private System.Windows.Forms.TabPage tabPage7;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryUserPath;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryUserAircraft;
		private System.Windows.Forms.CheckBox checkQueryUserPath;
		private System.Windows.Forms.CheckBox checkQueryUserAircraft;
		private System.Windows.Forms.GroupBox groupBox6;
		private System.Windows.Forms.GroupBox groupBoxUserAircraftPathPrediction;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.NumericUpDown numericUpDownUserPathPrediction;
		private System.Windows.Forms.CheckBox checkBoxUserPathPrediction;
		private System.Windows.Forms.GroupBox groupBox9;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIGroudUnitsRadius;
		private System.Windows.Forms.Label label23;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIBoatsRadius;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIHelicoptersRadius;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIAircraftsRadius;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIGroudUnitsInterval;
		private System.Windows.Forms.CheckBox checkBoxQueryAIGroudUnits;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIBoatsInterval;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIHelicoptersInterval;
		private System.Windows.Forms.NumericUpDown numericUpDownQueryAIAircraftsInterval;
		private System.Windows.Forms.CheckBox checkBoxQueryAIBoats;
		private System.Windows.Forms.CheckBox checkBoxQueryAIHelicopters;
		private System.Windows.Forms.CheckBox checkBoxQueryAIAircrafts;
		private System.Windows.Forms.CheckBox checkBoxQueryAIObjects;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ListBox listBoxPathPrediction;
		private System.Windows.Forms.CheckBox checkBoxSaveLog;
		private System.Windows.Forms.CheckBox checkBoxSubFoldersForLog;
		private System.Windows.Forms.Timer timerUserPrediction;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.NumericUpDown numericUpDownRefreshUserPrediction;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.TabPage tabPage8;
		private System.Windows.Forms.RadioButton radioButtonAccessRemote;
		private System.Windows.Forms.RadioButton radioButtonAccessLocalOnly;
		private System.Windows.Forms.Label label28;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.RadioButton radioButton5;
		private System.Windows.Forms.RadioButton radioButton4;
		private System.Windows.Forms.TextBox textBox2;
		private System.Windows.Forms.TextBox textBoxLocalPubPath;
		private System.Windows.Forms.CheckBox checkBoxAIGroundPredictPoints;
		private System.Windows.Forms.CheckBox checkBoxAIGroundPredict;
		private System.Windows.Forms.CheckBox checkBoxAIBoatsPredictPoints;
		private System.Windows.Forms.CheckBox checkBoxAIBoatsPredict;
		private System.Windows.Forms.CheckBox checkBoxAIHelicoptersPredictPoints;
		private System.Windows.Forms.CheckBox checkBoxAIHelicoptersPredict;
		private System.Windows.Forms.CheckBox checkBoxAIAircraftsPredictPoints;
		private System.Windows.Forms.CheckBox checkBoxAIAircraftsPredict;
		private System.Windows.Forms.Timer timerIPAddressRefresh;
		private System.Windows.Forms.ToolStripSeparator toolStripMenuItem4;
		private System.Windows.Forms.ToolStripMenuItem createGoogleEarthKMLFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialogKMLFile;
        private System.Windows.Forms.TabPage tabPage9;
        private System.Windows.Forms.GroupBox groupBox10;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.TextBox textBox3;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.CheckBox checkBox1;
	}
}

