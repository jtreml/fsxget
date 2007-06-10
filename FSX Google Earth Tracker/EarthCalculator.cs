using System;
using System.Collections.Generic;
using System.Text;

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
		/// <remarks>The implementation corresponds to the mathematics explained in the Wikipedia 
		/// article http://en.wikipedia.org/wiki/Reference_ellipsoid as of date 2007-05-30.</remarks>
		/// <param name="lat"></param>
		/// <param name="lon"></param>
		/// <param name="h"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		public static void geo2xyz(double lat, double lon, double h, ref double x, ref double y, ref double z)
		{
			double phi = lat * (double)(Math.PI) / (double)(180);
			double lambda = lon * (double)(Math.PI) / (double)(180);

			double n = a / Math.Sqrt((double)(1) - Math.Pow((Math.Sin(phi) * Math.Sin(ae)), (double)(2)));

			x = (n + h) * Math.Cos(phi) * Math.Cos(lambda);
			y = (n + h) * Math.Cos(phi) * Math.Sin(lambda);
			z = (Math.Pow(Math.Cos(ae), (double)(2)) * n + h) * Math.Sin(phi);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <remarks>The implementation corresponds to the mathematics explained in the Wikipedia 
		/// article http://en.wikipedia.org/wiki/Reference_ellipsoid as of date 2007-05-30.</remarks>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="lat"></param>
		/// <param name="lon"></param>
		/// <param name="h"></param>
		public static void xyz2geo(double x, double y, double z, ref double lat, ref double lon, ref double h)
		{
			const double fmin = (double)(0.000001);

			double psi_t = Math.Atan(z / Math.Sqrt(Math.Pow(x, (double)(2)) + Math.Pow(y, (double)(2))));

			double phi_c = Math.Atan(Math.Pow((double)(1) / Math.Cos(ae), 2) * Math.Tan(psi_t));
			double beta_c = Math.Atan(((double)(1) / Math.Cos(ae)) * Math.Tan(psi_t));

			double phi_p = phi_c;
			double beta_p = beta_c;

			do
			{
				phi_c = Math.Atan((z + b * Math.Pow(Math.Sin(beta_c), (double)(3)) * Math.Pow(Math.Tan(ae), (double)(2))) / (Math.Sqrt(Math.Pow(x, (double)(2)) + Math.Pow(y, (double)(2))) - a * Math.Pow(Math.Cos(beta_c), (double)(3)) * Math.Pow(Math.Sin(ae), (double)(2))));
				beta_c = Math.Atan(Math.Cos(ae) * Math.Tan(phi_c));

			} while (Math.Abs(phi_c - phi_p) > fmin || Math.Abs(beta_c - beta_p) > fmin);

			double phi = phi_c;

			double n = a / Math.Sqrt((double)(1) - Math.Pow((Math.Sin(phi) * Math.Sin(ae)), (double)(2)));

			h = ((double)(1) / Math.Cos(phi)) * Math.Sqrt(Math.Pow(x, (double)(2)) + Math.Pow(y, (double)(2))) - n;
			double lambda = Math.Acos(x * ((double)(1) / Math.Cos(phi)) * ((double)(1) / (n + h)));

			lat = phi * (double)(180) / (double)(Math.PI);
			lon = lambda * (double)(180) / (double)(Math.PI);
		}
	}
}
