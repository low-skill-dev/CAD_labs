﻿using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursework.Models
{
	class CircleF : AEditorElement, IPatterned
	{
		public PointF Center { get; set; }
		public float Radius { get; set; }
		public override int ColorArgb { get; set; }
		public string Pattern { get; set; }

		public CircleF(PointF center, float radius, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
		{
			Center = center;
			Radius = radius;
			ColorArgb = colorArgb;
			Pattern = pattern;
		}
		public CircleF(PointF center, PointF onCircle, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
			: this(center, Common.GetCirleRadius(center, onCircle), colorArgb, pattern) { }

		public override void Move(PointF diff)
		{
			Center = Center+diff;
		}
		public override void Rotate(float angleR, PointF relativeTo)
		{
			Center = Common.RotatePoint(this.Center, relativeTo, angleR);
		}
		public override void Scale(float scale, PointF relativeTo)
		{
			Center = Common.ScalePoint(Center, relativeTo, scale);
			Radius *= scale;
		}
		public override CircleF Clone()
		{
			return new(Center.Clone(), Radius, ColorArgb, Pattern);
		}


		public static explicit operator GraphicLibrary.Models.Circle(CircleF circleF)
		{
			return new GraphicLibrary.Models.Circle(
				circleF.Center,
				circleF.Radius,
				System.Drawing.Color.FromArgb(circleF.ColorArgb),
				(circleF as IPatterned).PatternResolver);
		}
	}
}
