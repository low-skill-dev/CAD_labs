using GraphicLibrary;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

namespace lab5;


public partial class MainWindow : Window
{
	enum States
	{
		WaitingFirstPoint,
		WaitingNextPoint,
		LoopCompleted
	}

	private States _currentState;
	private BitmapDrawer _drawer;
	private Point? _prevPoint;
	private Point? _firstPoint;

	public MainWindow()
	{
		InitializeComponent();
		LoopButton.IsEnabled = false;

		_currentState = States.WaitingFirstPoint;
		_drawer = new((int)this.ShowedImage.Width, (int)this.ShowedImage.Height);

		_drawer.RenderFrame();
		this.ShowedImage.Source = Common.BitmapToImageSource(_drawer.CurrentFrame);

		relX.Text = ((int)(this.ShowedImage.Width / 2)).ToString();
		relY.Text = ((int)(this.ShowedImage.Height / 2)).ToString();

		DebugOut.Text = $"Ожидание первой точки.";
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e)
	{
		_drawer.Reset();
		_drawer.AddPoint(new((int)(this.ShowedImage.Width / 2), (int)(this.ShowedImage.Height / 2)), System.Drawing.Color.LightCoral);
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;

		_currentState = States.WaitingFirstPoint;
		DebugOut.Text = $"Ожидание первой точки.";
		LoopButton.IsEnabled = false;
	}

	private void ShowedImage_Click(object sender, MouseButtonEventArgs e)
	{
		var pos = e.GetPosition(ShowedImage);

		if(_currentState == States.WaitingFirstPoint) {
			_prevPoint = _firstPoint = pos;
			_currentState = States.WaitingNextPoint;
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
		} else
		if(_currentState == States.WaitingNextPoint) {
			var p1 = new System.Drawing.Point((int)_prevPoint!.Value.X, (int)_prevPoint.Value.Y);
			var p2 = new System.Drawing.Point((int)pos.X, (int)pos.Y);

			_drawer.AddLine(p1, p2, null, GraphicLibrary.Models.ALinearElement.GetDefaultPatternResolver());
			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;

			if(pos.Equals(_firstPoint)) {
				_currentState = States.LoopCompleted;
				DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Контур замкнут.";
				LoopButton.IsEnabled = false;
				return;
			} else if(!_firstPoint.Equals(_prevPoint)) {
				LoopButton.IsEnabled = true;
			}

			_prevPoint = pos;
			_currentState = States.WaitingNextPoint;
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
		}

		return;
	}

	private void LoopButton_Click(object sender, RoutedEventArgs e)
	{
		_currentState = States.LoopCompleted;
		DebugOut.Text = DebugOut.Text.Split("...")[0] + $"... Контур замкнут.";
		LoopButton.IsEnabled = false;

		if(_prevPoint is null || _firstPoint is null) {
			throw new AggregateException();
		}

		_drawer.AddLine(
			Common.WindowsToDrawing(_prevPoint.Value),
			Common.WindowsToDrawing(_firstPoint.Value),
			null, GraphicLibrary.Models.ALinearElement.GetDefaultPatternResolver());

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void RotateButton_Click(object sender, RoutedEventArgs e)
	{
		System.Drawing.Point? relativeTo = null;
		try {
			relativeTo = new(int.Parse(relX.Text), int.Parse(relY.Text));
		} catch {
			relativeTo = new(
				(int)(this.ShowedImage.Width / 2),
				(int)(this.ShowedImage.Height / 2));
		}

		float angleD;
		try {
			angleD = float.Parse(RotateAngleIn.Text.Replace(',', '.'), CultureInfo.InvariantCulture) % 360;
		} catch {
			return;
		}

		float angleR = angleD * float.Pi/180;

		_drawer.RotateAll(angleR, relativeTo.Value);
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void MoveButton_Click(object sender, RoutedEventArgs e)
	{
		try {
			var dX = int.Parse(diffX.Text);
			var dY = int.Parse(diffY.Text);
			_drawer.MoveAll(dX, dY);
			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		} catch {
			return;
		}
	}

	private void DuplicateRotated_Click(object sender, RoutedEventArgs e)
	{
		System.Drawing.Point? relativeTo = null;
		try {
			relativeTo = new(int.Parse(relX.Text), int.Parse(relY.Text));
		} catch {
			relativeTo = new(
				(int)(this.ShowedImage.Width / 2),
				(int)(this.ShowedImage.Height / 2));
		}

		float angleD;
		try {
			angleD = float.Parse(RotateAngleIn.Text.Replace(',', '.'), CultureInfo.InvariantCulture) % 360;
		} catch {
			return;
		}

		float angleR = angleD * float.Pi / 180;

		_drawer.RotateAll(angleR, relativeTo.Value,true);
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void ScaleButton_Click(object sender, RoutedEventArgs e)
	{
		// увелич радиус от относительной точки
	}
}

