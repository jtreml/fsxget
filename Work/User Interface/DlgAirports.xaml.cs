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
using System.Data.OleDb;
using System.Xml;
using System.Web;
using System.Threading;
using System.ComponentModel;

namespace FSXGET
{
	/// <summary>
	/// Interaction logic for DlgAirports.xaml
	/// </summary>

	public partial class DlgAirports : System.Windows.Window
	{
		Predicate<object> pred;

		XmlDocument xmlDoc = new XmlDocument();
		XmlElement elemMain;

		protected delegate void FilterDelegate();
		FilterDelegate delFilter = null;

		Timer timer;


		protected struct AirportListItem
		{
			public String Name;
			public String Code;
			public bool Signs;
		}


		public DlgAirports()
		{
			InitializeComponent();

			timer = new Timer(TimerProc);

			// Predicate an delegate we'll use for filtering the listview
			pred = new Predicate<object>(Contains);
			delFilter = new FilterDelegate(Filter);

			// Set up the xml document
			elemMain = xmlDoc.CreateElement("doc");
			xmlDoc.AppendChild(elemMain);

			// Bind xml doc to airport list listview
			XmlDataProvider prov = new XmlDataProvider();
			prov.Document = xmlDoc;
			prov.XPath = "/doc";

			Binding itemsBinding = new Binding();
			itemsBinding.XPath = "Airport";
			itemsBinding.Source = prov;

			ListAirports.SetBinding(ListView.ItemsSourceProperty, itemsBinding);

			// Now get the data and fill listviewy
			OleDbConnection dbCon = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=C:\\Users\\jtr\\Programming\\Projects\\FSX Google Earth Tracker\\Work\\FSX Google Earth Tracker\\data\\fsxget.mdb");

			dbCon.Open();

			OleDbCommand cmd = new OleDbCommand("SELECT Airports.Ident, Airports.Name, Airports.City, Countrys.Name FROM Airports LEFT JOIN Countrys ON Airports.CountryID = Countrys.ID;", dbCon);
			OleDbDataReader rd = cmd.ExecuteReader();

			long lCount = 0;

			while (rd.Read())
			{
				lCount++;

				XmlNode nodeTemp = elemMain.AppendChild(xmlDoc.CreateElement("Airport"));

				XmlAttribute attriTemp = xmlDoc.CreateAttribute("Name");
				attriTemp.Value = rd.GetString(1);
				nodeTemp.Attributes.Append(attriTemp);

				attriTemp = xmlDoc.CreateAttribute("Code");
				attriTemp.Value = rd.GetString(0);
				nodeTemp.Attributes.Append(attriTemp);

				attriTemp = xmlDoc.CreateAttribute("City");
				attriTemp.Value = rd.GetString(2);
				nodeTemp.Attributes.Append(attriTemp);

				attriTemp = xmlDoc.CreateAttribute("Country");
				attriTemp.Value = rd.GetString(3);
				nodeTemp.Attributes.Append(attriTemp);

				nodeTemp.Attributes.Append(xmlDoc.CreateAttribute("Show"));
				nodeTemp.Attributes.Append(xmlDoc.CreateAttribute("Boundaries"));
				nodeTemp.Attributes.Append(xmlDoc.CreateAttribute("RealIcon"));
				nodeTemp.Attributes.Append(xmlDoc.CreateAttribute("TaxiSigns"));
			}
			rd.Close();

			while (dbCon.State != System.Data.ConnectionState.Closed && dbCon.State != System.Data.ConnectionState.Open)
			{
				if (dbCon.State == System.Data.ConnectionState.Closed)
					return;

				dbCon.Close();
			}

			ListAirports.Items.Refresh();

			LabelAirportsTotal.Content = lCount;
			LabelAirportsCurrent.Content = lCount;
		}

		void ButtonSearchCancelClick(object sender, RoutedEventArgs e)
		{
			ListAirports.Items.Filter = null;
			ButtonSearchCancel.Visibility = Visibility.Hidden;
			LabelAirportsCurrent.Content = ListAirports.Items.Count;
			TextBoxFilter.Text = "";
		}

		public bool Contains(object objTest)
		{
			XmlElement xmlEle = (XmlElement)objTest;
			return (xmlEle.Attributes["Name"].Value.ToLower().Contains(TextBoxFilter.Text.ToLower())
				|| xmlEle.Attributes["Code"].Value.ToLower().Contains(TextBoxFilter.Text.ToLower())
				|| xmlEle.Attributes["Country"].Value.ToLower().Contains(TextBoxFilter.Text.ToLower())
				|| xmlEle.Attributes["City"].Value.ToLower().Contains(TextBoxFilter.Text.ToLower()));
		}

		private void Tc(object sender, TextChangedEventArgs e)
		{
			if (!IsInitialized)
				return;

			timer.Change(400, Timeout.Infinite);
		}

		private void TimerProc(object state)
		{
			Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Background, delFilter);
		}


		protected void Filter()
		{
			Console.WriteLine("The filter delegate executes.");

			if (TextBoxFilter.Text != "")
			{
				ListAirports.Items.Filter = pred;
				ButtonSearchCancel.Visibility = Visibility.Visible;
				LabelAirportsCurrent.Content = ListAirports.Items.Count;
			}
			else if (TextBoxFilter.Text == "")
			{
				ListAirports.Items.Filter = null;
				ButtonSearchCancel.Visibility = Visibility.Hidden;
				LabelAirportsCurrent.Content = ListAirports.Items.Count;
			}

			LabelAirportsCurrent.Content = ListAirports.Items.Count;
		}

		protected void Fill()
		{
		}
	}
}