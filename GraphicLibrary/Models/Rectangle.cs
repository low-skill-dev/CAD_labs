using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class Rectangle:Line
{
	public Rectangle(PointF upperLeft, PointF bottomRight, Color color, IEnumerator<bool> patternResolver)
		:base(upperLeft,bottomRight,color, patternResolver)
	{

	}
}
