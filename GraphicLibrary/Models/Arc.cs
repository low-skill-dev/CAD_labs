using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;

public class Arc
{
	public Circle Circle { get; set; }
	public float StartAngle { get; set; }
	public float EndAngle { get; set; }
	public bool IsNegativeDirection { get; set; }
	public Arc(Circle circle, float startAngle, float endAngle, bool negativeDirection)
	{
		this.Circle = circle;
		this.StartAngle = startAngle;
		this.EndAngle = endAngle;
		this.IsNegativeDirection = negativeDirection;
	}
}

