using GraphicLibrary.Models;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Windows.Media.Imaging;
using static System.MathF;

namespace GraphicLibrary;
public class BitmapDrawer
{
	private static void Swap<T>(ref T v1, ref T v2)
	{
		var temp = v1;
		v1 = v2;
		v2 = temp;
	}

	public event EventHandler? FrameUpdated;


	public int FrameWidth { get; private set; }
	public int FrameHeight { get; private set; }
	public Bitmap CurrentFrame { get; private set; }
	public BitmapImage CurrentFrameImage => Common.BitmapToImageSource(CurrentFrame);
	/*
	private Graphics FrameGraphics { get; set; }
	public SolidBrush CommonElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.LightBlue);
	public SolidBrush SelectedElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.Blue);
	public SolidBrush InactiveElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.DarkGray);
	public SolidBrush BackgroundBrush { get; set; } = new SolidBrush(System.Drawing.Color.WhiteSmoke);
	*/


	private List<Dot> Dots { get; set; } = new();
	private List<Line> Lines { get; set; } = new();
	private List<Circle> Circles { get; set; } = new();

	public BitmapDrawer(int FrameWidth, int FrameHeight)
	{
		this.FrameWidth = FrameWidth;
		this.FrameHeight = FrameHeight;
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
	}

	#region public
	public void Reset()
	{
		this.Dots.Clear();
		this.Lines.Clear();
		this.Circles.Clear();
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
	}

	public void RenderFrame(bool addAxes=true, bool noClear = false)
	{
		if(!noClear) this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
		this.Dots.ForEach(x => DrawDot(x.Point, x.Color, x.PatternResolver));
		this.Lines.ForEach(x => DrawLine(x.Start, x.End, x.Color, x.PatternResolver));
		this.Circles.ForEach(x => DrawCircle(x.Center, x.Radius, x.Color, x.PatternResolver));
		if(addAxes) {
			var left = new PointF(0, FrameHeight / 2);
			var right = new PointF(FrameWidth, FrameHeight / 2);
			var up = new PointF(FrameWidth / 2, 0);
			var down = new PointF(FrameWidth / 2, FrameWidth);

			this.DrawLine(left, right, Color.Orange, ALinearElement.GetDefaultPatternResolver());
			this.DrawLine(up, down, Color.Orange, ALinearElement.GetDefaultPatternResolver());
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
			elements.Current.MoveCoordinates(dX, dY);
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
			elements.Current.Scale(scale,relativeTo);
		}
	}

	#endregion


	#region private

	#region common
	private bool isValidPoint(PointF target)
	{
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
	private void DrawDot(PointF point, Color color, IEnumerator<bool> patternResolver)
	{
		BypassPoint(point, color, patternResolver);
	}
	#endregion

	#region line
	private float GetLineStep(float start, float end, float steps) => (end - start) / steps;
	private void DrawLine(PointF start, PointF end, Color color, IEnumerator<bool> patternResolver)
	{
		var dX = Abs(end.X - start.X);
		var dY = Abs(end.Y - start.Y);

		if(dX > dY) { // we need to iterate  by X
			if(start.X > end.X) Swap(ref start, ref end);

			float stepY = GetLineStep(start.Y, end.Y, dX);
			for(float iX = start.X; iX <= end.X; iX++) {
				float currY = start.Y + (iX - start.X) * stepY;
				BypassPoint(new(iX, currY), color, patternResolver);
			}
		} else { // else by Y
			if(start.Y > end.Y) Swap(ref start, ref end);

			float stepX = GetLineStep(start.X, end.X, dY);
			for(float iY = start.Y; iY <= end.Y; iY++) {
				float currX = start.X + (iY - start.Y) * stepX;
				BypassPoint(new(currX, iY), color, patternResolver);
			}
		}
	}
	#endregion

	#region circle
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



	#endregion
}
