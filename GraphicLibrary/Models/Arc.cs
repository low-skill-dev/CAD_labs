namespace GraphicLibrary.Models;

public class Arc
{
	public Circle Circle { get; set; }
	public float StartAngle { get; set; }
	public float EndAngle { get; set; }
	public bool IsNegativeDirection { get; set; }
	public Arc(Circle circle, float startAngle, float endAngle, bool negativeDirection)
	{
		Circle = circle;
		StartAngle = startAngle;
		EndAngle = endAngle;
		IsNegativeDirection = negativeDirection;
	}
}

