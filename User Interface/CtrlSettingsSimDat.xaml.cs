using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FSXGET
{
	/// <summary>
	/// Interaction logic for CtrlSettingsFsx.xaml
	/// </summary>

	public partial class CtrlSettingsSimDat : System.Windows.Controls.UserControl
	{
		public CtrlSettingsSimDat()
		{
			InitializeComponent();
		}

		protected void ButtonAirportsClick(object sender, RoutedEventArgs args)
		{
			DlgAirports airp = new DlgAirports();
			airp.ShowDialog();
		}
	}
}