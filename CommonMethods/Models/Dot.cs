using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class Dot : ALinearElement
{
	public Point Point { get; private set; }

	public static IEnumerator<bool> GetPatternResolver()
	{
		while(true) yield return true;
	}

	public Dot(System.Drawing.Point point, Color color)
		: base(color, GetPatternResolver())
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
	public override void MoveCoordinates(int dX, int dY)
	{
		this.Point += new Size(dX, dY);
	}
	public override void Rotate(float angleR, Point relativeTo)
	{
		this.Point = Common.RotatePoint(this.Point, relativeTo, angleR);
	}
	#endregion
}
