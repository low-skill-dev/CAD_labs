using GraphicLibrary.MathModels;
using System.Drawing;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public sealed class Circle : ALinearElement
{
	#region self-contained
	public PointF Center { get; private set; }
	public float Radius { get; private set; }

	// 16. Штриховая линия 8: линия из 4-х пикселей, пропуск 4-х пикселей…
	public static IEnumerator<bool> GetPatternResolver16()
	{
		int i;
		while(true) {
			for(i = 0; i < 4; i++) {
				yield return true; // линия из 4-х пикселей
			}

			for(i = 0; i < 4; i++) {
				yield return false; // пропуск 4-х пикселей
			}
		}
	}
	// 16. Линия сплошная.
	public static IEnumerator<bool> GetBresenhamPatternResolver16()
	{
		while(true) {
			yield return true; // Линия сплошная.
		}
	}

	// by predefined radius
	public Circle(PointF center, float radius, Color color, IEnumerator<bool>? patternResolver = null)
		: base(color, patternResolver)
	{
		Center = center;
		Radius = radius;
	}
	// by center and point on circle
	public Circle(PointF center, PointF onCircle, Color color, IEnumerator<bool>? patternResolver = null)
		: this(center, Common.GetCirleRadius(center, onCircle), color, patternResolver) { }
	//by 3 points
	public Circle(PointF p1, PointF p2, PointF p3, Color color, IEnumerator<bool>? patternResolver = null)
		: this(Common.FindCenter(p1, p2, p3), p1, color, patternResolver) { }
	#endregion

	#region inherited or overriden
	public override Circle Clone()
	{
		return new Circle(Center, Radius, Color, PatternResolver);
	}
	public override void Move(float dX, float dY)
	{
		Center += new PointF(dX, dY);
	}
	public override void Rotate(float angleR, PointF relativeTo) // basically rotate the center
	{
		Center = Common.RotatePoint(Center, relativeTo, angleR);
	}
	public override void Scale(float scale, PointF relativeTo)
	{
		Center = Common.ScalePoint(Center, relativeTo, scale);
		Radius *= scale;
	}
	public override void Mirror(PointF relativeTo)
	{
		Center = Common.MirrorPoint(Center, relativeTo);
	}
	public override void Mirror(LineF relativeTo)
	{
		Center = Common.MirrorPoint(Center, relativeTo);
	}
	#endregion
}
