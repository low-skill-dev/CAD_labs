﻿using GraphicLibrary.MathModels;
using System.Drawing;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class AreaFiller : IGraphicalElement, IColoredElement
{
	public PointF StartFilling { get; private set; }
	public Color Color { get; init; }

	public AreaFiller(PointF start, Color color)
	{
		StartFilling = start;
		Color = color;
	}


	public void Move(float dX, float dY)
	{
		StartFilling += new PointF(dX, dY);
	}
	public void Rotate(float angleR, PointF relativeTo)
	{
		StartFilling = Common.RotatePoint(StartFilling, relativeTo, angleR);
	}
	public void Scale(float scale, PointF relativeTo)
	{
		StartFilling = Common.ScalePoint(StartFilling, relativeTo, scale);
	}
	public void Mirror(PointF relativeTo)
	{
		StartFilling = Common.MirrorPoint(StartFilling, relativeTo);
	}
	public void Mirror(LineF relativeTo)
	{
		StartFilling = Common.MirrorPoint(StartFilling, relativeTo);
	}
	public IGraphicalElement Clone()
	{
		return new AreaFiller(StartFilling, Color);
	}
}
