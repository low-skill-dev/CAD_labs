using coursework.DataModels;
using coursework.Models;
using coursework.ModelsInterfaces;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using static coursework.Services.StatefulEditor.States;
using PointF = GraphicLibrary.MathModels.PointF;

namespace coursework.Services;

/* Реализует паттерн Декоратор по отношению к BitmapEditor.
*/
public class StatefulEditor : BitmapEditor
{
	public enum States
	{
		None,
		ClickCapture = None, // not used internally, equals to empty stack
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
		RelativePointSelection,
		CapturedObjectsEdition
	}

	public Stack<States> State { get; init; } = new();
	public States CurrentState => State.Count > 0 ? State.Peek() : ClickCapture;
	public string CurrentStateString {
		get {
			switch(CurrentState) {
				case ClickCapture:
					return nameof(ClickCapture);
				case PartialSelection_NotStarted:
					return nameof(PartialSelection_NotStarted);
				case PartialSelection_FirstPoindAdded:
					return nameof(PartialSelection_FirstPoindAdded);
				case FullSelection_NotStarted:
					return nameof(FullSelection_NotStarted);
				case FullSelection_FirstPoindAdded:
					return nameof(FullSelection_FirstPoindAdded);
				case PolylineDrawing_NotStarted:
					return nameof(PolylineDrawing_NotStarted);
				case PolylineDrawing_LastIsLine:
					return nameof(PolylineDrawing_LastIsLine);
				case PolylineDrawing_LastIsCircle:
					return nameof(PolylineDrawing_LastIsCircle);
				case CircleDrawing_NotStarted:
					return nameof(CircleDrawing_NotStarted);
				case CircleDrawing_CenterSelected:
					return nameof(CircleDrawing_CenterSelected);
				case ArcDrawing_NotStarted:
					return nameof(ArcDrawing_NotStarted);
				case ArcDrawing_FistPointAdded:
					return nameof(ArcDrawing_FistPointAdded);
				case ArcDrawing_SecondPointAdded:
					return nameof(ArcDrawing_SecondPointAdded);
				case Filling:
					return nameof(Filling);
				case RelativePointSelection:
					return nameof(RelativePointSelection);
				case CapturedObjectsEdition:
					return nameof(CapturedObjectsEdition);
#if DEBUG
				default:
					throw new NotImplementedException();
#else
				default:
					return "N/A";
#endif
			}
		}
	}
	public Stack<PointF> CachedPoints { get; init; } = new();

	public PointF RelativenessPoint { get; set; }
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

	public StatefulEditor(Saved saved)
		: base(saved)
	{
		RelativenessPoint = new(saved.Width / 2, saved.Height / 2);

		BackgroundColorArgb = 0;
		DrawingColorArgb = AEditorElement.LightGreenArgb;
		FillingColorArgb = AEditorElement.LightCoralArgb;
	}

