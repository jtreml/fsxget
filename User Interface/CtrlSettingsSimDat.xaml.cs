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
		#region Definitions

		#region Control Definitions

		ControlTree treeUserAircraft;
		ControlTree treeUserAircraftPath;
		ControlTree treeUserAircraftPrediction;
		ControlTree treeUserAircraftSeparation;

		ControlTree treeAiAircraft;
		ControlTree treeAiAircraftPath;
		ControlTree treeAiAircraftPrediction;
		ControlTree treeAiAircraftSeparation;

		ControlTree treeAiHelicopter;
		ControlTree treeAiHelicopterPath;
		ControlTree treeAiHelicopterPrediction;
		ControlTree treeAiHelicopterSeparation;

		ControlTree treeAiBoat;
		ControlTree treeAiBoatPath;
		ControlTree treeAiBoatPrediction;
		ControlTree treeAiBoatSeparation;

		ControlTree treeAiGround;
		ControlTree treeAiGroundPath;
		ControlTree treeAiGroundPrediction;
		ControlTree treeAiGroundSeparation;

		#endregion

		#endregion

		public CtrlSettingsSimDat()
		{
			InitializeComponent();

			#region UI Init

			#region User Aircraft Control Tree

			treeUserAircraftPath = new ControlTree(UserAircraftCheckboxPath, 1, 0);
			treeUserAircraftPath.addControl(UserAircraftButtonPath);

			treeUserAircraftPrediction = new ControlTree(UserAircraftCheckboxPrediction, 1, 0);
			treeUserAircraftPrediction.addControl(UserAircraftButtonPrediction);

			treeUserAircraftSeparation = new ControlTree(UserAircraftCheckboxSeparation, 1, 0);
			treeUserAircraftSeparation.addControl(UserAircraftButtonSeparation);

			treeUserAircraft = new ControlTree(UserAircraftCheckboxEnable, 3, 4);
			treeUserAircraft.addControl(UserAircraftTextboxInterval);
			treeUserAircraft.addControl(UserAircraftCheckboxPath);
			treeUserAircraft.addControl(UserAircraftCheckboxPrediction);
			treeUserAircraft.addControl(UserAircraftCheckboxSeparation);
			treeUserAircraft.addControlTree(treeUserAircraftPath);
			treeUserAircraft.addControlTree(treeUserAircraftPrediction);
			treeUserAircraft.addControlTree(treeUserAircraftSeparation);

			treeUserAircraft.updateState();

			#endregion

			#region AI Aircraft Control Tree

			treeAiAircraftPath = new ControlTree(AiAircraftCheckboxPath, 1, 0);
			treeAiAircraftPath.addControl(AiAircraftButtonPath);

			treeAiAircraftPrediction = new ControlTree(AiAircraftCheckboxPrediction, 1, 0);
			treeAiAircraftPrediction.addControl(AiAircraftButtonPrediction);

			treeAiAircraftSeparation = new ControlTree(AiAircraftCheckboxSeparation, 1, 0);
			treeAiAircraftSeparation.addControl(AiAircraftButtonSeparation);

			treeAiAircraft = new ControlTree(AiAircraftCheckboxEnable, 3, 4);
			treeAiAircraft.addControl(AiAircraftTextboxInterval);
			treeAiAircraft.addControl(AiAircraftCheckboxPath);
			treeAiAircraft.addControl(AiAircraftCheckboxPrediction);
			treeAiAircraft.addControl(AiAircraftCheckboxSeparation);
			treeAiAircraft.addControlTree(treeAiAircraftPath);
			treeAiAircraft.addControlTree(treeAiAircraftPrediction);
			treeAiAircraft.addControlTree(treeAiAircraftSeparation);

			treeAiAircraft.updateState();

			#endregion

			#region AI Helicopter Control Tree

			treeAiHelicopterPath = new ControlTree(AiHelicopterCheckboxPath, 1, 0);
			treeAiHelicopterPath.addControl(AiHelicopterButtonPath);

			treeAiHelicopterPrediction = new ControlTree(AiHelicopterCheckboxPrediction, 1, 0);
			treeAiHelicopterPrediction.addControl(AiHelicopterButtonPrediction);

			treeAiHelicopterSeparation = new ControlTree(AiHelicopterCheckboxSeparation, 1, 0);
			treeAiHelicopterSeparation.addControl(AiHelicopterButtonSeparation);

			treeAiHelicopter = new ControlTree(AiHelicopterCheckboxEnable, 3, 4);
			treeAiHelicopter.addControl(AiHelicopterTextboxInterval);
			treeAiHelicopter.addControl(AiHelicopterCheckboxPath);
			treeAiHelicopter.addControl(AiHelicopterCheckboxPrediction);
			treeAiHelicopter.addControl(AiHelicopterCheckboxSeparation);
			treeAiHelicopter.addControlTree(treeAiHelicopterPath);
			treeAiHelicopter.addControlTree(treeAiHelicopterPrediction);
			treeAiHelicopter.addControlTree(treeAiHelicopterSeparation);

			treeAiHelicopter.updateState();

			#endregion

			#region AI Boat Control Tree

			treeAiBoatPath = new ControlTree(AiBoatCheckboxPath, 1, 0);
			treeAiBoatPath.addControl(AiBoatButtonPath);

			treeAiBoatPrediction = new ControlTree(AiBoatCheckboxPrediction, 1, 0);
			treeAiBoatPrediction.addControl(AiBoatButtonPrediction);

			treeAiBoatSeparation = new ControlTree(AiBoatCheckboxSeparation, 1, 0);
			treeAiBoatSeparation.addControl(AiBoatButtonSeparation);

			treeAiBoat = new ControlTree(AiBoatCheckboxEnable, 3, 4);
			treeAiBoat.addControl(AiBoatTextboxInterval);
			treeAiBoat.addControl(AiBoatCheckboxPath);
			treeAiBoat.addControl(AiBoatCheckboxPrediction);
			treeAiBoat.addControl(AiBoatCheckboxSeparation);
			treeAiBoat.addControlTree(treeAiBoatPath);
			treeAiBoat.addControlTree(treeAiBoatPrediction);
			treeAiBoat.addControlTree(treeAiBoatSeparation);

			treeAiBoat.updateState();

			#endregion

			#region AI Ground Control Tree

			treeAiGroundPath = new ControlTree(AiGroundCheckboxPath, 1, 0);
			treeAiGroundPath.addControl(AiGroundButtonPath);

			treeAiGroundPrediction = new ControlTree(AiGroundCheckboxPrediction, 1, 0);
			treeAiGroundPrediction.addControl(AiGroundButtonPrediction);

			treeAiGroundSeparation = new ControlTree(AiGroundCheckboxSeparation, 1, 0);
			treeAiGroundSeparation.addControl(AiGroundButtonSeparation);

			treeAiGround = new ControlTree(AiGroundCheckboxEnable, 3, 4);
			treeAiGround.addControl(AiGroundTextboxInterval);
			treeAiGround.addControl(AiGroundCheckboxPath);
			treeAiGround.addControl(AiGroundCheckboxPrediction);
			treeAiGround.addControl(AiGroundCheckboxSeparation);
			treeAiGround.addControlTree(treeAiGroundPath);
			treeAiGround.addControlTree(treeAiGroundPrediction);
			treeAiGround.addControlTree(treeAiGroundSeparation);

			treeAiGround.updateState();

			#endregion

			#endregion
		}

		#region UI Handlers

		protected void ButtonAirportsClick(object sender, RoutedEventArgs args)
		{
			DlgAirports airp = new DlgAirports();
			airp.ShowDialog();
		}


		#region User Aircraft Control Handlers

		protected void UserAircraftCheckboxEnableChecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraft.updateState();
		}

		protected void UserAircraftCheckboxEnableUnchecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraft.updateState();
		}

		protected void UserAircraftCheckboxPathUnchecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftPath.updateState();
		}

		protected void UserAircraftCheckboxPathChecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftPath.updateState();
		}

		protected void UserAircraftCheckboxPredictionUnchecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftPrediction.updateState();
		}

		protected void UserAircraftCheckboxPredictionChecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftPrediction.updateState();
		}

		protected void UserAircraftCheckboxSeparationUnchecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftSeparation.updateState();
		}

		protected void UserAircraftCheckboxSeparationChecked(object sender, RoutedEventArgs args)
		{
			treeUserAircraftSeparation.updateState();
		}

		protected void UserAircraftButtonPathClick(object sender, RoutedEventArgs args) { }

		protected void UserAircraftButtonPredictionClick(object sender, RoutedEventArgs args) { }

		protected void UserAircraftButtonSeparationClick(object sender, RoutedEventArgs args) { }

		#endregion

		#region AI Aircraft Control Handlers

		protected void AiAircraftCheckboxEnableChecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraft.updateState();
		}

		protected void AiAircraftCheckboxEnableUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraft.updateState();
		}

		protected void AiAircraftCheckboxPathUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftPath.updateState();
		}

		protected void AiAircraftCheckboxPathChecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftPath.updateState();
		}

		protected void AiAircraftCheckboxPredictionUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftPrediction.updateState();
		}

		protected void AiAircraftCheckboxPredictionChecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftPrediction.updateState();
		}

		protected void AiAircraftCheckboxSeparationUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftSeparation.updateState();
		}

		protected void AiAircraftCheckboxSeparationChecked(object sender, RoutedEventArgs args)
		{
			treeAiAircraftSeparation.updateState();
		}

		protected void AiAircraftButtonPathClick(object sender, RoutedEventArgs args) { }

		protected void AiAircraftButtonPredictionClick(object sender, RoutedEventArgs args) { }

		protected void AiAircraftButtonSeparationClick(object sender, RoutedEventArgs args) { }

		#endregion

		#region AI Helicopter Control Handlers

		protected void AiHelicopterCheckboxEnableChecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopter.updateState();
		}

		protected void AiHelicopterCheckboxEnableUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopter.updateState();
		}

		protected void AiHelicopterCheckboxPathUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterPath.updateState();
		}

		protected void AiHelicopterCheckboxPathChecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterPath.updateState();
		}

		protected void AiHelicopterCheckboxPredictionUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterPrediction.updateState();
		}

		protected void AiHelicopterCheckboxPredictionChecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterPrediction.updateState();
		}

		protected void AiHelicopterCheckboxSeparationUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterSeparation.updateState();
		}

		protected void AiHelicopterCheckboxSeparationChecked(object sender, RoutedEventArgs args)
		{
			treeAiHelicopterSeparation.updateState();
		}

		protected void AiHelicopterButtonPathClick(object sender, RoutedEventArgs args) { }

		protected void AiHelicopterButtonPredictionClick(object sender, RoutedEventArgs args) { }

		protected void AiHelicopterButtonSeparationClick(object sender, RoutedEventArgs args) { }

		#endregion

		#region AI Boat Control Handlers

		protected void AiBoatCheckboxEnableChecked(object sender, RoutedEventArgs args)
		{
			treeAiBoat.updateState();
		}

		protected void AiBoatCheckboxEnableUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiBoat.updateState();
		}

		protected void AiBoatCheckboxPathUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatPath.updateState();
		}

		protected void AiBoatCheckboxPathChecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatPath.updateState();
		}

		protected void AiBoatCheckboxPredictionUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatPrediction.updateState();
		}

		protected void AiBoatCheckboxPredictionChecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatPrediction.updateState();
		}

		protected void AiBoatCheckboxSeparationUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatSeparation.updateState();
		}

		protected void AiBoatCheckboxSeparationChecked(object sender, RoutedEventArgs args)
		{
			treeAiBoatSeparation.updateState();
		}

		protected void AiBoatButtonPathClick(object sender, RoutedEventArgs args) { }

		protected void AiBoatButtonPredictionClick(object sender, RoutedEventArgs args) { }

		protected void AiBoatButtonSeparationClick(object sender, RoutedEventArgs args) { }

		#endregion

		#region AI Ground Control Handlers

		protected void AiGroundCheckboxEnableChecked(object sender, RoutedEventArgs args)
		{
			treeAiGround.updateState();
		}

		protected void AiGroundCheckboxEnableUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiGround.updateState();
		}

		protected void AiGroundCheckboxPathUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundPath.updateState();
		}

		protected void AiGroundCheckboxPathChecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundPath.updateState();
		}

		protected void AiGroundCheckboxPredictionUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundPrediction.updateState();
		}

		protected void AiGroundCheckboxPredictionChecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundPrediction.updateState();
		}

		protected void AiGroundCheckboxSeparationUnchecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundSeparation.updateState();
		}

		protected void AiGroundCheckboxSeparationChecked(object sender, RoutedEventArgs args)
		{
			treeAiGroundSeparation.updateState();
		}

		protected void AiGroundButtonPathClick(object sender, RoutedEventArgs args) { }

		protected void AiGroundButtonPredictionClick(object sender, RoutedEventArgs args) { }

		protected void AiGroundButtonSeparationClick(object sender, RoutedEventArgs args) { }

		#endregion

		#endregion
	}
}