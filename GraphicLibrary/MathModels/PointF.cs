﻿using System.Drawing;
using static System.MathF;

namespace GraphicLibrary.MathModels;
public class PointF : IEquatable<PointF>, IEquatable<System.Drawing.Point>, IEquatable<System.Drawing.PointF>, IEquatable<System.Windows.Point>
{
	public float X, Y;

	public PointF()
	{

	}
	public PointF(float x, float y)
	{
		X = x;
		Y = y;
	}
	public PointF(double x, double y)
	{
		X = (float)x;
		Y = (float)y;
	}



	public static PointF operator *(float left, PointF right)
	{
		return right * left;
	}

	public static PointF operator *(PointF left, float right)
	{
		return new PointF(left.X * right, left.Y * right);
	}
	public static PointF operator /(PointF left, float right)
	{
		return new PointF(left.X / right, left.Y / right);
	}


	public static PointF operator +(PointF left, PointF right)
	{
		return new PointF(left.X + right.X, left.Y + right.Y);
	}
	public static PointF operator -(PointF left, PointF right)
	{
		return new PointF(left.X - right.X, left.Y - right.Y);
	}
	public static PointF operator *(PointF left, PointF right)
	{
		return new PointF(left.X * right.X, left.Y * right.Y);
	}
	public static PointF operator /(PointF left, PointF right)
	{
		return new PointF(left.X * right.X, left.Y * right.Y);
	}


	public static explicit operator System.Drawing.PointF(PointF p)
	{
		return new(p.X, p.Y);
	}
	public static explicit operator System.Drawing.Point(PointF p)
	{
		return new((int)Round(p.X), (int)Round(p.Y));
	}

	public static implicit operator PointF(System.Drawing.PointF p)
	{
		return new(p.X, p.Y);
	}
	public static implicit operator PointF(System.Drawing.Point p)
	{
		return new(p.X, p.Y);
	}
	public static implicit operator PointF(System.Windows.Point p)
	{
		return new(p.X, p.Y);
	}

	public bool Equals(PointF other)
	{
		return
			Round(X, 3) == Round(other.X, 3)
			&&
			Round(Y, 3) == Round(other.Y, 3);
	}

	public Point ToRounded()
	{
		return new((int)Round(X), (int)Round(Y));
	}

	public bool Equals(System.Drawing.PointF other)
	{
		return
			Round(X, 3) == Round(other.X, 3)
			&&
			Round(Y, 3) == Round(other.Y, 3);
	}

	public bool Equals(System.Windows.Point other)
	{
		return
			Round(X, 3) == Round((float)other.X, 3)
			&&
			Round(Y, 3) == Round((float)other.Y, 3);
	}

	public bool Equals(Point other)
	{
		return
			Round(X, 3) == other.X
			&&
			Round(Y, 3) == other.Y;
	}

	public virtual PointF Clone()
	{
		return new(X, Y);
	}
}
