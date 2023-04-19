namespace FastTestConsoleApp;

internal class Program
{
	private static void Main(string[] args)
	{
		using MyWindow game = new(800, 600) {
			RenderFrequency = 60
		};
		game.Run();
	}
}
