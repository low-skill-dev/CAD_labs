using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class Line
{
	public Point Start { get; init; }
	public Point End { get; init; }
	public Color Color { get; init; }
	public IEnumerator<bool> PatternResolver { get; init; }


	// solid line
	public static IEnumerator<bool> GetDefaultResolver()
	{
		while(true) yield return true;
	}
	// 16. Штрихпунктирная линия 8: линия из 3-х пикселей, пропуск 3-х пикселей, пиксель, пропуск 3-х пикселей…
	public static IEnumerator<bool> GetResolver16()
	{
		int i;
		while(true) {
			for(i = 0; i < 3; i++) yield return true; // линия из 3-х пикселей
			for(i = 0; i < 3; i++) yield return false; // пропуск 3-х пикселей
			for(i = 0; i < 1; i++) yield return true; // пиксель
			for(i = 0; i < 3; i++) yield return false; // пропуск 3-х пикселей
		}
	}

	public Line(System.Drawing.Point start, System.Drawing.Point end, Color color, IEnumerator<bool>? patternResolver = null)
	{
		this.Start = start;
		this.End = end;
		this.Color = color;
		this.PatternResolver = patternResolver ?? GetDefaultResolver();
	}

	public Line(System.Windows.Point start, System.Windows.Point end, Color color, IEnumerator<bool>? patternResolver = null)
		: this(
			  new System.Drawing.Point((int)start.X, (int)start.Y),
			  new System.Drawing.Point((int)end.X, (int)end.Y),
			  color, patternResolver)
	{ }

}
