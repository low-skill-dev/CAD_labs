using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static System.Math;

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
		this.Lines = new();
		this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
	}

	public void RenderFrame(bool noClear = false)
	{
		if(!noClear) this.CurrentFrame = new Bitmap(FrameWidth, FrameHeight);
		this.Lines.ForEach(x => DrawLine(x.Start, x.End, x.Color, x.PatternResolver));
	}

	public void AddLine(Point start, Point end, Color? color = null, IEnumerator<bool>? patternResolver=null)
	{
		Lines.Add(new(start, end, color ?? Color.LightGreen, patternResolver ?? Line.GetDefaultResolver()));
	}
	#endregion





	#region private
	private float GetStep(int start, int end, int steps) => (end - start) / (float)steps;
	private void DrawLine(Point start, Point end, Color color, IEnumerator<bool> patternResolver)
	{
		var dX = Abs(end.X - start.X);
		var dY = Abs(end.Y - start.Y);


		if(dX > dY) { // we need to iterate  by X
			if(start.X > end.X) Swap(ref start, ref end);

			float stepY = GetStep(start.Y, end.Y, dX);
			for(int iX = start.X; iX <= end.X; iX++) {
				float currY = start.Y + (iX - start.X) * stepY;

				patternResolver.MoveNext();
				if(patternResolver.Current)
					this.CurrentFrame.SetPixel(iX, (int)Math.Max(0, currY), color);
			}
		} else { // else by Y
			if(start.Y > end.Y) Swap(ref start, ref end);

			float stepX = GetStep(start.X, end.X, dY);
			for(int iY = start.Y; iY <= end.Y; iY++) {
				float currX = start.X + (iY - start.Y) * stepX;

				patternResolver.MoveNext();
				if(patternResolver.Current)
					this.CurrentFrame.SetPixel((int)Math.Max(0, currX), iY, color);
			}
		}
	}
	#endregion
}
