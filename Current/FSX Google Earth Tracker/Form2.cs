using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace FSX_Google_Earth_Tracker
{
	public partial class Form2 : Form
	{
		public Form2()
		{
			InitializeComponent();
		}

		public DialogResult ShowDialog(out int iSeconds)
		{
			comboBox1.SelectedIndex = 1;
			DialogResult dlrTemp = ShowDialog();

			iSeconds = (int)numericUpDown1.Value;
			switch(comboBox1.SelectedIndex)
			{
				case 0:
					break;
				case 1:
					iSeconds *= 60;
					break;
				case 2:
					iSeconds *= 3600;
					break;
				default:
					break;
			}

			return dlrTemp;
		}

		private void button1_Click(object sender, EventArgs e)
		{
		
		}

		private void Form2_Load(object sender, EventArgs e)
		{

		}
	}
}
