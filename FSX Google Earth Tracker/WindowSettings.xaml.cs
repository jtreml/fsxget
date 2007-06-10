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
using System.Windows.Shapes;


namespace Fsxget
{
	/// <summary>
	/// Interaction logic for WindowSettings.xaml
	/// </summary>

	public partial class WindowSettings : System.Windows.Window
	{
		CtrlOverviewHome ctrlHome;

		CtrlSettingsSimDat ctrlSimDat;
		CtrlSettingsView ctrlView;
		CtrlSettingsLog ctrlLog;
		CtrlSettingsFp ctrlFp;
		CtrlSettingsFs ctrlFs;

		public WindowSettings()
		{
			InitializeComponent();

			showHome();			
		}


		protected void HyperlinkHelpClicked(object sender, RoutedEventArgs args)
		{
			ContextMenu cm = HyperlinkHelp.ContextMenu;

			HyperlinkHelp.ContextMenu.PlacementTarget = StackPanelHelp;
			HyperlinkHelp.ContextMenu.VerticalOffset = StackPanelHelp.ActualHeight;
			HyperlinkHelp.ContextMenu.HorizontalOffset = StackPanelHelp.ActualWidth;

			HyperlinkHelp.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Left;


			HyperlinkHelp.ContextMenu.IsOpen = true;
		}


		protected void HyperlinkOverviewStatusClicked(object sender, RoutedEventArgs args)
		{
			showHome();
		}

		protected void showHome()
		{
			if (ctrlHome == null)
				ctrlHome = new CtrlOverviewHome();

			LabelHeadline.Content = ctrlHome.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlHome);
		}


		protected void HyperlinkSettingsSimDataClicked(object sender, RoutedEventArgs args)
		{
			if (ctrlSimDat == null)
				ctrlSimDat = new CtrlSettingsSimDat();

			LabelHeadline.Content = ctrlSimDat.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlSimDat);
		}

		protected void HyperlinkSettingsViewClicked(object sender, RoutedEventArgs args)
		{
			if (ctrlView == null)
				ctrlView = new CtrlSettingsView();

			LabelHeadline.Content = ctrlView.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlView);
		}

		protected void HyperlinkSettingsFpClicked(object sender, RoutedEventArgs args)
		{
			if (ctrlFp == null)
				ctrlFp = new CtrlSettingsFp();

			LabelHeadline.Content = ctrlFp.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlFp);
		}

		protected void HyperlinkSettingsLogClicked(object sender, RoutedEventArgs args)
		{
			if (ctrlLog == null)
				ctrlLog = new CtrlSettingsLog();

			LabelHeadline.Content = ctrlLog.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlLog);
		}

		protected void HyperlinkSettingsFsClicked(object sender, RoutedEventArgs args)
		{
			if (ctrlFs == null)
				ctrlFs = new CtrlSettingsFs();

			LabelHeadline.Content = ctrlFs.Tag;

			MainPanel.Children.Clear();
			MainPanel.Children.Add(ctrlFs);
		}
	}
}