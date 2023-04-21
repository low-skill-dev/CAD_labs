using InteractiveLibrary;
using System.Runtime;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace geom_lab1_18;
public partial class MainWindow : Window
{
	private SquaresRunner gameRender;
	private Thread? updThread;
	private bool breakThread;
	public MainWindow()
	{
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

		InitializeComponent();

		gameRender = null!;
	}

	private void StartNewGameBT_Click(object sender, RoutedEventArgs e)
	{
		gameRender = new((int)GameImage.Width, (int)GameImage.Height) {
			AddAxes = false
		};

		lastGameStarted = DateTime.UtcNow;
		StartNewGameBT.IsEnabled = false;

		gameRender.RenderCurrentState();
		GameImage.Source = gameRender.CurrentFrameImage;

		breakThread = true;
		Thread.Sleep(50);
		breakThread = false;
		ThreadStart thr = new(() => {
			while(true) {
				Dispatcher.Invoke(() => {
					UpdateTBs();
					try {
						gameRender.UpdateState(0.025f);
					} catch(GameException ex) {
						Player_message.Text = ex.Message;
						breakThread = true;
					} finally {
						gameRender.RenderCurrentState();
						GameImage.Source = gameRender.CurrentFrameImage;
					}
				});

				if(!breakThread) {
					Thread.Sleep(25);
				} else {
					break;
				}
			}
		});
		updThread = new Thread(thr) { IsBackground = true };
		updThread.Start();

		StartNewGameBT.IsEnabled = true;
	}

	DateTime lastGameStarted = DateTime.UtcNow;
	private void UpdateTBs()
	{
		TotalRoundsTB.Text = (DateTime.UtcNow - lastGameStarted).TotalSeconds.ToString("0.0");
		//WonRoundsTB.Text = wonRounds.ToString();
	}

	private void GameImage_MouseMove(object sender, MouseEventArgs e)
	{
		var pos = e.GetPosition((Image)sender);

		gameRender.MouseAt = pos;
	}
}
