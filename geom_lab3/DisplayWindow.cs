using OpenTK.Windowing.Desktop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace geom_lab3;
internal class DisplayWindow : GameWindow
{
	public float[] vertices;
	public DisplayWindow(int width, int height, string title = nameof(DisplayWindow))
		: base(GameWindowSettings.Default, new() { Size = new(width, height), Title = title })
	{
		base.RenderFrequency = 1;
	}
}
