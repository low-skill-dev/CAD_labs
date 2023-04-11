using InteractiveLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
	private bool disableBallFilling;
	public MainWindow()
	{
		InitializeComponent();
		disableBallFilling = true;

		//gameRender = new((int)GameImage.Width, (int)GameImage.Height, 10) {
		//	DisableBallFilling = disableBallFilling,
		//	UseRandomColors = true
		//};
		//gameRender.StartNewGame();
		//gameRender.RenderCurrentState();


		//GameImage.Source = gameRender.CurrentFrameImage;
	}

	private void StartNewGameBT_Click(object sender, RoutedEventArgs e)
	{
		if(BallAccel == 0) {
			BallAccel = 100;
			this.BallAccelerationTB.Text = BallAccel.ToString("0.00");
		}
		var parsed = int.TryParse(BallAccelerationTB.Text, out int accel);
		gameRender = new((int)GameImage.Width, (int)GameImage.Height, 10, ballAcceleration: parsed ? accel : BallAccel) {
			DisableBallFilling = disableBallFilling,
			UseRandomColors = true
		};


		StartNewGameBT.IsEnabled = false;

		totalRoundsPlayed++;
		UpdateTBs();
		this.Player_message.Text = string.Empty;

		gameRender.StartNewGame();
		gameRender.RenderCurrentState();
		GameImage.Source = gameRender.CurrentFrameImage;

		breakThread = true;
		Thread.Sleep(101);
		breakThread = false;
		ThreadStart thr = new(() => {
			while(true) {
				this.Dispatcher.Invoke(() => {
					try {
						gameRender.UpdateState(0.025f);
					} catch(GameException ex) {
						this.Player_message.Text = ex.Message;
						breakThread = true;
						UpdateTBs();
					} catch(GameWonException ex) {
						this.Player_message.Text = ex.Message;
						breakThread = true;
						wonRounds++;
						this.BallAccel *= 1.5f;
						this.BallAccelerationTB.Text = ((int)BallAccel).ToString();
						UpdateTBs();
					} finally {
						gameRender.RenderCurrentState();
						GameImage.Source = gameRender.CurrentFrameImage;
					}
				});

				if(!breakThread)
					Thread.Sleep(25);
				else
					break;
			}
		});
		updThread = new Thread(thr) { IsBackground = true };
		updThread.Start();

		StartNewGameBT.IsEnabled = true;
	}

	private void UpdateTBs()
	{
		this.TotalRoundsTB.Text = totalRoundsPlayed.ToString();
		this.WonRoundsTB.Text = wonRounds.ToString();
	}

	private void GameImage_MouseMove(object sender, MouseEventArgs e)
	{
		var pos = e.GetPosition((Image)sender);

		this.gameRender.SetPlatformX((float)pos.X);
	}
}
