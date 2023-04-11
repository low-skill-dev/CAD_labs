namespace FastTestConsoleApp;

internal class Program
{
	static void Main(string[] args)
	{
		using MyWindow game = new(800, 600) {
			RenderFrequency = 30
		};
		game.Run();
	}
}
