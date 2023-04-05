using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public sealed class Circle : ALinearElement
{
	#region self-contained
	public Point Center { get; private set; }
	public int Radius { get; init; }

	// 16. Штриховая линия 8: линия из 4-х пикселей, пропуск 4-х пикселей…
	public static IEnumerator<bool> GetPatternResolver16()
	{
		int i;
		while(true) {
			for(i = 0; i < 4; i++) yield return true; // линия из 4-х пикселей
			for(i = 0; i < 4; i++) yield return false; // пропуск 4-х пикселей
		}
	}
	// 16. Линия сплошная.
	public static IEnumerator<bool> GetBresenhamPatternResolver16()
	{
		int i;
		while(true) {
			 yield return true; // Линия сплошная.
		}
	}

	public static float GetCirleRadius(Point center, Point onCirle)
	{
		var dX = onCirle.X - center.X;
		var dY = onCirle.Y - center.Y;
		return MathF.Sqrt(dX * dX + dY * dY);
	}

	// by predefined radius
	public Circle(System.Drawing.Point center, int radius, Color color, IEnumerator<bool>? patternResolver = null)
		: base(color, patternResolver)
	{
		this.Center = center;
		this.Radius = radius;
	}
	public Circle(System.Windows.Point center, int radius, Color color, IEnumerator<bool>? patternResolver = null)
		: this(Common.WindowsToDrawing(center), radius, color, patternResolver) { }

	// by center and point on circle
	public Circle(System.Drawing.Point center, System.Drawing.Point onCircle, Color color, IEnumerator<bool>? patternResolver = null)
		: this(center, (int)GetCirleRadius(center, onCircle),color, patternResolver) { }
	public Circle(System.Windows.Point center, System.Windows.Point onCircle, Color color, IEnumerator<bool>? patternResolver = null)
		: this(Common.WindowsToDrawing(center), Common.WindowsToDrawing(onCircle), color, patternResolver) { }
	#endregion

	#region inherited or overriden
	public override IGraphicalElement Clone()
	{
		return new Circle(this.Center,this.Radius,this.Color,this.PatternResolver);
	}
	public override void MoveCoordinates(int dX, int dY)
	{
		this.Center += new Size(dX, dY);
	}
	public override void Rotate(float angleR, Point relativeTo) // basically rotate the center
	{
		this.Center = Common.RotatePoint(this.Center,relativeTo,angleR);
	}
	#endregion
}
