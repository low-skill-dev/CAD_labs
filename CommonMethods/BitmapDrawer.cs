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
	private Graphics FrameGraphics { get; set; }
	/*
	public SolidBrush CommonElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.LightBlue);
	public SolidBrush SelectedElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.Blue);
	public SolidBrush InactiveElementBrush { get; set; } = new SolidBrush(System.Drawing.Color.DarkGray);
	public SolidBrush BackgroundBrush { get; set; } = new SolidBrush(System.Drawing.Color.WhiteSmoke);
	*/


	private List<Line> Lines { get; set; } = new();
	private List<Circle> Circles { get; set; } = new();

	public BitmapDrawer(int FrameWidth, int FrameHeight)
	{
		this.FrameWidth = FrameWidth;
		this.FrameHeight = FrameHeight;
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
		this.FrameGraphics = Graphics.FromImage(this.CurrentFrame);
	}

	#region public
	public void Reset()
	{
		this.Lines.Clear();
		this.Circles.Clear();
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
	}

	public void RenderFrame(bool noClear = false)
	{
		if(!noClear) this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
		this.Lines.ForEach(x => DrawLine(x.Start, x.End, x.Color, x.PatternResolver));
		this.Circles.ForEach(x => DrawCircle(x.Center, x.Radius, x.Color, x.PatternResolver));
	}

	public void AddLine(Point start, Point end, Color? color = null, IEnumerator<bool>? patternResolver = null)
	{
		Lines.Add(new(start, end, color ?? Color.LightGreen, patternResolver ?? ALinearElement.GetDefaultPatternResolver()));
	}

	public void AddCircle(Point center, Point onCircle, Color? color = null, IEnumerator<bool>? patternResolver = null)
	{
		Circles.Add(new(center, onCircle, color ?? Color.LightGreen, patternResolver ?? ALinearElement.GetDefaultPatternResolver()));
	}
	#endregion


	#region private
	private float GetLineStep(int start, int end, int steps) => (end - start) / (float)steps;
	private void DrawLine(Point start, Point end, Color color, IEnumerator<bool> patternResolver)
	{
		var dX = Abs(end.X - start.X);
		var dY = Abs(end.Y - start.Y);

		if(dX > dY) { // we need to iterate  by X
			if(start.X > end.X) Swap(ref start, ref end);

			float stepY = GetLineStep(start.Y, end.Y, (int)dX);
			for(int iX = start.X; iX <= end.X; iX++) {
				float currY = start.Y + (iX - start.X) * stepY;

				patternResolver.MoveNext();
				if(patternResolver.Current)
					this.CurrentFrame.SetPixel(iX, (int)Math.Max(0, currY), color);
			}
		} else { // else by Y
			if(start.Y > end.Y) Swap(ref start, ref end);

			float stepX = GetLineStep(start.X, end.X, (int)dY);
			for(int iY = start.Y; iY <= end.Y; iY++) {
				float currX = start.X + (iY - start.Y) * stepX;

				patternResolver.MoveNext();
				if(patternResolver.Current)
					this.CurrentFrame.SetPixel((int)Math.Max(0, currX), iY, color);
			}
		}
	}



	private bool isValidPoint(Point target)
	{
		if(target.X < 0 || target.Y < 0) return false;
		if(target.X > this.CurrentFrame.Width - 1) return false;
		if(target.Y > this.CurrentFrame.Height - 1) return false;

		return true;
	}
	private void BypassPoint(Point point, Color color, IEnumerator<bool> patternResolver)
	{
		if(isValidPoint(point)) {
			patternResolver.MoveNext();
			if(patternResolver.Current)
				this.CurrentFrame.SetPixel(point.X, point.Y, color);
		}
	}
	/* x^2 + y^2 = r^2
	 * y = sqrt(r^2-x^2)
	 * x = sqrt(r^2-y^2)
	 * Суть алгоритма: https://imgur.com/a/58tXokQ
	 * Все данные циклы никак невозможно замерджить в 1, ибо это ломает паттерн окружности.
	 * В случае окружности сполной линии алгоритм будет в 8 раз проще.
	 * Рабочий порядок обхода: 1 -> 4 -> 3 -> 2 (по кругу).
	 */
	private IEnumerator<Point> GetOneFourthPoints(Point center, float radius, int part)
	{
		float 
			iterateX = 0, iterateY = 0, 
			relativeX = 0, relativeY = 0, 
			absoluteX = 0, absoluteY = 0;

		int 
			kX = 0, kY = 0;

		if(part == 1) {
			kX = 1; kY = -1; 

			for(iterateX = 0; iterateX <= radius / 2; iterateX++) {
				relativeX = iterateX;
				relativeY = Sqrt(Abs(radius * radius - relativeX * relativeX)); // X is increasing

				absoluteX = center.X + kX * relativeX;
				absoluteY = center.Y + kY * relativeY;

				yield return new((int)absoluteX, (int)absoluteY);
			}
			for(iterateY = relativeY; iterateY >= 0; iterateY--) { // Y is increasing 
				relativeY = iterateY;
				relativeX = Sqrt(Abs(radius * radius - relativeY * relativeY));

				absoluteY = center.Y + kY * iterateY;
				absoluteX = center.X + kX * relativeX;

				yield return new((int)absoluteX, (int)absoluteY);
			}

			yield break;
		}
		if(part == 4) {
			kX = 1; kY = 1;

			for(iterateY = 0; iterateY <= radius / 2; iterateY++) { // Y is increasing 
				relativeY = iterateY;
				relativeX = Sqrt(Abs(radius * radius - relativeY * relativeY));

				absoluteY = center.Y + kY * iterateY;
				absoluteX = center.X + kX * relativeX;

				yield return new((int)absoluteX, (int)absoluteY);
			}
			for(iterateX = relativeX; iterateX >= 0; iterateX--) { // X is decreasing
				relativeX = iterateX;
				relativeY = Sqrt(Abs(radius * radius - relativeX * relativeX));

				absoluteX = center.X + kX * relativeX;
				absoluteY = center.Y + kY * relativeY;

				yield return new((int)absoluteX, (int)absoluteY);
			}

			yield break;
		}
		if(part == 3) {
			kX = -1; kY = 1;

			for(iterateX = 0; iterateX <= radius / 2; iterateX++) {
				relativeX = iterateX;
				relativeY = Sqrt(Abs(radius * radius - relativeX * relativeX)); // X is increasing

				absoluteX = center.X + kX * relativeX;
				absoluteY = center.Y + kY * relativeY;

				yield return new((int)absoluteX, (int)absoluteY);
			}
			for(iterateY = relativeY; iterateY >= 0; iterateY--) { // Y is increasing 
				relativeY = iterateY;
				relativeX = Sqrt(Abs(radius * radius - relativeY * relativeY));

				absoluteY = center.Y + kY * iterateY;
				absoluteX = center.X + kX * relativeX;

				yield return new((int)absoluteX, (int)absoluteY);
			}

			yield break;
		}
		if(part == 2) {
			kX = -1; kY = -1;

			for(iterateY = 0; iterateY <= radius / 2; iterateY++) {
				relativeY = iterateY;
				relativeX = Sqrt(Abs(radius * radius - relativeY * relativeY));

				absoluteY = center.Y + kY * iterateY;
				absoluteX = center.X + kX * relativeX;

				yield return new((int)absoluteX, (int)absoluteY);
			}
			for(iterateX = relativeX; iterateX >= 0; iterateX--) {
				relativeX = iterateX;
				relativeY = Sqrt(Abs(radius * radius - relativeX * relativeX));

				absoluteX = center.X + kX * relativeX;
				absoluteY = center.Y + kY * relativeY;

				yield return new((int)absoluteX, (int)absoluteY);
			}

			yield break;
		}

		throw new ArgumentOutOfRangeException($"Valid range: 1 <= {nameof(part)} <= 4.");
	}

	private static readonly int[] order = { 1, 4, 3, 2 };
	private IEnumerator<Point> GetCirclePoints(Point center, float radius)
	{
		for(int i =0; i < order.Length; i++) {
			var points = GetOneFourthPoints(center, radius, order[i]);
			while(points.MoveNext()) {
				yield return points.Current;
			}
		}
	}
	private void DrawCircle(Point center, float radius, Color color, IEnumerator<bool> patternResolver)
	{
		var points = GetCirclePoints(center, radius);
		while(points.MoveNext()) {
			BypassPoint(points.Current, color, patternResolver);
		}
	}
	#endregion
}
