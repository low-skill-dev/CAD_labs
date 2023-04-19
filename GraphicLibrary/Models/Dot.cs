using GraphicLibrary.MathModels;
using System.Drawing;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class Dot : ALinearElement
{
	public PointF Point { get; private set; }

	public Dot(PointF point, Color color)
		: base(color, GetDefaultPatternResolver())
	{
		Point = point;
	}

	public Dot(System.Windows.Point point, Color color)
		: this(Common.WindowsToDrawing(point), color) { }

	#region inherited or overriden
	public override Dot Clone()
	{
		return new Dot(Point, Color);
	}
	public override void Move(float dX, float dY)
	{
		Point += new PointF(dX, dY);
	}
	public override void Rotate(float angleR, PointF relativeTo)
	{
		Point = Common.RotatePoint(Point, relativeTo, angleR);
	}
	public override void Scale(float scale, PointF relativeTo)
	{
		Point = Common.ScalePoint(Point, relativeTo, scale);
	}
	public override void Mirror(PointF relativeTo)
	{
		Point = Common.MirrorPoint(Point, relativeTo);
	}
	public override void Mirror(LineF relativeTo)
	{
		Point = Common.MirrorPoint(Point, relativeTo);
	}
	#endregion
}
