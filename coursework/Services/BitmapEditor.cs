using coursework.Models;
using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArcF = coursework.Models.ArcF;
using Color = System.Drawing.Color;

namespace coursework.Services
{
	/* Реализует паттерн Прокси по отношению к BitmapDrawer.
	 */
	class BitmapEditor
	{
		private readonly BitmapDrawer drawer; // it must be readonly!
		public BitmapEditor(int width, int height)
		{
			drawer = new BitmapDrawer(width, height);
		}

		public bool AddAxes { get; set; } = true;

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
		public BitmapImage RenderCurrentState()
		{
			drawer.Reset();
			drawer.AddAxes = AddAxes;

			PolyLines.ForEach(x => AddPolylineObjectToDrawer(x));
			Circles.ForEach(x => drawer.Circles.Add((GraphicLibrary.Models.Circle)x));
			Arcs.ForEach(x => drawer.Arcs.Add((GraphicLibrary.Models.Arc)x));
			Fillers.ForEach(x => drawer.AreaFillers.Add((GraphicLibrary.Models.AreaFiller)x));

			drawer.RenderFrame();
			return drawer.CurrentFrameImage;
		}


		public List<AEditorElement> CaptureObjects(RectangleF captureRect, bool partialCaptureMode = false, bool fillerFastMode = false, int baseColor = 0)
		{
			List<AEditorElement> captured = new();

			PolyLines.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) captured.Add(x); });
			Circles.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) captured.Add(x); });
			Arcs.ForEach(x => { if(Capture.IsCaptured(captureRect, x, partialCaptureMode)) captured.Add(x); });
			Fillers.ForEach(x => { if(Capture.IsCaptured(captureRect, x, drawer.CurrentFrame, baseColor, partialCaptureMode, fillerFastMode)) captured.Add(x); });

			return captured;
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

			for(int i = 1; i < PolyLines.Count; i++) {
				var prev = pts.At(i - 1);
				var curr = pts.At(i);
				var next = pts.At(i + 1);

				var pp = prev.Location;
				var pc = curr.Location;
				var pn = next.Location;

				if(curr.IsCirclePoint) {
					if(i == PolyLines.Count - 1) break;

					var center = Common.FindCenter(pp, pc, pn);
					var start = Common.FindAngleOfPointOnCircle(pp, center);
					var mid = Common.FindAngleOfPointOnCircle(pc, center);
					var end = Common.FindAngleOfPointOnCircle(pn, center);

					var inPos = mid - start;
					var inNeg = start - mid;
					bool isNeg = inPos > inNeg ? true : false;

					drawer.Arcs.Add(new(new(pp, pc, pn, clr, ptrn), start, end, isNeg));
				} else {
					drawer.Lines.Add(new(pp, pc, clr, ptrn));
				}
			}
		}
		#endregion
	}
}
