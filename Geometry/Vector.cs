using System;
using System.Collections.Generic;
using System.Text;

namespace Geometry
{
	public class Vector
	{
		public double X;
		public double Y;
		public double Z;

		public Vector(Point Start, Point End)
		{
			X = End.X - Start.X;
			Y = End.Y - Start.Y;
			Z = End.Z - Start.Z;
		}

		public Vector(Point Direction)
		{
			X = Direction.X;
			Y = Direction.Y;
			Z = Direction.Z;
		}

		public Vector(double X, double Y, double Z)
		{
			this.X = X;
			this.Y = Y;
			this.Z = Z;
		}

		public static Vector crossProduct(Vector A, Vector B)
		{
			return new Vector(
				A.Y * B.Z - A.Z * B.Y,
				A.Z * B.X - A.X * B.Z,
				A.X * B.Y - A.Y * B.X);
		}

		public double getLength()
		{
			return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
		}

		public Vector getNormalized()
		{
			double l = getLength();

			if (l == 0)
				return new Vector(0.0, 0.0, 0.0);
			else
				return this / l;
		}

		/// <summary>
		/// Rotates the vector around a given axis perpendicular to the vector.
		/// </summary>
		/// <remarks>Do not use this function for non-perpendicular axes. The method will not check
		/// perpendicularity but the results will be wrong.</remarks>
		/// <param name="Axis"></param>
		/// <param name="angle">The angle to rotate given in degrees.</param>
		/// <returns>The rotated vector (not normalized).</returns>
		public Vector rotateAroundPerpAxis(Vector Axis, double angle)
		{
			double theta = angle / 180.0 * Math.PI;

			double c = Math.Cos(theta);
			double s = Math.Sin(theta);
			double t = 1 - Math.Cos(theta);

			Matrix mxRotate = new Matrix(
				t * Math.Pow(Axis.X, 2.0) + c, t * Axis.X * Axis.Y - s * Axis.Z, t * Axis.X * Axis.Z + s * Axis.Y,
				t * Axis.X * Axis.Y + s * Axis.Z, t * Math.Pow(Axis.Y, 2.0) + c, t * Axis.Y * Axis.Z - s * Axis.X,
				t * Axis.X * Axis.Z - s * Axis.Y, t * Axis.Y * Axis.Z + s * Axis.X, t * Math.Pow(Axis.Z, 2.0) + c);

			return mxRotate * this;
		}

		public static Vector operator /(Vector V, double div)
		{
			return new Vector(V.X / div, V.Y / div, V.Z / div);
		}

		public static Vector operator *(Vector V, double div)
		{
			return new Vector(V.X * div, V.Y * div, V.Z * div);
		}

		public static Vector operator +(Vector V1, Vector V2)
		{
			return new Vector(V1.X + V2.X, V1.Y + V2.Y, V1.Z + V2.Z);
		}

		public static Vector operator -(Vector V1, Vector V2)
		{
			return new Vector(V1.X - V2.X, V1.Y - V2.Y, V1.Z - V2.Z);
		}
	}
}
