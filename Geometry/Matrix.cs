using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry
{
	public class Matrix
	{
		public double[,] data = new double[3, 3];

		public Matrix(double val00, double val01, double val02, double val10, double val11, double val12, double val20, double val21, double val22)
		{
			data[0, 0] = val00;
			data[0, 1] = val01;
			data[0, 2] = val02;
			data[1, 0] = val10;
			data[1, 1] = val11;
			data[1, 2] = val12;
			data[2, 0] = val20;
			data[2, 1] = val21;
			data[2, 2] = val22;
		}

		public static Vector operator *(Matrix mat, Vector vec)
		{
			return new Vector(
				mat.data[0,0] * vec.X + mat.data[0,1] * vec.Y + mat.data[0,2] * vec.Z,
				mat.data[1,0] * vec.X + mat.data[1,1] * vec.Y + mat.data[1,2] * vec.Z,
				mat.data[2,0] * vec.X + mat.data[2,1] * vec.Y + mat.data[2,2] * vec.Z);
		}
	}
}
