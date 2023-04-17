using coursework.Models;
using coursework.ModelsInterfaces;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using PointF = GraphicLibrary.MathModels.PointF;
using static System.MathF;
using GraphicLibrary;
using System.Windows.Input;

namespace coursework.Services
{
	/* Реализует паттерн Декоратор по отношению к BitmapEditor.
	*/
	class StatefulEditor : BitmapEditor
	{
		public enum States
		{
			None, // not used internally, equals to empty stack
			PartialSelection_NotStarted,
			PartialSelection_FirstPoindAdded,
			FullSelection_NotStarted,
			FullSelection_FirstPoindAdded,
			PolylineDrawing_NotStarted,
			PolylineDrawing_LastIsLine,
			PolylineDrawing_LastIsCircle,
			CircleDrawing_NotStarted,
			CircleDrawing_CenterSelected,
			ArcDrawing_NotStarted,
			ArcDrawing_FistPointAdded,
			ArcDrawing_SecondPointAdded,
			Filling,
			RelativePointSelection
		}

		public Stack<States> State { get; init; } = new();
		public States CurrentState => State.Count > 0 ? State.Peek() : States.None;
		public Stack<PointF> CachedPoints { get; init; } = new();

		public PointF RelativenessPoint { get; set; }
		public int BackgroundColorArgb { get; set; }
		public int FillingColorArgb { get; set; }
		public int DrawingColorArgb { get; set; }
		public string DrawingPattern { get; set; }
		public List<AEditorElement> CurrentlySelectedObjects { get; init; } = new();

		public bool FillerSelectionFastMode { get; set; } = false;

		public StatefulEditor(int width, int height)
			: base(width, height)
		{
			RelativenessPoint = new(width / 2, height / 2);

			BackgroundColorArgb = 0;
			DrawingColorArgb = AEditorElement.LightGreenArgb;
			FillingColorArgb = AEditorElement.LightCoralArgb;

		}

		public void DeleteObjects(IEnumerable<AEditorElement> objects)
		{
			foreach(var obj in objects) {
				if(obj is not null && obj is ArcF) base.Arcs.Remove((ArcF)obj);
				if(obj is not null && obj is CircleF) base.Circles.Remove((CircleF)obj);
				if(obj is not null && obj is FillerF) base.Fillers.Remove((FillerF)obj);
				if(obj is not null && obj is PolyLineF) base.PolyLines.Remove((PolyLineF)obj);
			}
		}

		public override void Reset()
		{
			base.Reset();

			this.State.Clear();
		}


		private RectangleF RectangleFromPoints(PointF p1, PointF p2)
		{
			var x1 = p1.X;
			var y1 = p1.Y;
			var x2 = p2.X;
			var y2 = p2.Y;

			var leftX = x1 < x2 ? x1 : x2;
			var upperY = y1 > y2 ? y1 : y2;

			var width = x1 > x2 ? x1 - x2 : x2 - x1;
			var height = y1 > y2 ? y1 - y2 : y2 - y1;

			return new(leftX, upperY, width, height);
		}
		public void MouseDown(PointF pos, MouseButtonEventArgs args)
		{
			switch(CurrentState) {
				case States.None:
					return;
				case States.RelativePointSelection:
					RelativenessPoint = pos;
					State.Pop(); // POP
					break;
				case States.Filling:
					Fillers.Add(new(pos, FillingColorArgb));
					State.Pop();
					break;
				case States.FullSelection_NotStarted:
				case States.PartialSelection_NotStarted:
					CachedPoints.Push(pos);
					State.Pop(); // POP
					State.Push(
						CurrentState == States.FullSelection_NotStarted ?
						States.FullSelection_FirstPoindAdded : States.PartialSelection_FirstPoindAdded);
					break;
				case States.FullSelection_FirstPoindAdded:
				case States.PartialSelection_FirstPoindAdded:
					this.CurrentlySelectedObjects.Clear();
					this.CurrentlySelectedObjects.AddRange(CaptureObjects(
						RectangleFromPoints(CachedPoints.Pop(), pos),
						CurrentState == States.PartialSelection_FirstPoindAdded ? true : false,
						FillerSelectionFastMode,
						this.BackgroundColorArgb));
					State.Pop();
					break;
				case States.CircleDrawing_NotStarted:
					CachedPoints.Push(pos);
					State.Pop();
					State.Push(States.CircleDrawing_CenterSelected);
					break;
				case States.CircleDrawing_CenterSelected:
					this.Circles.Add(new(CachedPoints.Pop(), pos, this.DrawingColorArgb, DrawingPattern ?? "+"));
					State.Pop();
					break;
				case States.ArcDrawing_NotStarted:
				case States.ArcDrawing_FistPointAdded:
					CachedPoints.Push(pos);
					var nextState = CurrentState == States.ArcDrawing_NotStarted ?
						States.ArcDrawing_FistPointAdded : States.ArcDrawing_SecondPointAdded;
					State.Pop();
					State.Push(nextState);
					break;
				case States.ArcDrawing_SecondPointAdded:
					var p2 = CachedPoints.Pop();
					var p1 = CachedPoints.Pop();
					this.Arcs.Add(new(p1, p2, pos, this.DrawingColorArgb, DrawingPattern ?? "+"));
					State.Pop();
					break;
				case States.PolylineDrawing_NotStarted:
					this.CachedPoints.Push(new PolyPointF(pos, false));
					State.Pop();
					State.Push(States.PolylineDrawing_LastIsLine);
					break;
				case States.PolylineDrawing_LastIsLine:
				case States.PolylineDrawing_LastIsCircle:
					if(args.MiddleButton == MouseButtonState.Pressed) { // stop line
						CachedPoints.Push(new PolyPointF(pos, false));
						var pl = new PolyLineF(DrawingColorArgb, DrawingPattern) {
							Points = CachedPoints.TakeWhile(x => x is PolyPointF).Select(x => (PolyPointF)x).ToList() };
						for(var i = 0; i < pl.Points.Count; i++) CachedPoints.Pop();
						State.Pop();
						State.Push(States.None);
					} else
					if(args.RightButton == MouseButtonState.Pressed && !((PolyPointF)CachedPoints.Peek()).IsCirclePoint) {
						CachedPoints.Push(new PolyPointF(pos, true)); // add circle if last is line
						State.Pop();
						State.Push(States.PolylineDrawing_LastIsCircle);
					} else {
						CachedPoints.Push(new PolyPointF(pos, false)); // add line
						State.Pop();
						State.Push(States.PolylineDrawing_LastIsLine);
					}
					break;

				default:
					throw new NotImplementedException();
			}
		}
	}
}
