using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;

public abstract class ALinearElement
{
	public Color Color { get; init; }
	public IEnumerator<bool> PatternResolver { get; init; }

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
