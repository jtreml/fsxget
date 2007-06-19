using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry
{
	public struct Point
	{
		public Point(double X, double Y, double Z)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		public Point(Vector vec)
		{
			X = vec.X;
			Y = vec.Y;
			Z = vec.Z;
		}

		public double X;
		public double Y;
		public double Z;
	}

	public struct GeoPoint
	{
		public GeoPoint(double Lat, double Lon, double Alt)
		{
			this.Lat = Lat;
			this.Lon = Lon;
			this.Alt = Alt;
		}

		public double Lat;
		public double Lon;
		public double Alt;

		public override String ToString()
		{
			// TODO: I'm sure there's some special formatting function to get the right number format without using string replace functions

			return Lon.ToString().Replace(",", ".") + "," + Lat.ToString().Replace(",", ".") + "," + Alt.ToString().Replace(",", ".");
		}
	}
}
