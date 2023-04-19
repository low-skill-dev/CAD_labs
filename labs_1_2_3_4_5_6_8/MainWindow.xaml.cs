using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointF = GraphicLibrary.MathModels.PointF;

namespace lab5;


public partial class MainWindow : Window
{
	private enum States
	{
		WaitingFirstPoint,
		WaitingNextPoint,
		LoopCompleted,
	}

	private enum OverrideStates
	{
		None,
		WaitingMirrorPoint,
		WaitingMirrorLine1,
		WaitingMirrorLine2,
		WaitingFillingPoint
	}

	private States _currentState;
	private OverrideStates _currentOverrideState = OverrideStates.None;
	private readonly BitmapDrawer _drawer;
	private System.Windows.Point? _prevMirror;
	private System.Windows.Point? _prevPoint;
	private System.Windows.Point? _firstPoint;

	private System.Windows.Point _center => new(ShowedImage.Width / 2, ShowedImage.Height / 2);

	private LineF? _mirrorAxeLine;

	public MainWindow()
	{
		InitializeComponent();
		LoopButton.IsEnabled = false;

		_currentState = States.WaitingFirstPoint;
		_drawer = new((int)ShowedImage.Width / 2, (int)ShowedImage.Height / 2);

		_drawer.RenderFrame();
		ShowedImage.Source = Common.BitmapToImageSource(_drawer.CurrentFrame.Bitmap);

		relX.Text = relXscale.Text = _center.X.ToString();
		relY.Text = relYscale.Text = _center.Y.ToString();

		MirrorX.Text = _center.X.ToString();
		MirrorY.Text = _center.Y.ToString();
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e)
	{
		_drawer.Reset();

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;

		_currentState = States.WaitingFirstPoint;
		DebugOut.Text = $"Ожидание первой точки.";
		LoopButton.IsEnabled = false;
		_firstPoint = null;
	}
	private bool isPatternValid()
	{
		var userPattern = PatterResolver.Text;
		if(string.IsNullOrWhiteSpace(userPattern) || userPattern.Any(c => c != '+' && c != '-')) {
			PatterResolver.Background = new SolidColorBrush(Colors.LightCoral);
			return false;
		}

		PatterResolver.Background = new SolidColorBrush(Colors.LightGreen);
		return true;
	}
	private IEnumerator<bool> CreateUserResolver()
	{
		var userPattern = PatterResolver.Text;
		while(true) {
			foreach(var c in userPattern) {
				if(c == '+') {
					yield return true;
				}

				if(c == '-') {
					yield return false;
				}
			}
		}
	}

	[Obsolete]
	private void ShowedImage_Click(object sender, MouseButtonEventArgs e)
	{
		var pos = e.GetPosition(ShowedImage);
		pos = new(pos.X / 2, pos.Y / 2);

		if(_currentOverrideState != OverrideStates.None) {
			if(_currentOverrideState == OverrideStates.WaitingMirrorPoint) {
				MirrorX.Text = pos.X.ToString();
				MirrorY.Text = pos.Y.ToString();
				_currentOverrideState = OverrideStates.None;
			} else if(_currentOverrideState == OverrideStates.WaitingMirrorLine1) {
				MirrorX1.Text = pos.X.ToString();
				MirrorY1.Text = pos.Y.ToString();
				_prevMirror = pos;
				_currentOverrideState = OverrideStates.WaitingMirrorLine2;
			} else if(_currentOverrideState == OverrideStates.WaitingMirrorLine2) {
				MirrorX2.Text = pos.X.ToString();
				MirrorY2.Text = pos.Y.ToString();

				var prev = _prevMirror!.Value;
				var curr = pos;

				var start = (PointF)prev;
				var end = new PointF((float)curr.X, (float)curr.Y);
				_drawer.ConstantObjects.Clear();
				_drawer.ConstantObjects.Add(new GraphicLibrary.Models.Line(start, end, System.Drawing.Color.Red, null));
				_mirrorAxeLine = new(start, end);

				_drawer.RenderFrame();
				ShowedImage.Source = _drawer.CurrentFrameImage;

				_currentOverrideState = OverrideStates.None;
			} else if(_currentOverrideState == OverrideStates.WaitingFillingPoint) {
				_drawer.AddFiller(new(new((float)pos.X, (float)pos.Y), SelectedFillColor!.Value));

				_drawer.RenderFrame();
				ShowedImage.Source = _drawer.CurrentFrameImage;
				_currentOverrideState = OverrideStates.None;
			}
		} else {
			if(_currentState == States.WaitingFirstPoint) {
				_firstPoint ??= pos;
				_prevPoint = pos;
				_currentState = States.WaitingNextPoint;
				DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
			} else
			if(_currentState == States.WaitingNextPoint) {
				var p1 = new System.Drawing.PointF((float)_prevPoint!.Value.X, (float)_prevPoint.Value.Y);
				var p2 = new System.Drawing.PointF((float)pos.X, (float)pos.Y);
				if(isPatternValid()) {
					if((isSimpleLine.IsChecked ?? false) || (isBresLine.IsChecked ?? false)) {
						_drawer.AddLine(p1, p2, null, CreateUserResolver());
					} else
					if((isSimpleCircle.IsChecked ?? false) || (isBresCircle.IsChecked ?? false)) {
						_drawer.AddCircle(p1, p2, null, CreateUserResolver());
					}
				} else {
					if(isSimpleLine.IsChecked ?? false) {
						_drawer.AddLine(p1, p2, null, GraphicLibrary.Models.Line.GetPatternResolver16());
					} else if(isBresLine.IsChecked ?? false) {
						_drawer.AddLine(p1, p2, null, GraphicLibrary.Models.Line.GetBresenhamPatternResolver16());
					} else if(isSimpleCircle.IsChecked ?? false) {
						_drawer.AddCircle(p1, p2, null, GraphicLibrary.Models.Circle.GetPatternResolver16());
					} else if(isBresCircle.IsChecked ?? false) {
						_drawer.AddCircle(p1, p2, null, GraphicLibrary.Models.Circle.GetBresenhamPatternResolver16());
					}
				}
				_currentState = States.WaitingFirstPoint;
				DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание первой точки.";
			}

			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;

			if(pos.Equals(_firstPoint)) {
				//_currentState = States.LoopCompleted;
				//DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Контур замкнут.";
				//LoopButton.IsEnabled = false;
				//return;
			} else if(!_firstPoint.Equals(_prevPoint)) {
				LoopButton.IsEnabled = true;
			}

			_prevPoint = pos;
		}

		return;
	}