	public void DeleteObjects(IEnumerable<AEditorElement> objects)
	{
		foreach(var obj in objects) {
			if(obj is not null && obj is ArcF) {
				_ = Arcs.Remove((ArcF)obj);
			}

			if(obj is not null && obj is CircleF) {
				_ = Circles.Remove((CircleF)obj);
			}

			if(obj is not null && obj is FillerF) {
				_ = Fillers.Remove((FillerF)obj);
			}

			if(obj is not null && obj is PolyLineF) {
				_ = PolyLines.Remove((PolyLineF)obj);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();

		State.Clear();
	}


	private RectangleF RectangleFromPoints(PointF p1, PointF p2)
	{
		var x1 = p1.X;
		var y1 = p1.Y;
		var x2 = p2.X;
		var y2 = p2.Y;

		var leftX = x1 < x2 ? x1 : x2;
		var upperY = y1 < y2 ? y1 : y2;

		var width = x1 > x2 ? x1 - x2 : x2 - x1;
		var height = y1 > y2 ? y1 - y2 : y2 - y1;

		return new(leftX, upperY, width, height);
	}
	public void MouseDown(PointF pos, MouseButtonEventArgs args)
	{
		States prev;
		switch(CurrentState) {
			case ClickCapture:
				var d = new PointF(3, 3);
				var rect = RectangleFromPoints(pos - d, pos + d);
				CurrentlySelectedObjects.Clear();
				CurrentlySelectedObjects.AddRange(CaptureObjects(
					rect, true, FillerSelectionFastMode, BackgroundColorArgb));
				CachedPoints.Push(pos);
				prev = State.Pop();
				State.Push(CapturedObjectsEdition);
				break;
			case RelativePointSelection:
				RelativenessPoint = pos;
				_ = State.Pop(); // POP
				break;
			case Filling:
				Fillers.Add(new(pos, DrawingColorArgb));
				//State.Pop();
				break;
			case FullSelection_NotStarted:
			case PartialSelection_NotStarted:
				CachedPoints.Push(pos);
				prev = State.Pop();
				State.Push(
					prev == FullSelection_NotStarted ?
					FullSelection_FirstPoindAdded : PartialSelection_FirstPoindAdded);
				break;
			case FullSelection_FirstPoindAdded:
			case PartialSelection_FirstPoindAdded:
				CurrentlySelectedObjects.Clear();
				CurrentlySelectedObjects.AddRange(CaptureObjects(
					RectangleFromPoints(CachedPoints.Peek(), pos),
					CurrentState == PartialSelection_FirstPoindAdded,
					FillerSelectionFastMode,
					BackgroundColorArgb));
				CachedPoints.Push(pos);
				prev = State.Pop();
				State.Push(CapturedObjectsEdition);
				break;
			case CircleDrawing_NotStarted:
				CachedPoints.Push(pos);
				_ = State.Pop();
				State.Push(CircleDrawing_CenterSelected);
				break;
			case CircleDrawing_CenterSelected:
				Circles.Add(new(CachedPoints.Pop(), pos, DrawingColorArgb, DrawingPattern ?? "+"));
				_ = State.Pop();
				State.Push(CircleDrawing_NotStarted);
				break;
			case ArcDrawing_NotStarted:
			case ArcDrawing_FistPointAdded:
				CachedPoints.Push(pos);
				var nextState = CurrentState == ArcDrawing_NotStarted ?
					ArcDrawing_FistPointAdded : ArcDrawing_SecondPointAdded;
				_ = State.Pop();
				State.Push(nextState);
				break;
			case ArcDrawing_SecondPointAdded:
				PointF
					p3 = pos,
					p2 = CachedPoints.Pop(),
					p1 = CachedPoints.Pop();
				Arcs.Add(new(p1, p2, p3, DrawingColorArgb, DrawingPattern ?? "+"));
				_ = State.Pop();
				State.Push(ArcDrawing_NotStarted);
				break;
			case PolylineDrawing_NotStarted:
				CachedPoints.Push(new PolyPointF(pos, false));
				_ = State.Pop();
				State.Push(PolylineDrawing_LastIsLine);
				break;
			case PolylineDrawing_LastIsLine:
			case PolylineDrawing_LastIsCircle:
				if(args.MiddleButton == MouseButtonState.Pressed) { // stop line
					CachedPoints.Push(new PolyPointF(pos, false));
					var pl = new PolyLineF(DrawingColorArgb, DrawingPattern) {
						Points = CachedPoints.TakeWhile(x => x is PolyPointF).Select(x => (PolyPointF)x).ToList()
					};
					for(var i = 0; i < pl.Points.Count; i++) {
						_ = CachedPoints.Pop();
					}

					base.PolyLines.Add(pl);
					_ = State.Pop();
					State.Push(PolylineDrawing_NotStarted);
				} else
				if(args.RightButton == MouseButtonState.Pressed && !((PolyPointF)CachedPoints.Peek()).IsCirclePoint) {
					CachedPoints.Push(new PolyPointF(pos, true)); // add circle if last is line
					_ = State.Pop();
					State.Push(PolylineDrawing_LastIsCircle);
				} else {
					CachedPoints.Push(new PolyPointF(pos, false)); // add line
					_ = State.Pop();
					State.Push(PolylineDrawing_LastIsLine);
				}
				break;
			case CapturedObjectsEdition:
				break;

			default:
				throw new NotImplementedException();
		}
	}

	public bool TryExit()
	{
		State.Clear();
		CachedPoints.Clear();
		return true;
	}

	public PointF MouseAt { get; set; }


	private readonly AEditorElement CachedFigure;
	public override BitmapImage RenderCurrentState()
	{
		MouseAt ??= new(-1f, -1f);

		base.drawer.BackgroundColor = System.Drawing.Color.FromArgb(BackgroundColorArgb);

		if(CurrentState == CapturedObjectsEdition) {
			var points = CachedPoints.TakeLast(2).ToArray();

			var rect = RectangleFromPoints(points[0], points[1]);

			var p1 = rect.Location;
			var p2 = rect.Location + rect.Size;

			var rectFig = new GraphicLibrary.Models.Rectangle(p1, p2, Color.Yellow, ALinearElement.GetDefaultPatternResolver());

			base.drawer.ConstantObjects.Add(rectFig);
			var result = base.RenderCurrentState();
			_ = base.drawer.ConstantObjects.Remove(rectFig);

			return result;
		}

		var circles = new List<CircleF>(CachedPoints.Count);
		foreach(Point p in CachedPoints) {
			circles.Add(new(p, 3));
		}
		base.Circles.AddRange(circles);

		Action? RemoveCachedFigure = null;
		switch(CurrentState) {
			case ArcDrawing_SecondPointAdded:
				var points = CachedPoints.Take(2).ToArray();
				PointF
					p3 = MouseAt,
					p2 = points[0],
					p1 = points[1];
				if(p3.Equals(p2) || p3.Equals(p1)) {
					break; // cant build arc on equal points
				}

				var cachedArc = new ArcF(p1, p2, p3, DrawingColorArgb, DrawingPattern ?? "+");
				base.Arcs.Add(cachedArc);
				RemoveCachedFigure = () => base.Arcs.Remove(cachedArc);
				break;
			case CircleDrawing_CenterSelected:
				PointF
					p = MouseAt,
					c = CachedPoints.Peek();
				var cachedCircle = new CircleF(c, p, DrawingColorArgb, DrawingPattern ?? "+");
				base.Circles.Add(cachedCircle);
				RemoveCachedFigure = () => base.Circles.Remove(cachedCircle);
				break;
			case PolylineDrawing_LastIsCircle:
			case PolylineDrawing_LastIsLine:
				var pts = CachedPoints.TakeWhile(x => x is PolyPointF)
					.Reverse().ToList();

				if(!(CurrentState == PolylineDrawing_LastIsCircle && pts[^1].Equals(MouseAt))) {
					pts.Add(new PolyPointF(MouseAt, false));
				} else {
					break;
				}

				PolyLineF cachedPoly;
				cachedPoly = new PolyLineF(DrawingColorArgb, DrawingPattern) {
					Points = pts.Select(x => (PolyPointF)x).ToList()
				};
				RemoveCachedFigure = () => base.PolyLines.Remove(cachedPoly);
				base.PolyLines.Add(cachedPoly);
				break;
		}


		var ret = base.RenderCurrentState();
		RemoveCachedFigure?.Invoke();
		_ = base.Circles.RemoveAll(circles.Contains);
		return ret;
	}
}
