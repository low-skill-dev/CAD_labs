using coursework.Models;
using GraphicLibrary;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = GraphicLibrary.MathModels.PointF;
using static System.MathF;
using GraphicLibrary.Models;
using GraphicLibrary.MathModels;
using System.Windows.Media;
using System.Security.Cryptography;
using Color = System.Drawing.Color;

namespace coursework
{
	public static class Capture
	{
		private static void Swap<T>(ref T v1, ref T v2)
		{
			var temp = v1;
			v1 = v2;
			v2 = temp;
		}

		internal static (float minX, float minY, float maxX, float maxY) GetBounds(RectangleF rect)
		{
			return (
					rect.X,
					rect.Y - rect.Height,
					rect.X + rect.Width,
					rect.Y
				);
		}

		private static bool IsInBounds(RectangleF rect, PointF point)
		{
			var (minX, minY, maxX, maxY) = GetBounds(rect);

			if(point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY) return false;

			return true;
		}

		private static bool isValidPoint(PointF target, SizeF Rect)
		{
			if(!(float.IsFinite(target.X) && float.IsFinite(target.Y))) return false;

			if(target.X < 0 || target.Y < 0) return false;
			if(target.X > Rect.Width - 1) return false;
			if(target.Y > Rect.Height - 1) return false;

			return true;
		}

		internal static bool IsCaptured(RectangleF captureRect, ArcF arc, bool partialCaptureMode = false)
		{
			var startA = arc.StartAngle;
			var endA = arc.EndAngle;
			var dir = arc.IsNegativeDirection ? -1 : 1;

			var curr = Common.FindPointOnCircle(arc.Center, arc.Radius, startA);
			if(IsInBounds(captureRect, curr)) {
				if(partialCaptureMode) return true; // точка попала и включен режим попадания части
			} else {
				if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
			}

			curr = Common.FindPointOnCircle(arc.Center, arc.Radius, endA);
			if(IsInBounds(captureRect, curr)) {
				if(partialCaptureMode) return true; // точка попала и включен режим попадания части
			} else {
				if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
			}

			if(dir < 0) {
				var closestAxisAngle = Round((startA - PI / 2) / PI) * PI;
				for(float a = closestAxisAngle; a > endA; a -= PI / 2) {
					curr = Common.FindPointOnCircle(arc.Center, arc.Radius, endA);
					if(IsInBounds(captureRect, curr)) {
						if(partialCaptureMode) return true; // точка попала и включен режим попадания части
					} else {
						if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
					}
				}
			} else {
				var closestAxisAngle = Round((startA + PI / 2) / PI) * PI;
				for(float a = closestAxisAngle; a < endA; a += PI / 2) {
					curr = Common.FindPointOnCircle(arc.Center, arc.Radius, endA);
					if(IsInBounds(captureRect, curr)) {
						if(partialCaptureMode) return true; // точка попала и включен режим попадания части
					} else {
						if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
					}
				}
			}

			/* partialCaptureMode == true:
			 * Будет возвращать true на каждом попадании, а в конце должен вернуть обратный случай - false, если
			 * ни одна из точек не попала.
			 * 
			 * partialCaptureMode == false:
			 * Обратная ситуация. Будет возвращать false сразу же при любом непопадании точки. В обратном случае
			 * должен вернуть true.
			 */
			return !partialCaptureMode;
		}

		internal static bool IsCaptured(RectangleF captureRect, CircleF circle, bool partialCaptureMode = false)
		{
			PointF curr;
			for(int mx = -1; mx <= 1; mx += 1) { // m = multiplier, -1 -> 0 -> 1
				for(int my = -1; my <= 1; my += 1) {
					curr = circle.Center + new PointF(circle.Radius * mx, circle.Radius * my);
					if(IsInBounds(captureRect, curr)) {
						if(partialCaptureMode) return true; // точка попала и включен режим попадания части
					} else {
						if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
					}
				}
			}

			return !partialCaptureMode;
		}

		internal static bool IsCaptured(RectangleF captureRect, FillerF filler, DirectBitmap bitmap, int baseColor, bool partialCaptureMode = false, bool useSimpleAlg = false)
		{
			if(useSimpleAlg) return IsInBounds(captureRect, filler.StartPoint);

			// extremely slow alg!
			PointF curr;
			Stack<Point> points = new();
			points.Push(new((int)Round(filler.StartPoint.X), (int)Round(filler.StartPoint.Y)));
			var screenSize = new SizeF(bitmap.Width, bitmap.Height);
			var clr = Color.FromArgb(baseColor);

			while(points.TryPop(out var point)) {
				curr = point;
				if(IsInBounds(captureRect, curr)) {
					if(partialCaptureMode) return true; // точка попала и включен режим попадания части
				} else {
					if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
				}

				if(filler.EightDirectionsMode) {
					for(int dx = -1; dx < 2; dx += 1) {
						for(int dy = -1; dy < 2; dy += 1) {
							if(dx == 0 && dy == 0) continue;
							Point target = new(point.X + dx, point.Y + dy);
							if(isValidPoint(target, screenSize)) {
								if(bitmap.GetPixel(target.X, target.Y).Equals(clr)) {
									points.Push(target);
								}
							}
						}
					}
				} else {
					for(int dx = -1; dx < 2; dx += 2) {
						Point target = new(point.X + dx, point.Y);
						if(isValidPoint(target, screenSize)) {
							if(bitmap.GetPixel(target.X, target.Y).Equals(clr)) {
								points.Push(target);
							}
						}
					}
					for(int dy = -1; dy < 2; dy += 2) {
						Point target = new(point.X, point.Y + dy);
						if(isValidPoint(target, screenSize)) {
							if(bitmap.GetPixel(target.X, target.Y).Equals(clr)) {
								points.Push(target);
							}
						}
					}
				}
			}

			return !partialCaptureMode;
		}

		internal static bool IsCaptured(RectangleF captureRect, LineF line, bool partialCaptureMode = false)
		{
			var startCapt = IsInBounds(captureRect, line.Start);
			var endCapt = IsInBounds(captureRect, line.End);

			if(startCapt && endCapt) return true;
			if((startCapt || endCapt) && partialCaptureMode) return true;


			var (k, b) = Common.FindLinearEquation(line);

			var minX = captureRect.Left;
			var maxX = captureRect.Right;

			var minY = captureRect.Top;
			var maxY = captureRect.Bottom;

			var atMinX = k * minX + b;
			var atMaxX = k * maxX + b;

			if((line.Left.X < minX && atMinX > minY && atMinX < maxY) 
				|| (line.Right.X > maxX && atMaxX > minY && atMaxX < maxY)) {
				if(partialCaptureMode) return true; // точка попала и включен режим попадания части
			} else {
				if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
			}

			return !partialCaptureMode;
		}

		internal static bool IsCaptured(RectangleF captureRect, PolyLineF line, bool partialCaptureMode = false)
		{
			var objects = line.ToArcsAndLines();

			foreach(LineF ln in  objects.Where(obj=> obj is LineF)) {
				if(IsCaptured(captureRect, ln, partialCaptureMode)) {
					if(partialCaptureMode) return true; // точка попала и включен режим попадания части
				} else {
					if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
				}
			}
			foreach(ArcF arc in objects.Where(obj => obj is ArcF)) {
				if(IsCaptured(captureRect, arc, partialCaptureMode)) {
					if(partialCaptureMode) return true; // точка попала и включен режим попадания части
				} else {
					if(!partialCaptureMode) return false; // точка не попала и включен режим частичного попадания
				}
			}

			return !partialCaptureMode;
		}
	}
}