	[Obsolete]
	private void LoopButton_Click(object sender, RoutedEventArgs e)
	{
		_currentState = States.WaitingFirstPoint;
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
		System.Drawing.Point? relativeTo;
		try {
			relativeTo = new(int.Parse(relX.Text), int.Parse(relY.Text));
		} catch {
			relativeTo = new(
				(int)(ShowedImage.Width / 2),
				(int)(ShowedImage.Height / 2));
		}

		float angleD;
		try {
			angleD = float.Parse(RotateAngleIn.Text.Replace(',', '.'), CultureInfo.InvariantCulture) % 360;
		} catch {
			return;
		}

		var angleR = angleD * float.Pi / 180;

		_drawer.RotateAll(angleR, relativeTo.Value);
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void MoveButton_Click(object sender, RoutedEventArgs e)
	{
		if(string.IsNullOrWhiteSpace(diffX.Text)) {
			diffX.Text = "0";
		}

		if(string.IsNullOrWhiteSpace(diffY.Text)) {
			diffY.Text = "0";
		}

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
		System.Drawing.Point? relativeTo;
		try {
			relativeTo = new(int.Parse(relX.Text), int.Parse(relY.Text));
		} catch {
			relativeTo = new(
				(int)(ShowedImage.Width / 2),
				(int)(ShowedImage.Height / 2));
		}

		float angleD;
		try {
			angleD = float.Parse(RotateAngleIn.Text.Replace(',', '.'), CultureInfo.InvariantCulture) % 360;
		} catch {
			return;
		}

		var angleR = angleD * float.Pi / 180;


		_drawer.RotateAll(angleR, relativeTo.Value, true);
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void ScaleButton_Click(object sender, RoutedEventArgs e)
	{
		System.Windows.Point? relativeTo;
		try {
			relativeTo = new(int.Parse(relXscale.Text), int.Parse(relYscale.Text));
		} catch {
			relativeTo = _center;
		}


		float scale;
		try {
			scale = float.Parse(ScaleIn.Text.Replace(',', '.'), CultureInfo.InvariantCulture) % 360;
			if(scale < 0.0001 || scale > 1000) {
				return;
			}
		} catch {
			return;
		}

		_drawer.ScaleAll(scale, Common.WindowsToDrawing(relativeTo.Value));
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void SelectMirrorPointButton_Click(object sender, RoutedEventArgs e)
	{
		_currentOverrideState = OverrideStates.WaitingMirrorPoint;
	}

	private void MirrorByPoint_Click(object sender, RoutedEventArgs e)
	{
		float x, y;
		try {
			x = float.Parse(MirrorX.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
			y = float.Parse(MirrorY.Text.Replace(',', '.'), CultureInfo.InvariantCulture);
		} catch {
			return;
		}

		_drawer.MirrorAll(new System.Drawing.PointF(x, y));
		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void MirrorByLine_Click(object sender, RoutedEventArgs e)
	{
		if(_mirrorAxeLine is not null) {
			_drawer.MirrorAll(_mirrorAxeLine);
			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		}
	}

	private void SelectMirrorLine2Button_Click(object sender, RoutedEventArgs e)
	{
		_currentOverrideState = OverrideStates.WaitingMirrorLine2;
	}

	private void SelectMirrorLine1Button_Click(object sender, RoutedEventArgs e)
	{
		_currentOverrideState = OverrideStates.WaitingMirrorLine1;
	}

	private System.Drawing.Color? SelectedFillColor;
	private void SelectColorButton_Click(object sender, RoutedEventArgs e)
	{
		var colorDialog = new System.Windows.Forms.ColorDialog();
		if(colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
			SelectColorButton.Background = new SolidColorBrush(System.Windows.Media.Color
				.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
			SelectedFillColor = System.Drawing.Color
				.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B);
		}
	}

	private void FillButton_Click(object sender, RoutedEventArgs e)
	{
		if(SelectedFillColor is null) {
			return;
		}

		_currentOverrideState = OverrideStates.WaitingFillingPoint;
	}

	private void Interpolate_Click(object sender, RoutedEventArgs e)
	{

	}
}

