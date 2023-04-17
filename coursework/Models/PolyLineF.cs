﻿using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using PointF = GraphicLibrary.MathModels.PointF;

namespace coursework.Models
{
	class PolyPointF : PointF
	{
		public PointF Location {
			get => this as PointF;
			set {
				base.X = value.X;
				base.Y = value.Y;
			}
		}
		public bool IsCirclePoint { get; set; }

		public PolyPointF(PointF location, bool isCirclePoint)
			: base(location.X, location.Y)
		{
			this.IsCirclePoint = isCirclePoint;
		}
		public PolyPointF(float x, float y, bool isCirclePoint)
			: base(x, y)
		{
			this.IsCirclePoint = isCirclePoint;
		}

		public override PolyPointF Clone()
		{
			return new(new(Location.X, Location.Y), IsCirclePoint);
		}


#pragma warning disable CS8618
		private PolyPointF() : base(0, 0) { }
#pragma warning restore
	}

	//class LineF : AEditorElement, IPatterned
	//{
	//	public PointF Start;
	//	public PointF End;

	//	public PointF Left => Start.X < End.X ? Start : End;
	//	public PointF Right => Start.X > End.X ? Start : End;

	//	public string Pattern { get; set; }
	//	public override int ColorArgb { get; set; }

	//	public LineF(PointF start, PointF end, string pattern = DefaultPattern)
	//	{
	//		Start = start;
	//		End = end;
	//		this.Pattern = pattern;
	//	}
	//	public LineF(System.Drawing.PointF start, System.Drawing.PointF end, string pattern = DefaultPattern)
	//	{
	//		Start = start;
	//		End = end;
	//		this.Pattern = pattern;
	//	}
	//	public LineF(System.Windows.Point start, System.Windows.Point end, string pattern = DefaultPattern)
	//	{
	//		Start = start;
	//		End = end; 
	//		this.Pattern = pattern;
	//	}

	//	public bool Equals(LineF other)
	//	{
	//		return this.Start.Equals(other.Start)
	//			&& this.End.Equals(other.End);
	//	}

	//	public bool Equals(PointF other)
	//	{
	//		return this.Start.Equals(other)
	//			&& this.End.Equals(other);
	//	}

	//	public override void Move(PointF diff)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public override void Rotate(float angleR, PointF relativeTo)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public override void Scale(float scale, PointF relativeTo)
	//	{
	//		throw new NotImplementedException();
	//	}

	//	public override AEditorElement Clone()
	//	{
	//		throw new NotImplementedException();
	//	}
	//}

	class PolyLineF : AEditorElement, IPatterned
	{
		public List<PolyPointF> Points { get; init; } = new();
		public override int ColorArgb { get; set; }
		public string Pattern { get; set; }
		public IEnumerator<bool> PatternResolver => Common.CreatePatternResolver(Pattern);

		public PolyLineF(int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
		{
			ColorArgb = colorArgb;
			Pattern = pattern;
		}

		/// <returns>
		/// Collection contains <see cref="GraphicLibrary.MathModels.LineF"/> and
		/// <see cref="coursework.Models.ArcF"/> objects.
		/// </returns>
		public List<object> ToArcsAndLines()
		{
			List<object> result = new(this.Points.Count);

			for(int i = 1; i < Points.Count; i++) {
				var prev = Points.At(i - 1);
				var curr = Points.At(i);
				var next = Points.At(i + 1);

				var pp = prev.Location;
				var pc = curr.Location;
				var pn = next.Location;

				if(curr.IsCirclePoint) {
					if(i == Points.Count - 1) {
#if DEBUG
						throw new ArgumentException("Circle creation point cannot be last at the polyline sequence.");
#else
						return new LineF(pp, pc);
#endif
					}

					var center = Common.FindCenter(pp, pc, pn);
					var start = Common.FindAngleOfPointOnCircle(pp, center);
					var mid = Common.FindAngleOfPointOnCircle(pc, center);
					var end = Common.FindAngleOfPointOnCircle(pn, center);
					var radius = Common.GetCirleRadius(center, pp);

					var inPos = mid - start;
					var inNeg = start - mid;
					bool isNeg = inPos > inNeg ? true : false;

					result.Add(new ArcF(center, radius, start, end, isNeg));
				} else {
					result.Add(new LineF(pp, pc));
				}
			}

			return result;
		}

		public override void Move(PointF diff)
		{
			for(int i = 0; i < Points.Count; i++)
				Points[i].Location += diff;
		}

		public override void Rotate(float angleR, PointF relativeTo)
		{
			for(int i = 0; i < Points.Count; i++)
				Points[i].Location = Common.RotatePoint(Points[i].Location, relativeTo, angleR);
		}

		public override void Scale(float scale, PointF relativeTo)
		{
			for(int i = 0; i < Points.Count; i++)
				Points[i].Location = Common.ScalePoint(Points[i].Location, relativeTo, scale);
		}

		public override PolyLineF Clone()
		{
			return new(ColorArgb, Pattern) {
				Points = this.Points.Select(p => p.Clone()).ToList()
			};
		}
	}
}