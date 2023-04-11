using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicLibrary.MathModels;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class Dot : ALinearElement
{
	public PointF Point { get; private set; }

	public Dot(PointF point, Color color)
		: base(color, GetDefaultPatternResolver())
	{
		this.Point = point;
	}

	public Dot(System.Windows.Point point, Color color)
		: this(Common.WindowsToDrawing(point), color) { }

	#region inherited or overriden
	public override Dot Clone()
	{
		return new Dot(this.Point, this.Color);
	}
	public override void Move(float dX, float dY)
	{
		this.Point += new PointF(dX, dY);
	}
	public override void Rotate(float angleR, PointF relativeTo)
	{
		this.Point = Common.RotatePoint(this.Point, relativeTo, angleR);
	}
	public override void Scale(float scale, PointF relativeTo)
	{
		this.Point = Common.ScalePoint(Point, relativeTo, scale);
	}
	public override void Mirror(PointF relativeTo)
	{
		this.Point = Common.MirrorPoint(Point, relativeTo);
	}
	public override void Mirror(LineF relativeTo)
	{
		this.Point = Common.MirrorPoint(Point, relativeTo);
	}
	#endregion
}
