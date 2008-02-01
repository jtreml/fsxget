using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Reflection;

namespace FSX_Config_Tool
{
	public partial class Form2 : Form
	{
		struct ConfigurationData
		{
			public String szName;
			public bool bEnabled;
			public String szProtocol;
			public String szScope;
			public String szAddress;
			public int iMaxClients;
			public int iPort;

			public ConfigurationData(String Name, bool Enabled, String Protocol, String Scope, String Address, int MaxClients, int Port)
			{
				szName = Name;
				bEnabled = Enabled;
				szProtocol = Protocol;
				szScope = Scope;
				szAddress = Address;
				iMaxClients = MaxClients;
				iPort = Port;
			}
		}

		List<ConfigurationData> listConfigs = new List<ConfigurationData>(5);

		XmlNode xmlnCurrent;

		public Form2()
		{
			InitializeComponent();

			listConfigs.Add(new ConfigurationData("FSXGET Default Configuration", true, "IPv4", "Global", "0.0.0.0", 64, 9017));
			listConfigs.Add(new ConfigurationData("Global Access (IPv4)", true, "IPv4", "Global", "0.0.0.0", 64, 9017));
			listConfigs.Add(new ConfigurationData("Global Access (IPv6)", true, "IPv6", "Global", "::", 64, 9017));
			listConfigs.Add(new ConfigurationData("Global Access (Pipe)", true, "Pipe", "Global", "", 64, 9017));
			listConfigs.Add(new ConfigurationData("Local Access (IPv4)", true, "IPv4", "Local", "127.0.0.1", 64, 9017));
			listConfigs.Add(new ConfigurationData("Local Access (IPv6)", true, "IPv6", "Local", "::1", 64, 9017));
			listConfigs.Add(new ConfigurationData("Local Access (Pipe)", true, "Pipe", "Local", "", 64, 9017));

			foreach (ConfigurationData cdTemp in listConfigs)
			{
				comboBox1.Items.Add(cdTemp.szName);
			}

			comboBox1.Items.Add("Custom Configuration");
		}

		private void Form2_Load(object sender, EventArgs e)
		{ }

		private void findMatchingPreset()
		{
			if (comboBox2.SelectedItem != null && comboBox3.SelectedItem != null)
			{
				foreach (ConfigurationData cdTemp in listConfigs)
				{
					if (checkBox1.Checked == cdTemp.bEnabled &&
						comboBox2.SelectedItem.ToString().ToLower() == cdTemp.szProtocol.ToLower() &&
						comboBox3.SelectedItem.ToString().ToLower() == cdTemp.szScope.ToLower() &&
						numericUpDown1.Value == cdTemp.iMaxClients &&
						numericUpDown2.Value == cdTemp.iPort &&
						textBox1.Text.ToLower() == cdTemp.szAddress.ToLower())
					{

						comboBox1.SelectedIndex = comboBox1.FindString(cdTemp.szName);
						return;
					}
				}
			}

			comboBox1.SelectedIndex = comboBox1.FindString("Custom Configuration");
		}

		public DialogResult ShowDialog(XmlNode xmlnEdit, bool bEdit)
		{
			xmlnCurrent = xmlnEdit;

			if (bEdit)
			{
				Text = AssemblyProduct + " - Edit Item";

				checkBox1.Checked = xmlnEdit["Disabled"].InnerText.ToLower() == "false" ? true : false;
				comboBox2.SelectedIndex = comboBox2.FindString(xmlnEdit["Protocol"].InnerText);
				comboBox3.SelectedIndex = comboBox3.FindString(xmlnEdit["Scope"].InnerText);

				int iTemp;

				if (!int.TryParse(xmlnEdit["MaxClients"].InnerText, out iTemp))
					iTemp = 0;
				numericUpDown1.Value = iTemp;

				if (!int.TryParse(xmlnEdit["Port"].InnerText, out iTemp))
					iTemp = 0;
				numericUpDown2.Value = iTemp;

				textBox1.Text = xmlnEdit["Address"].InnerText;

				findMatchingPreset();
			}
			else
			{
				Text = AssemblyProduct + " - Add Item";

				comboBox1.SelectedIndex = comboBox1.FindString("FSXGET Default Configuration");
				comboBox1_SelectedIndexChanged(null, null);
			}

			ShowDialog();

			return DialogResult.OK;
		}

		#region Assembly Attribute Accessors

		public string AssemblyTitle
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
				if (attributes.Length > 0)
				{
					AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
					if (titleAttribute.Title != "")
					{
						return titleAttribute.Title;
					}
				}
				return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
			}
		}

		public string AssemblyVersion
		{
			get
			{
				return Assembly.GetExecutingAssembly().GetName().Version.ToString();
			}
		}

		public string AssemblyDescription
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyDescriptionAttribute)attributes[0]).Description;
			}
		}

		public string AssemblyProduct
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyProductAttribute)attributes[0]).Product;
			}
		}

		public string AssemblyCopyright
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
			}
		}

		public string AssemblyCompany
		{
			get
			{
				object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
				if (attributes.Length == 0)
				{
					return "";
				}
				return ((AssemblyCompanyAttribute)attributes[0]).Company;
			}
		}

		#endregion

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void numericUpDown1_ValueChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void numericUpDown2_ValueChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void textBox1_TextChanged(object sender, EventArgs e)
		{
			findMatchingPreset();
		}

		private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
		{
			foreach (ConfigurationData cdTemp in listConfigs)
			{
				if (cdTemp.szName.ToLower() == comboBox1.SelectedItem.ToString().ToLower())
				{
					checkBox1.Checked = cdTemp.bEnabled;
					comboBox2.SelectedIndex = comboBox2.FindString(cdTemp.szProtocol);
					comboBox3.SelectedIndex = comboBox3.FindString(cdTemp.szScope);
					numericUpDown1.Value = cdTemp.iMaxClients;
					numericUpDown2.Value = cdTemp.iPort;
					textBox1.Text = cdTemp.szAddress;

					return;
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			xmlnCurrent["Disabled"].InnerText = checkBox1.Checked ? "False" : "True";
			xmlnCurrent["Protocol"].InnerText = comboBox2.SelectedItem.ToString();
			xmlnCurrent["Scope"].InnerText = comboBox3.SelectedItem.ToString();
			xmlnCurrent["MaxClients"].InnerText = numericUpDown1.Value.ToString();
			xmlnCurrent["Address"].InnerText = textBox1.Text;
			xmlnCurrent["Port"].InnerText = numericUpDown2.Value.ToString();
		}

		private void button1_Click(object sender, EventArgs e)
		{

		}

	}
}
