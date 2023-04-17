using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursework.Models
{
	class FillerF : AEditorElement
	{
		public PointF StartPoint { get; set; }
		public bool EightDirectionsMode { get; set; } = false;
		public override int ColorArgb { get; set; }

		public FillerF(PointF center, int colorArgb = LightGreenArgb)
		{
			StartPoint = center;
			ColorArgb = colorArgb;
		}


		public override FillerF Clone()
		{
			return new(StartPoint.Clone(), ColorArgb);
		}
		public override void Move(PointF diff)
		{
			StartPoint = StartPoint + diff;
		}
		public override void Rotate(float angleR, PointF relativeTo)
		{
			StartPoint = Common.RotatePoint(this.StartPoint, relativeTo, angleR);
		}
		public override void Scale(float scale, PointF relativeTo)
		{
			StartPoint = Common.ScalePoint(StartPoint, relativeTo, scale);
		}

		public static explicit operator GraphicLibrary.Models.AreaFiller(FillerF fillerF)
		{
			return new GraphicLibrary.Models.AreaFiller(
				fillerF.StartPoint, System.Drawing.Color.FromArgb(fillerF.ColorArgb));
		}
	}
}
