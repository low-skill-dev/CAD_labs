using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;

public abstract class ALinearElement : IGraphicalElement
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
		while(true) yield return true;
	}

	public ALinearElement(Color color, IEnumerator<bool>? patternResolver = null)
	{
		this.Color = color;
		this.PatternResolver = patternResolver ?? GetDefaultPatternResolver();
	}
}
