using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace FSX_Config_Tool
{
	public partial class Form1 : Form
	{
		XmlDocument xmldSimCon;

		public Form1()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			ListViewItem item1 = listView1.Items.Add("test");
			item1.SubItems.Add("test2");
			item1.SubItems.Add("test3");
			item1.SubItems.Add("test4");
			item1.SubItems.Add("test5");
			
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			XmlReader xmlrSimCon = new XmlTextReader("C:\\Users\\jtr\\AppData\\Roaming\\Microsoft\\FSX\\SimConnect.xml");
			XmlDocument xmldSimCon = new XmlDocument();
			xmldSimCon.Load(xmlrSimCon);
			xmlrSimCon.Close();
			xmlrSimCon = null;

			int iCount = 0;
			for (XmlNode xmlnTemp = xmldSimCon["SimBase.Document"].FirstChild; xmlnTemp != null; xmlnTemp = xmlnTemp.NextSibling)
			{
				if (xmlnTemp.Name.ToLower() == "SimConnect.Comm".ToLower())
				{
					iCount++;

					ListViewItem itemTemp = listView1.Items.Add(iCount.ToString());
					itemTemp.SubItems.Add(xmlnTemp["Protocol"].InnerText);
					itemTemp.Tag = xmlnTemp;
				}
			}

		}
	}
}
