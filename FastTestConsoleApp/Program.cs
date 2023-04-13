using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace FastTestConsoleApp;

internal class Program
{
	static void Main(string[] args)
	{
		using MyWindow game = new(800, 600) {
			RenderFrequency = 60
		};
		game.Run();
	}
}
