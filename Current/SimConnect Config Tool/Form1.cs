using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace FSX_Config_Tool
{
	public partial class Form1 : Form
	{
		bool bChanges = false;

		XmlDocument xmldSimCon;

		String szFileSimCon = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Microsoft\\FSX\\SimConnect.xml";

#if DEBUG
		String szFileSimConSample = Application.StartupPath + "\\..\\..\\SimConnect.sample";
#else
		String szFileSimConSample = Application.StartupPath + "\\SimConnect.sample";
#endif

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			bool bLoadSample = false;

			if (File.Exists(szFileSimCon))
			{
				radioButton2.Checked = true;

				XmlReader xmlrSimCon;

				try
				{
					xmlrSimCon = new XmlTextReader(szFileSimCon);
					xmldSimCon = new XmlDocument();
					xmldSimCon.Load(xmlrSimCon);
					xmlrSimCon.Close();
					xmlrSimCon = null;

					if (xmldSimCon["SimBase.Document"] == null)
						throw new Exception("Invalid existing SimConnect config file.");
				}
				catch
				{
					xmlrSimCon = null;

					try
					{
						File.Delete(szFileSimCon);
						bLoadSample = true;
					}
					catch
					{
						MessageBox.Show("There is already a SimConnect file in your FSX directory but the file is invalid and cannot be deleted. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						bChanges = false;
						Close();
					}
				}
			}
			else
				bLoadSample = true;

			if (bLoadSample)
			{
				radioButton1.Checked = true;

				XmlReader xmlrSimCon = new XmlTextReader(szFileSimConSample);
				xmldSimCon = new XmlDocument();
				xmldSimCon.Load(xmlrSimCon);
				xmlrSimCon.Close();
				xmlrSimCon = null;
			}

			radioButton1_CheckedChanged(null, null);
			radioButton2_CheckedChanged(null, null);

			updateListView();

			// Important to set to false because we have called some UI methods before
			bChanges = false;
		}

		private void updateListView()
		{
			int iCount = 0;

			for (XmlNode xmlnTemp = xmldSimCon["SimBase.Document"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			{
				if (xmlnTemp.Name.ToLower() == "SimConnect.Comm".ToLower())
				{
					iCount++;

					ListViewItem itemTemp = listView1.Items.Add(iCount.ToString());

					if (xmlnTemp["Scope"] != null)
						itemTemp.SubItems.Add(xmlnTemp["Scope"].InnerText);
					else
						itemTemp.SubItems.Add("");

					if (xmlnTemp["Protocol"] != null)
						itemTemp.SubItems.Add(xmlnTemp["Protocol"].InnerText);
					else
						itemTemp.SubItems.Add("");

					if (xmlnTemp["Port"] != null)
						itemTemp.SubItems.Add(xmlnTemp["Port"].InnerText);
					else
						itemTemp.SubItems.Add("");

					if (xmlnTemp["Address"] != null)
						itemTemp.SubItems.Add(xmlnTemp["Address"].InnerText);
					else
						itemTemp.SubItems.Add("");

					if (xmlnTemp["Disabled"] != null)
						itemTemp.SubItems.Add(xmlnTemp["Disabled"].InnerText);
					else
						itemTemp.SubItems.Add("");

					if (xmlnTemp["MaxClients"] != null)
						itemTemp.SubItems.Add(xmlnTemp["MaxClients"].InnerText);
					else
						itemTemp.SubItems.Add("");

					itemTemp.Tag = xmlnTemp;
				}
			}
		}


		private void button1_Click_1(object sender, EventArgs e)
		{
			saveSettings();
			Close();
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			checkBox1.Enabled = listView1.Enabled = button2.Enabled = radioButton2.Checked;
			button3.Enabled = button4.Enabled = false;
			bChanges = true;
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			checkBox1.Enabled = listView1.Enabled = button2.Enabled = button3.Enabled = button4.Enabled = !radioButton1.Checked;
			bChanges = true;
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void Form1_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (bChanges)
			{
				DialogResult dlgRes = MessageBox.Show("You have made changes to your SimConnect configuration. Do you want to save your changes?", Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

				switch (dlgRes)
				{
					case DialogResult.Yes:
						break;

					case DialogResult.No:
						break;

					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}

		private void saveSettings()
		{
			if (bChanges)
			{
				if (radioButton1.Checked)
				{
					if (File.Exists(szFileSimCon))
					{
						try
						{
							File.Delete(szFileSimCon);
						}
						catch
						{
							MessageBox.Show("Cannot delete existing SimConnect file and change current configuration. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
						}
					}
				}
				else
				{
					// Set enabled or disabled state
					if (xmldSimCon["SimBase.Document"]["Disabled"] == null)
					{
						XmlNode xmlnNew = xmldSimCon.CreateTextNode("Disabled");
						xmldSimCon["SimBase.Document"].AppendChild(xmlnNew);
					}

					if (checkBox1.Checked)
						xmldSimCon["SimBase.Document"]["Disabled"].InnerText = "False";
					else
						xmldSimCon["SimBase.Document"]["Disabled"].InnerText = "True";


					// Set connection entries
					// TODO: still missing...


					// Save config file
					try
					{
						File.Delete(szFileSimCon);
						xmldSimCon.Save(szFileSimCon);
					}
					catch
					{
						MessageBox.Show("Cannot edit SimConnect file to change current configuration. Aborting!", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
					}
				}

				MessageBox.Show("Save");
				bChanges = false;
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			bChanges = false;
			Close();
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			bChanges = true;
		}

		private void listView1_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (listView1.SelectedItems.Count == 1)
			{
				button3.Enabled = true;
				button4.Enabled = true;
			}
			else
			{
				button3.Enabled = false;
				button4.Enabled = false;
			}
		}

		private void listView1_Enter(object sender, EventArgs e)
		{
			listView1_SelectedIndexChanged(null, null);
		}

		private void aboutToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			AboutBox1 frmAbout = new AboutBox1();
			frmAbout.ShowDialog();
		}
	}
}
