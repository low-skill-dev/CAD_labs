using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;

namespace coursework.Models;

public class FillerF : AEditorElement
{
	public PointF StartPoint { get; set; }
	public bool EightDirectionsMode { get; set; } = false;
	public override int ColorArgb { get; set; }

	private string modeString => EightDirectionsMode ? "8 пикселей" : "4 пикселя";
	public override string ToRussianString => $"Заливка.\nТочка начала: {StartPoint}.\nРежим: {modeString}.\n";

	public FillerF()
		: this(new(0, 0))
	{

	}
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
		StartPoint += diff;
	}
	public override void Rotate(float angleR, PointF relativeTo)
	{
		StartPoint = Common.RotatePoint(StartPoint, relativeTo, angleR);
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
