using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace FSXGET
{
	class ControlTree
	{
		CheckBox box;
		protected List<Control> controls;
		protected List<ControlTree> controlTrees;

		public ControlTree(CheckBox checkBox, int controls, int controlTrees)
		{
			box = checkBox;
			this.controls = new List<Control>(controls);
			this.controlTrees = new List<ControlTree>(controlTrees);
		}

		public void addControl(Control control)
		{
			controls.Add(control);
		}

		public void addControlTree(ControlTree controlTree)
		{
			controlTrees.Add(controlTree);
		}

		public void updateState()
		{
			foreach (Control control in controls)
				control.IsEnabled = (box.IsChecked == true  && box.IsEnabled == true);

			foreach (ControlTree controlTree in controlTrees)
				controlTree.updateState();
		}
	}
}
