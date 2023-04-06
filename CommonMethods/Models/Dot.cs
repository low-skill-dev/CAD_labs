using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class Dot : ALinearElement
{
	public PointF Point { get; private set; }

	public Dot(System.Drawing.PointF point, Color color)
		: base(color, GetDefaultPatternResolver())
	{
		this.Point = point;
	}

	public Dot(System.Windows.Point point, Color color)
		: this(Common.WindowsToDrawing(point), color) { }

	#region inherited or overriden
	public override IGraphicalElement Clone()
	{
		return new Dot(this.Point, this.Color);
	}
	public override void MoveCoordinates(float dX, float dY)
	{
		this.Point += new SizeF(dX, dY);
	}
	public override void Rotate(float angleR, PointF relativeTo)
	{
		this.Point = Common.RotatePoint(this.Point, relativeTo, angleR);
	}
	public override void Scale(float scale, PointF relativeTo)
	{
		this.Point = Common.ScalePoint(Point, relativeTo, scale);
	}
	#endregion
}
