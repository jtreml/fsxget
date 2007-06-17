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
	}
}
