using InteractiveLibrary;
using System.Runtime;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace geom_lab1;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private PongCatcher gameRender;
	private Thread? updThread;
	private bool breakThread;
	private float BallAccel;
	private int totalRoundsPlayed;
	private int wonRounds;
	private readonly bool disableBallFilling;
	public MainWindow()
	{
		GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;

		InitializeComponent();
		disableBallFilling = true;

		gameRender = null!;
	}

	private void StartNewGameBT_Click(object sender, RoutedEventArgs e)
	{
		if(BallAccel == 0) {
			BallAccel = 100;
			BallAccelerationTB.Text = BallAccel.ToString("0.00");
		}

		var ballsCount = 10;
		try {
			ballsCount = int.Parse(NumofBallsTB.Text);
		} catch { }

		var parsed = int.TryParse(BallAccelerationTB.Text, out var accel);
		gameRender = new((int)GameImage.Width, (int)GameImage.Height, ballsCount, ballAcceleration: parsed ? accel : BallAccel, ballBorderColor: System.Drawing.Color.Yellow) {
			DisableBallFilling = disableBallFilling,
			UseRandomColors = false
		};


		StartNewGameBT.IsEnabled = false;

		totalRoundsPlayed++;
		UpdateTBs();
		Player_message.Text = string.Empty;

		gameRender.StartNewGame();
		gameRender.RenderCurrentState();
		GameImage.Source = gameRender.CurrentFrameImage;

		breakThread = true;
		Thread.Sleep(101);
		breakThread = false;
		ThreadStart thr = new(() => {
			while(true) {
				Dispatcher.Invoke(() => {
					try {
						gameRender.UpdateState(0.025f);
					} catch(GameException ex) {
						Player_message.Text = ex.Message;
						breakThread = true;
						UpdateTBs();
					} catch(GameWonException ex) {
						Player_message.Text = ex.Message;
						breakThread = true;
						wonRounds++;
						BallAccel *= 1.5f;
						BallAccelerationTB.Text = ((int)BallAccel).ToString();
						UpdateTBs();
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

	private void UpdateTBs()
	{
		TotalRoundsTB.Text = totalRoundsPlayed.ToString();
		WonRoundsTB.Text = wonRounds.ToString();
	}

	private void GameImage_MouseMove(object sender, MouseEventArgs e)
	{
		var pos = e.GetPosition((Image)sender);

		gameRender.SetPlatformX((float)pos.X);
	}
}
