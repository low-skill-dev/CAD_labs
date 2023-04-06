using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class Line : ALinearElement
{
	public PointF Start { get; private set; }
	public PointF End { get; private set; }

	// 16. Штрихпунктирная линия 8: линия из 3-х пикселей, пропуск 3-х пикселей, пиксель, пропуск 3-х пикселей…
	public static IEnumerator<bool> GetPatternResolver16()
	{
		int i;
		while(true) {
			for(i = 0; i < 3; i++) yield return true; // линия из 3-х пикселей
			for(i = 0; i < 3; i++) yield return false; // пропуск 3-х пикселей
			for(i = 0; i < 1; i++) yield return true; // пиксель
			for(i = 0; i < 3; i++) yield return false; // пропуск 3-х пикселей
		}
	}

	// 16. Штриховая линия с разными штрихами 8: линия из 2-х пикселей, пропуск 4-х пикселей, линия из 3-х пикселей, пропуск 4-х пикселей…
	public static IEnumerator<bool> GetBresenhamPatternResolver16()
	{
		int i;
		while(true) {
			for(i = 0; i < 2; i++) yield return true; // линия из 2-х пикселей
			for(i = 0; i < 4; i++) yield return false; // пропуск 4-х пикселей
			for(i = 0; i < 3; i++) yield return true; // линия из 3-х пикселей
			for(i = 0; i < 4; i++) yield return false; // пропуск 4-х пикселей
		}
	}

	public Line(System.Drawing.PointF start, System.Drawing.PointF end, Color color, IEnumerator<bool>? patternResolver = null)
		: base(color, patternResolver)
	{
		this.Start = start;
		this.End = end;
	}

	public Line(System.Windows.Point start, System.Windows.Point end, Color color, IEnumerator<bool>? patternResolver = null)
		: this(Common.WindowsToDrawing(start), Common.WindowsToDrawing(end), color, patternResolver) { }

	#region inherited or overriden
	public override IGraphicalElement Clone()
	{
		return new Line(this.Start, this.End, this.Color, this.PatternResolver);
	}
	public override void MoveCoordinates(float dX, float dY)
	{
		this.Start += new SizeF(dX, dY);
		this.End += new SizeF(dX, dY);
	}
	public override void Rotate(float angleR, PointF relativeTo)
	{
		this.Start = Common.RotatePoint(this.Start, relativeTo, angleR);
		this.End = Common.RotatePoint(this.End, relativeTo, angleR);
	}
	public override void Scale(float scale, PointF relativeTo)
	{
		this.Start = Common.ScalePoint(Start, relativeTo, scale);
		this.End = Common.ScalePoint(End, relativeTo, scale);
	}
	#endregion
}
