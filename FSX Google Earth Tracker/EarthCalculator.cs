using System;
using System.Collections.Generic;
using System.Text;
using Geometry;

namespace Fsxget
{
	class EarthCalculator
	{
		static double a = (double)(6378137.0);
		static double b = (double)(6356752.3);

		static double ae = Math.Acos(b / a);


		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <para>The implementation corresponds to the mathematics explained in the Wikipedia 
		/// article http://en.wikipedia.org/wiki/Reference_ellipsoid as of date 2007-05-30.</para>
		/// <para>This function uses a coodinate system with Z axis in direction North-South 
		/// and X axis throught the prime meridian (geocentric coordinate system).</para>
		/// </remarks>
		/// <param name="ptIn"></param>
		/// <returns></returns>
		public static Point geo2xyz(GeoPoint ptIn)
		{
			double phi = ptIn.Lat * (double)(Math.PI) / (double)(180);
			double lambda = ptIn.Lon * (double)(Math.PI) / (double)(180);

			double n = a / Math.Sqrt((double)(1) - Math.Pow((Math.Sin(phi) * Math.Sin(ae)), (double)(2)));

			return new Point(
				(n + ptIn.Alt) * Math.Cos(phi) * Math.Cos(lambda),
				(n + ptIn.Alt) * Math.Cos(phi) * Math.Sin(lambda),
				(Math.Pow(Math.Cos(ae), (double)(2)) * n + ptIn.Alt) * Math.Sin(phi));
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// <para>The implementation corresponds to the mathematics explained in the Wikipedia 
		/// article http://en.wikipedia.org/wiki/Reference_ellipsoid as of date 2007-05-30.</para>
		/// <para>This function uses a coodinate system with Z axis in direction North-South 
		/// and X axis throught the prime meridian (geocentric coordinate system).</para>
		/// </remarks>
		/// <param name="ptIn"></param>
		/// <returns></returns>
		public static GeoPoint xyz2geo(Point ptIn)
		{
			const double fmin = (double)(0.01);

			// TODO: The functions seems quite instable regarding the fmin loop exit criteria. 
			// Therefore maxLoops limits the number of maximum iterations to avoid infinite looping. 
			// Maybe there's a better solution to that problem?!?
			const int maxLoops = 10;

			double psi_t = Math.Atan(ptIn.Z / Math.Sqrt(Math.Pow(ptIn.X, (double)(2)) + Math.Pow(ptIn.Y, (double)(2))));

			double phi_c = Math.Atan(Math.Pow((double)(1) / Math.Cos(ae), 2) * Math.Tan(psi_t));
			double beta_c = Math.Atan(((double)(1) / Math.Cos(ae)) * Math.Tan(psi_t));

			double phi_p = phi_c;
			double beta_p = beta_c;

			int iLoops = 0;
			do
			{
				iLoops++;
				phi_c = Math.Atan((ptIn.Z + b * Math.Pow(Math.Sin(beta_c), (double)(3)) * Math.Pow(Math.Tan(ae), (double)(2))) / (Math.Sqrt(Math.Pow(ptIn.X, (double)(2)) + Math.Pow(ptIn.Y, (double)(2))) - a * Math.Pow(Math.Cos(beta_c), (double)(3)) * Math.Pow(Math.Sin(ae), (double)(2))));
				beta_c = Math.Atan(Math.Cos(ae) * Math.Tan(phi_c));

			} while ((Math.Abs(phi_c - phi_p) > fmin || Math.Abs(beta_c - beta_p) > fmin) && iLoops < maxLoops);

			double phi = phi_c;

			double n = a / Math.Sqrt((double)(1) - Math.Pow((Math.Sin(phi) * Math.Sin(ae)), (double)(2)));

			double h = ((double)(1) / Math.Cos(phi)) * Math.Sqrt(Math.Pow(ptIn.X, (double)(2)) + Math.Pow(ptIn.Y, (double)(2))) - n;
			double lambda = Math.Acos(ptIn.X * ((double)(1) / Math.Cos(phi)) * ((double)(1) / (n + h)));

			return new GeoPoint(
				phi * (double)(180) / (double)(Math.PI),
				lambda * (double)(180) / (double)(Math.PI),
				h);
		}
	}
}
