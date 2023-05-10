using coursework.DataModels;
using coursework.Models;
using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Media.Imaging;
using ArcF = coursework.Models.ArcF;
using Color = System.Drawing.Color;

namespace coursework.Services;

/* Реализует паттерн Прокси по отношению к BitmapDrawer.
 */
public class BitmapEditor
{
	protected readonly BitmapDrawer drawer; // it must be readonly!
	public BitmapEditor(int width, int height)
	{
		drawer = new BitmapDrawer(width, height);
	}
	public BitmapEditor(Saved saved)
	{
		drawer = new(saved.Width, saved.Height);
		BackgroundColorArgb = saved.BackgroundColorArgb;
		PolyLines = saved.PolyLines?.ToList() ?? new();
		Circles = saved.Circles?.ToList() ?? new();
		Arcs = saved.Arcs?.ToList() ?? new();
		Fillers = saved.Fillers?.ToList() ?? new();
	}

	public bool AddAxes { get; set; } = true;

	public Bitmap Bitmap => drawer.CurrentFrame.Bitmap;

	public int BackgroundColorArgb { get; set; }
	public List<PolyLineF> PolyLines { get; init; } = new();
	public List<CircleF> Circles { get; init; } = new();
	public List<ArcF> Arcs { get; init; } = new();
	public List<FillerF> Fillers { get; init; } = new();

	public virtual void Reset()
	{
		drawer.Reset();

		PolyLines.Clear();
		Circles.Clear();
		Arcs.Clear();
		Fillers.Clear();
	}
	public virtual BitmapImage RenderCurrentState()
	{
		drawer.Reset();
		drawer.AddAxes = AddAxes;

		PolyLines.ForEach(AddPolylineObjectToDrawer);
		Circles.ForEach(x => drawer.Circles.Add((GraphicLibrary.Models.Circle)x));
		Arcs.ForEach(x => drawer.Arcs.Add((GraphicLibrary.Models.Arc)x));
		Fillers.ForEach(x => drawer.AreaFillers.Add((GraphicLibrary.Models.AreaFiller)x));

		drawer.RenderFrame();
		return drawer.CurrentFrameImage;
	}


	public List<AEditorElement> CaptureObjects(RectangleF captureRect, bool partialCaptureMode = false, bool fillerFastMode = false, int baseColor = 0)
	{
		List<AEditorElement> captured = new();

		PolyLines.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) { captured.Add(x); } });
		Circles.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) { captured.Add(x); } });
		Arcs.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) { captured.Add(x); } });
		Fillers.ForEach(x => { if(Capture.IsCaptured(captureRect, x, drawer.CurrentFrame, baseColor, partialCaptureMode, fillerFastMode)) { captured.Add(x); } });

		return captured;
	}

	public Saved ToSaved()
	{
		return new Saved {
			Width = drawer.CurrentFrame.Width,
			Height = drawer.CurrentFrame.Height,
			BackgroundColorArgb = BackgroundColorArgb,
			PolyLines = PolyLines.ToArray(),
			Circles = Circles.ToArray(),
			Arcs = Arcs.ToArray(),
			Fillers = Fillers.ToArray()
		};
	}

	#region private
	/* Пока имеем обычную линию - так и рисуем.
         * Когда встречаем точку окружности - после неё обязана идти еще одна точка.
         */
	private void AddPolylineObjectToDrawer(PolyLineF polyLine)
	{
		var pts = polyLine.Points;
		var clr = Color.FromArgb(polyLine.ColorArgb);
		var ptrn = polyLine.PatternResolver;

		for(var i = 1; i < pts.Count; i++) {
			var pp = pts.At(i - 1);
			var pc = pts.At(i);
			var pn = pts.At(i + 1);


			if(pc.IsCirclePoint) {
				if(i == PolyLines.Count - 1) {
					break;
				}
				drawer.Arcs.Add((Arc)new ArcF(pp, pc, pn, polyLine.ColorArgb, polyLine.Pattern));
				i++;
			} else {
				drawer.Lines.Add(new(pp, pc, clr, ptrn));
			}
		}
	}

	//private IEnumerator<PointF> EnumeratePointsRefs()
	//{
	//	PolyLines.Clear();
	//	Circles.Clear();
	//	Arcs.Clear();
	//	Fillers.Clear();
	//	bool seenStart = false;

	//	foreach(var obj in PolyLines) {
	//		var split = obj.ToArcsAndLines();
	//		foreach(LineF line in split.Where(l=> l is LineF)) {
	//			if(!seenStart) {
	//				seenStart = true;
	//				yield return line.Start;
	//			}
	//			yield return line.End;
	//		}
	//		foreach(ArcF arc in split.Where(l => l is ArcF)) {
	//			yield return arc.Center;
	//		}
	//	}
	//}

	#endregion
}
