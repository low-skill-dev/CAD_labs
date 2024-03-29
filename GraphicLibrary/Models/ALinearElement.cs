﻿using GraphicLibrary.MathModels;
using System.Drawing;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;

public abstract class ALinearElement : IGraphicalElement, IColoredElement
{
	public Color Color { get; init; }
	public IEnumerator<bool> PatternResolver { get; init; }

	#region IGraphicalElement
	public abstract void Move(float dX, float dY);
	public abstract void Rotate(float angleR, PointF relativeTo);
	public abstract void Scale(float scale, PointF relativeTo);
	public abstract void Mirror(PointF relativeTo);
	public abstract void Mirror(LineF relativeTo);

	public abstract IGraphicalElement Clone();
	#endregion

	// solid line
	public static IEnumerator<bool> GetDefaultPatternResolver()
	{
		while(true) {
			yield return true;
		}
	}

	public ALinearElement(Color color, IEnumerator<bool>? patternResolver = null)
	{
		Color = color;
		PatternResolver = patternResolver ?? GetDefaultPatternResolver();
	}
}
