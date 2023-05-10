using System.Drawing;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class Rectangle : Line
{
	public Rectangle(PointF upperLeft, PointF bottomRight, Color color, IEnumerator<bool> patternResolver)
		: base(upperLeft, bottomRight, color, patternResolver)
	{

	}
}
