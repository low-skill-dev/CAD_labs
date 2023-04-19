using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System.Windows.Media.Imaging;
using static System.MathF;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using PointF = GraphicLibrary.MathModels.PointF;
using Rectangle = GraphicLibrary.Models.Rectangle;

namespace GraphicLibrary;
public class BitmapDrawer
{
	protected int FrameWidth { get; set; }
	protected int FrameHeight { get; set; }

	//private FastBitmap? CurrentFastLock { get; set; }
	public DirectBitmap CurrentFrame { get; init; }
	public BitmapImage CurrentFrameImage => Common.BitmapToImageSource(CurrentFrame.Bitmap);

	public List<Dot> Dots { get; private set; } = new();
	public List<Line> Lines { get; private set; } = new();
	public List<Circle> Circles { get; private set; } = new();
	public List<Arc> Arcs { get; private set; } = new();
	public List<Rectangle> Rectangles { get; private set; } = new();
	public List<AreaFiller> AreaFillers { get; private set; } = new();
	public List<InterpolatedPoints> InterpolatedPoints { get; private set; } = new();
	public List<InterpolatedPoints> LagrangePolys { get; private set; } = new();
	public List<InterpolatedPoints> Besie2Polys { get; private set; } = new();
	public List<IGraphicalElement> ConstantObjects { get; private set; } = new();

	public Color BackgroundColor { get; set; } = Color.FromArgb(0, 0, 0, 0);
	public bool AddAxes { get; set; } = true;

	public BitmapDrawer(int frameWidth, int frameHeight)
	{
		FrameWidth = frameWidth;
		FrameHeight = frameHeight;
		CurrentFrame = new(frameWidth, frameHeight);
	}

	#region public
	public void Reset(bool noClear = true)
	{
		Dots.Clear();
		Lines.Clear();
		Rectangles.Clear();
		Circles.Clear();
		Arcs.Clear();
		AreaFillers.Clear();
		InterpolatedPoints.Clear();
		LagrangePolys.Clear();
		Besie2Polys.Clear();
		if(!noClear) {
			CurrentFrame.Clear(Color.FromArgb(0, 0, 0, 0));
		}
	}

	public void RenderFrame(bool noClear = false)
	{
		//using var currentFastLock = this.CurrentFrame.FastLock();
		//this.CurrentFastLock = currentFastLock;

		if(!noClear) {
			CurrentFrame.Clear(BackgroundColor);
		}

		Dots.ForEach(DrawDot);
		Lines.ForEach(DrawLine);
		Rectangles.ForEach(DrawRectangle);
		Circles.ForEach(DrawCircle);
		Arcs.ForEach(DrawArc);
		AreaFillers.ForEach(x => FillAreaFrom(x));
		InterpolatedPoints.ForEach(DrawInterpolated);
		LagrangePolys.ForEach(DrawLagrange);
		Besie2Polys.ForEach(DrawBesie2);

		if(AddAxes) {
			var left = new PointF(0, FrameHeight / 2);
			var right = new PointF(FrameWidth, FrameHeight / 2);
			var up = new PointF(FrameWidth / 2, 0);
			var down = new PointF(FrameWidth / 2, FrameWidth);

			DrawLine(left, right, Color.Orange, ALinearElement.GetDefaultPatternResolver());
			DrawLine(up, down, Color.Orange, ALinearElement.GetDefaultPatternResolver());
		}
		foreach(var obj in ConstantObjects) {
			if(obj is null) {
				continue;
			}

			if(obj is Dot) {
				DrawDot((Dot)obj);
			}

			if(obj is Line) {
				DrawLine((Line)obj);
			}

			if(obj is Circle) {
				DrawCircle((Circle)obj);
			}

			if(obj is Rectangle) {
				DrawRectangle((Rectangle)obj);
			}
		}

		//this.CurrentFastLock = null;
	}

	#region no-render methods

	public void AddPoint(Dot dot)
	{
		Dots.Add(dot);
	}

	[Obsolete]
	public void AddPoint(PointF point, Color? color = null)
	{
		Dots.Add(new(point, color ?? Color.LightGreen));
	}
	public void AddLine(Line line)
	{
		Lines.Add(line);
	}

	[Obsolete]
	public void AddLine(PointF start, PointF end, Color? color = null, IEnumerator<bool>? patternResolver = null)
	{
		if(start.Equals(end)) {
			AddPoint(start, color);
			return;
		}

		Lines.Add(new(start, end, color ?? Color.LightGreen, patternResolver ?? ALinearElement.GetDefaultPatternResolver()));
	}
	public void AddRectangle(Rectangle rect)
	{
		Rectangles.Add(rect);
	}
	public void AddCircle(Circle circle)
	{
		Circles.Add(circle);
	}

