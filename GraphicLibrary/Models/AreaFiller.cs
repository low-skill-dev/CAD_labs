using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class AreaFiller : IGraphicalElement, IColoredElement
{
	public PointF StartFilling { get; private set; }
	public Color Color { get; init; }

	public AreaFiller(PointF start, Color color)
	{
		this.StartFilling = start;
		this.Color = color;
	}


	public void Move(float dX, float dY)
	{
		StartFilling += new SizeF(dX, dY);
	}
	public void Rotate(float angleR, PointF relativeTo)
	{
		StartFilling = Common.RotatePoint(StartFilling, relativeTo, angleR);
	}
	public void Scale(float scale, PointF relativeTo)
	{
		StartFilling = Common.ScalePoint(StartFilling,relativeTo,scale);
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
