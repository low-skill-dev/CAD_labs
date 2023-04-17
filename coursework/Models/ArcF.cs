using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.MathF;

namespace coursework.Models
{
	class ArcF : CircleF
	{
		public float StartAngle { get; set; }
		public float EndAngle { get; set; }
		public bool IsNegativeDirection { get; set; }
		public ArcF(PointF center, float radius, float startAngle, float endAngle, bool isNegativeDirection, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
			: base(center, radius, colorArgb, pattern)
		{
			StartAngle = startAngle;
			EndAngle = endAngle;
			IsNegativeDirection = isNegativeDirection;
		}
		public ArcF(PointF p1, PointF p2, PointF p3, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
			: base(Common.FindCenter(p1, p2, p3), p1, colorArgb, pattern)
		{
			var start = Common.FindAngleOfPointOnCircle(p1, Center);
			var mid = Common.FindAngleOfPointOnCircle(p2, Center);
			var end = Common.FindAngleOfPointOnCircle(p3, Center);

			var inPos = mid - start;
			var inNeg = start - mid;

			// TODO: this method is working not correcly
			if(inPos < 0) inPos =  + Abs(inPos);
			if(inNeg < 0) inNeg = mid + Abs(inNeg);

			bool isNeg = inPos > inNeg ? true : false;

			StartAngle = start;
			EndAngle = end;
			IsNegativeDirection = isNeg;
		}

		public static explicit operator GraphicLibrary.Models.Arc(ArcF arcF)
		{
			return new GraphicLibrary.Models.Arc(
				new(
					arcF.Center,
					arcF.Radius,
					System.Drawing.Color.FromArgb(arcF.ColorArgb),
					(arcF as IPatterned).PatternResolver
				), arcF.StartAngle, arcF.EndAngle, arcF.IsNegativeDirection);
		}
	}
}