	[Obsolete]
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
		InterpolatedPoints.Add(polynomial);
	}
	#endregion

	public IEnumerator<IGraphicalElement> GetElements()
	{
		foreach(var obj in Dots) {
			yield return obj;
		}

		foreach(var obj in Lines) {
			yield return obj;
		}

		foreach(var obj in Circles) {
			yield return obj;
		}

		foreach(var obj in AreaFillers) {
			yield return obj;
		}

		foreach(var obj in InterpolatedPoints) {
			yield return obj;
		}

		foreach(var obj in LagrangePolys) {
			yield return obj;
		}

		foreach(var obj in Besie2Polys) {
			yield return obj;
		}
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
			foreach(var obj in Dots) {
				Dots.Add(obj.Clone());
				Dots.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in Lines) {
				Lines.Add(obj.Clone());
				Lines.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in Circles) {
				Circles.Add(obj.Clone());
				Circles.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in AreaFillers) {
				AreaFillers.Add((AreaFiller)obj.Clone());
				AreaFillers.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in InterpolatedPoints) {
				InterpolatedPoints.Add(obj.Clone());
				InterpolatedPoints.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in LagrangePolys) {
				LagrangePolys.Add(obj.Clone());
				LagrangePolys.At(-1).Rotate(angleR, relativeTo);
			}
			foreach(var obj in Besie2Polys) {
				Besie2Polys.Add(obj.Clone());
				Besie2Polys.At(-1).Rotate(angleR, relativeTo);
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
		if(!(float.IsFinite(target.X) && float.IsFinite(target.Y))) {
			return false;
		}

		if(target.X < 0 || target.Y < 0) {
			return false;
		}

		if(target.X > CurrentFrame.Width - 1) {
			return false;
		}

		if(target.Y > CurrentFrame.Height - 1) {
			return false;
		}

		return true;
	}
	private void BypassPoint(PointF point, Color color, IEnumerator<bool> patternResolver)
	{
		if(isValidPoint(point) && patternResolver.MoveNext()) {
			if(patternResolver.Current) {
				CurrentFrame.SetPixel((int)Round(point.X), (int)Round(point.Y), color);
			}
		}
	}
	#endregion

	#region point
	private void DrawDot(Dot dot)
	{
		DrawDot(dot.Point, dot.Color, dot.PatternResolver);
	}

	private void DrawDot(PointF point, Color color, IEnumerator<bool> patternResolver)
	{
		BypassPoint((System.Drawing.Point)point, color, patternResolver);
	}
	#endregion

	#region line
	private float GetLineStep(float start, float end, float steps)
	{
		return (end - start) / steps;
	}

	private void DrawLine(Line line)
	{
		DrawLine(line.Start, line.End, line.Color, line.PatternResolver);
	}

	private void DrawLine(PointF start, PointF end, Color color, IEnumerator<bool> patternResolver)
	{
		var dX = Abs(end.X - start.X);
		var dY = Abs(end.Y - start.Y);

		if(dX > dY) { // we need to iterate  by X
			if(start.X > end.X) {
				Swap(ref start, ref end);
			}

			var stepY = GetLineStep(start.Y, end.Y, dX);
			for(var iX = start.X; iX <= end.X; iX++) {
				var currY = start.Y + ((iX - start.X) * stepY);
				BypassPoint((Point)new PointF(iX, currY), color, patternResolver);
			}
		} else { // else by Y
			if(start.Y > end.Y) {
				Swap(ref start, ref end);
			}

			var stepX = GetLineStep(start.X, end.X, dY);
			for(var iY = start.Y; iY <= end.Y; iY++) {
				var currX = start.X + ((iY - start.Y) * stepX);
				BypassPoint((Point)new PointF(currX, iY), color, patternResolver);
			}
		}
	}
	#endregion

	#region circle
	private void DrawCircle(Circle circle)
	{
		DrawCircle(circle.Center, circle.Radius, circle.Color, circle.PatternResolver);
	}

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
	#region Arc
	private void DrawArc(Arc arc)
	{
		var center = arc.Circle.Center;
		var radius = arc.Circle.Radius;
		var pattern = arc.Circle.PatternResolver;
		var color = arc.Circle.Color;
		var isNeg = arc.IsNegativeDirection;

		var len = 2 * PI * arc.Circle.Radius;
		var angleStep = 2 * PI / len;

		float
			start = arc.StartAngle,
			end = arc.EndAngle;

		if(start > end) {
			Swap(ref start, ref end);
			isNeg = !isNeg;
		}

		if(!isNeg) { // is positive direction
			for(var a = start; a < end; a += angleStep) {
				BypassPoint(Common.FindPointOnCircle(center, radius, a), color, pattern);
			}
		} else {
			for(var a = start; a > (end - (2 * PI)); a -= angleStep) {
				BypassPoint(Common.FindPointOnCircle(center, radius, a), color, pattern);
			}
		}
	}
	#endregion

	#region fillArea
	private void FillAround(Point start, Color newColor, Color baseColor)
	{
		if(CurrentFrame.GetPixel(start.X, start.Y) != baseColor) {
			baseColor = CurrentFrame.GetPixel(start.X, start.Y);
		}

		Stack<Point> points = new();
		points.Push(start);

		var maxCount = 0;

		while(points.TryPop(out var point)) {
			if(!isValidPoint(point)) {
				continue;
			}

			if(points.Count > maxCount) {
				maxCount = points.Count;
			}

			BypassPoint(new(point.X, point.Y), newColor, ALinearElement.GetDefaultPatternResolver());
			for(var dx = -1; dx < 2; dx += 2) {
				Point target = new(point.X + dx, point.Y);
				if(isValidPoint(target)) {
					if(CurrentFrame.GetPixel(target.X, target.Y).Equals(baseColor)) {
						points.Push(target);
					}
				}
			}
			for(var dy = -1; dy < 2; dy += 2) {
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
		for(var i = 0; i < lines.Length - 1; i++) {
			var curr = lines[i];
			var next = lines[i + 1];
			DrawLine(curr, next, poly.Color, resolver);

			if(poly.DebugDraw) {
				DrawCircle(curr, 3, Color.Yellow, ALinearElement.GetDefaultPatternResolver());
			}
		}

		DrawLine(lines[^1], lines[0], poly.Color, poly.PatternResolver);
		if(poly.DebugDraw) {
			DrawCircle(lines[^1], 3, Color.Yellow, ALinearElement.GetDefaultPatternResolver());
		}
	}
	#endregion

	#region Lagrange
	private void DrawLagrange(InterpolatedPoints poly)
	{
		float x = 0;
		var y = poly.CalcLagrangeY(x);
		PointF curr, prev = new(x, y);

		var step = poly.StepsBetweenPoints;
		for(x = 1; x < FrameWidth; x += step) {
			y = poly.CalcLagrangeY(x);
			curr = new(x, y);

			if(isValidPoint(prev) && isValidPoint(curr)) {
				DrawLine(prev, curr, poly.Color, poly.PatternResolver);
			}

			prev = curr;

			if(poly.DebugDraw) {
				DrawCircle(curr, 3, Color.Yellow, ALinearElement.GetDefaultPatternResolver());
			}
		}
	}
	#endregion
	#region Besie2
	private void DrawBesie2(InterpolatedPoints poly)
	{
		var points = poly.Points;
		var lines = Besie.Calculate(points, (int)Round(poly.StepsBetweenPoints), poly.BendingFactor, poly.FirstSplineCorrection);

		var resolver = poly.PatternResolver;
		for(var i = 0; i < lines.Count - 1; i++) {
			var curr = lines[i];
			var next = lines[i + 1];
			DrawLine(curr, next, poly.Color, resolver);

			if(poly.DebugDraw) {
				DrawCircle(curr, 3, Color.Yellow, ALinearElement.GetDefaultPatternResolver());
			}
		}

		DrawLine(lines[^1], lines[0], poly.Color, poly.PatternResolver);
		if(poly.DebugDraw) {
			DrawCircle(lines[^1], 3, Color.Yellow, ALinearElement.GetDefaultPatternResolver());
		}
	}
	#endregion
	#region rectangle
	private void DrawRectangle(Rectangle rect)
	{
		var start = rect.Start;
		var end = rect.End;

		var right = new PointF(end.X, start.Y);
		var left = new PointF(start.X, end.Y);

		DrawLine(start, right, rect.Color, rect.PatternResolver);
		DrawLine(right, end, rect.Color, rect.PatternResolver);
		DrawLine(end, left, rect.Color, rect.PatternResolver);
		DrawLine(left, start, rect.Color, rect.PatternResolver);
	}
	#endregion

	#endregion
}
