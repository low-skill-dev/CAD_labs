using FastBitmapLib;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using static System.MathF;
using static System.Net.Mime.MediaTypeNames;
using Point = System.Drawing.Point;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary;
public class BitmapDrawer
{
	private int FrameWidth { get; set; }
	private int FrameHeight { get; set; }
	public Bitmap CurrentFrame { get; private set; }
	public BitmapImage CurrentFrameImage => Common.BitmapToImageSource(CurrentFrame);

	public List<Dot> Dots { get; private set; } = new();
	public List<Line> Lines { get; private set; } = new();
	public List<Circle> Circles { get; private set; } = new();
	public List<AreaFiller> AreaFillers { get; private set; } = new();
	public List<InterpolatedPoints> InterpolatedPoints { get; private set; } = new();
	public List<InterpolatedPoints> LagrangePolys { get; private set; } = new();
	public List<IGraphicalElement> ConstantObjects { get; private set; } = new();

	public bool AddAxes { get; set; } = true;

	public BitmapDrawer(int frameWidth, int frameHeight)
	{
		this.FrameWidth = frameWidth;
		this.FrameHeight = frameHeight;
		this.CurrentFrame = new Bitmap(frameWidth, frameHeight);
	}

	#region public
	public void Reset()
	{
		this.Dots.Clear();
		this.Lines.Clear();
		this.Circles.Clear();
		this.AreaFillers.Clear();
		this.InterpolatedPoints.Clear();
		this.LagrangePolys.Clear();
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
	}

	public void RenderFrame(bool noClear = false)
	{
		if(!noClear) this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);

		if(AddAxes) {
			var left = new PointF(0, FrameHeight / 2);
			var right = new PointF(FrameWidth, FrameHeight / 2);
			var up = new PointF(FrameWidth / 2, 0);
			var down = new PointF(FrameWidth / 2, FrameWidth);

			this.DrawLine(left, right, Color.Orange, ALinearElement.GetDefaultPatternResolver());
			this.DrawLine(up, down, Color.Orange, ALinearElement.GetDefaultPatternResolver());
		}

