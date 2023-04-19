using coursework.ModelsInterfaces;
using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using static System.MathF;

namespace coursework.Models;

public class ArcF : CircleF
{
	public float StartAngle { get; set; }
	public float EndAngle { get; set; }
	public bool IsNegativeDirection { get; set; }

	private string dirString => IsNegativeDirection ? "Отриц" : "Полож";
	public override string ToRussianString => $"Дуга. Центр: ({(int)Center.X},{(int)Center.Y}).\n" +
		$"Начало дуги: {StartAngle:0.00} рад.\n" +
		$"Конец дуги: {EndAngle:0.00} рад.\n" +
		$"Направление: {dirString}.\n";

	public ArcF(PointF center, float radius, float startAngle, float endAngle, bool isNegativeDirection, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
		: base(center, radius, colorArgb, pattern)
	{
		StartAngle = startAngle;
		EndAngle = endAngle;
		IsNegativeDirection = isNegativeDirection;
	}

	public ArcF()
		: this(new(0f, 0f), 0, 0, PI, false)
	{

	}

	public ArcF(PointF p1, PointF p2, PointF p3, int colorArgb = LightGreenArgb, string pattern = DefaultPattern)
		: base(Common.FindCenter(p1, p2, p3), p1, colorArgb, pattern)
	{
		var start = Common.FindAngleOfPointOnCircle(p1, Center);
		var mid = Common.FindAngleOfPointOnCircle(p2, Center);
		var end = Common.FindAngleOfPointOnCircle(p3, Center);

		var isNegDir = false;
		if(start > mid) {
			isNegDir = (start - mid) < PI;
		}
		if(mid > start) {
			isNegDir = (mid - start) > PI;
		}

#if DEBUG
		Console.WriteLine($"start={start}\nmid={mid}\nisNegDir={isNegDir}\n");
#endif
		StartAngle = start;
		EndAngle = end;
		IsNegativeDirection = isNegDir;
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