		this.Dots.ForEach(DrawDot);
		this.Lines.ForEach(DrawLine);
		this.Circles.ForEach(DrawCircle);
		this.AreaFillers.ForEach(x => FillAreaFrom(x));
		this.InterpolatedPoints.ForEach(x => DrawInterpolated(x));
		this.LagrangePolys.ForEach(x => DrawLagrange(x));
		foreach(var obj in ConstantObjects) {
			if(obj is null) continue;

			if(obj is Dot) this.DrawDot((Dot)obj);
			if(obj is Line) this.DrawLine((Line)obj);
			if(obj is Circle) this.DrawCircle((Circle)obj);
		}
	}

	public void AddPoint(PointF point, Color? color = null)
	{
		Dots.Add(new(point, color ?? Color.LightGreen));
	}
	public void AddLine(PointF start, PointF end, Color? color = null, IEnumerator<bool>? patternResolver = null)
	{
		if(start.Equals(end)) {
			AddPoint(start, color);
			return;
		}

		Lines.Add(new(start, end, color ?? Color.LightGreen, patternResolver ?? ALinearElement.GetDefaultPatternResolver()));
	}
	public void AddCircle(PointF center, PointF onCircle, Color? color = null, IEnumerator<bool>? patternResolver = null)
	{
		if(center.Equals(onCircle)) {
			AddPoint(center, color);
			return;
		}

		Circles.Add(new(center, onCircle, color ?? Color.LightGreen, patternResolver ?? ALinearElement.GetDefaultPatternResolver()));
	}
	public void AddFiller(AreaFiller filler)
	{
		AreaFillers.Add(filler);
	}
	public void AddLagrangePolynomial(InterpolatedPoints polynomial)
	{
		this.InterpolatedPoints.Add(polynomial);
	}

	public IEnumerator<IGraphicalElement> GetElements()
	{
		for(int i = 0; i < Dots.Count; i++) yield return Dots[i];
		for(int i = 0; i < Lines.Count; i++) yield return Lines[i];
		for(int i = 0; i < Circles.Count; i++) yield return Circles[i];
	}

	public void MoveAll(float dX, float dY)
	{
		var elements = GetElements();

		while(elements.MoveNext()) {
			elements.Current.Move(dX, dY);
		}
	}
	public void RotateAll(float angleR, PointF relativeTo, bool byCopy = false)
	{
		var elements = GetElements();

		if(!byCopy) {
			while(elements.MoveNext()) {
				elements.Current.Rotate(angleR, relativeTo);
			}
		} else {
			List<IGraphicalElement> enumerated = new();
			while(elements.MoveNext()) {
				enumerated.Add(elements.Current);
			}

			foreach(var element in enumerated) {
				var copy = element.Clone();
				copy.Rotate(angleR, relativeTo);

				if(copy is Dot) this.Dots.Add((Dot)copy);
				if(copy is Line) this.Lines.Add((Line)copy);
				if(copy is Circle) this.Circles.Add((Circle)copy);
			}
		}
	}
	public void ScaleAll(float scale, PointF relativeTo)
	{
		var elements = GetElements();

		while(elements.MoveNext()) {
			elements.Current.Scale(scale, relativeTo);
		}
	}
	public void MirrorAll(PointF relativeTo)
	{
		var elements = GetElements();

		while(elements.MoveNext()) {
			elements.Current.Mirror(relativeTo);
		}
	}
	public void MirrorAll(LineF relativeTo)
	{
		var elements = GetElements();

		while(elements.MoveNext()) {
			elements.Current.Mirror(relativeTo);
		}
	}

	#endregion


	#region private

	#region common
	private static void Swap<T>(ref T v1, ref T v2)
	{
		var temp = v1;
		v1 = v2;
		v2 = temp;
	}
	private static Point RoundPoint(PointF point)
	{
		return point.ToRounded();
	}
	private bool isValidPoint(PointF target)
	{
		if(!(float.IsFinite(target.X) && float.IsFinite(target.Y))) return false;

		if(target.X < 0 || target.Y < 0) return false;
		if(target.X > this.CurrentFrame.Width - 1) return false;
		if(target.Y > this.CurrentFrame.Height - 1) return false;

		return true;
	}
	private void BypassPoint(PointF point, Color color, IEnumerator<bool> patternResolver)
	{
		if(isValidPoint(point)) {
			patternResolver.MoveNext();
			if(patternResolver.Current)
				this.CurrentFrame.SetPixel((int)Round(point.X), (int)Round(point.Y), color);
		}
	}
	#endregion

	#region point
	private void DrawDot(Dot dot) => DrawDot(dot.Point, dot.Color, dot.PatternResolver);
	private void DrawDot(PointF point, Color color, IEnumerator<bool> patternResolver)
	{
		BypassPoint((System.Drawing.Point)point, color, patternResolver);
	}
	#endregion

	#region line
	private float GetLineStep(float start, float end, float steps) => (end - start) / steps;
	private void DrawLine(Line line) => DrawLine(line.Start, line.End, line.Color, line.PatternResolver);
	private void DrawLine(PointF start, PointF end, Color color, IEnumerator<bool> patternResolver)
	{
		var dX = Abs(end.X - start.X);
		var dY = Abs(end.Y - start.Y);

		if(dX > dY) { // we need to iterate  by X
			if(start.X > end.X) Swap(ref start, ref end);

			float stepY = GetLineStep(start.Y, end.Y, dX);
			for(float iX = start.X; iX <= end.X; iX++) {
				float currY = start.Y + (iX - start.X) * stepY;
				BypassPoint((Point)new PointF(iX, currY), color, patternResolver);
			}
		} else { // else by Y
			if(start.Y > end.Y) Swap(ref start, ref end);

			float stepX = GetLineStep(start.X, end.X, dY);
			for(float iY = start.Y; iY <= end.Y; iY++) {
				float currX = start.X + (iY - start.Y) * stepX;
				BypassPoint((Point)new PointF(currX, iY), color, patternResolver);
			}
		}
	}
	#endregion

	#region circle
	private void DrawCircle(Circle circle) => DrawCircle(circle.Center, circle.Radius, circle.Color, circle.PatternResolver);
	private void DrawCircle(PointF center, float radius, Color color, IEnumerator<bool> patternResolver)
	{
		var len = 2 * PI * radius;
		var angleStep = 2 * PI / len;

		for(float a = 0; a < (2 * PI); a += angleStep) {
			var point = Common.FindPointOnCircle(center, radius, a);
			var rounded = new Point((int)Round(point.X), (int)Round(point.Y));
			BypassPoint(rounded, color, patternResolver);
		}
	}
	#endregion

	#region fillArea
	private void FillAround(Point start, Color newColor, Color baseColor)
	{
		Stack<Point> points = new();
		points.Push(start);

		while(points.TryPop(out var point)) {
			CurrentFrame.SetPixel(point.X, point.Y, newColor);
			for(int dx = -1; dx < 2; dx += 2) {
				Point target = new(point.X + dx, point.Y);
				if(isValidPoint(target)) {
					if(CurrentFrame.GetPixel(target.X, target.Y).Equals(baseColor)) {
						points.Push(target);
					}
				}
			}
			for(int dy = -1; dy < 2; dy += 2) {
				Point target = new(point.X, point.Y + dy);
				if(isValidPoint(target)) {
					if(CurrentFrame.GetPixel(target.X, target.Y).Equals(baseColor)) {
						points.Push(target);
					}
				}
			}
		}
	}
	private void FillAreaFrom(AreaFiller filler, Color? baseColor = null)
	{
		FillAround(RoundPoint(filler.StartFilling), filler.Color, baseColor ?? Color.FromArgb(0, 0, 0, 0));
	}
	#endregion

	#region Interpolation
	private void DrawInterpolated(InterpolatedPoints poly)
	{
		var points = poly.Points;
		var lines = Spline.Calculate(points.Select(x => new PointF(x.X, x.Y)).ToArray(), poly.Degree, (int)Round(poly.StepsBetweenPoints));

		var resolver = poly.PatternResolver;
		for(int i = 0; i < lines.Length - 1; i++) {
			var curr = lines[i];
			var next = lines[i + 1];
			DrawLine(curr, next, Color.Aqua, resolver);
		}

		DrawLine(lines[lines.Length - 1], lines[0], Color.Aqua, poly.PatternResolver);
	}
	#endregion

	#region Lagrange
	private void DrawLagrange(InterpolatedPoints poly)
	{
		float x = 0;
		float y = poly.CalcLagrangeY(x);
		PointF curr, prev = new(x, y);

		float step = poly.StepsBetweenPoints;
		for(x = 1; x < this.FrameWidth; x += step) {
			y = poly.CalcLagrangeY(x);
			curr = new(x, y);

			if(isValidPoint(prev) && isValidPoint(curr))
				DrawLine(prev, curr, poly.Color, poly.PatternResolver);

			prev = curr;
		}
	}
	#endregion

	#endregion
}
