using GraphicLibrary;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
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

namespace lab7;


public partial class MainWindow : Window
{
	enum States
	{
		WaitingFirstPoint,
		WaitingNextPoint,
		LoopCompleted,
	}


	private States _currentState;
	private BitmapDrawer _drawer;
	private System.Windows.Point? _prevMirror;
	private System.Windows.Point? _prevPoint;
	private System.Windows.Point? _firstPoint;

	private List<PointF> _points = new();
	private List<LineF> _lines = new();

	private System.Windows.Point _center => new(ShowedImage.Width / 2, ShowedImage.Height / 2);

	private LineF? _mirrorAxeLine;

	public MainWindow()
	{
		InitializeComponent();
		LoopButton.IsEnabled = false;

		_currentState = States.WaitingFirstPoint;
		_drawer = new((int)this.ShowedImage.Width, (int)this.ShowedImage.Height);

		_drawer.RenderFrame();
		this.ShowedImage.Source = Common.BitmapToImageSource(_drawer.CurrentFrame);

		DebugOut.Text = $"Ожидание первой точки.";
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e)
	{
		_points.Clear();
		_lines.Clear();

		_drawer.Reset();

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;

		_currentState = States.WaitingFirstPoint;
		DebugOut.Text = $"Ожидание первой точки.";
		LoopButton.IsEnabled = false;
		_firstPoint = null;
	}

	private void ShowedImage_Click(object sender, MouseButtonEventArgs e)
	{
		if(_currentState == States.LoopCompleted) return;

		var p = e.GetPosition(ShowedImage);
		var pos = new PointF((float)p.X, (float)p.Y);
		_points.Add(pos);

		if(_currentState == States.WaitingFirstPoint) {
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
			_currentState = States.WaitingNextPoint;
		} else if(_currentState == States.WaitingNextPoint) {
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
			_drawer.AddLine(_points[_points.Count - 2], _points[_points.Count - 1], null, ALinearElement.GetDefaultPatternResolver());
			_lines.Add(new(_points[_points.Count - 2], _points[_points.Count - 1]));
		}

		if(_points.Count > 2) LoopButton.IsEnabled = true;

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}



	private void LoopButton_Click(object sender, RoutedEventArgs e)
	{
		_currentState = States.LoopCompleted;
		DebugOut.Text = DebugOut.Text.Split("...")[0] + $"... Контур замкнут.";
		LoopButton.IsEnabled = false;

		_drawer.AddLine(
			_points[_points.Count - 1], _points[0],
			null, ALinearElement.GetDefaultPatternResolver());

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;

		PerimeterOut.Text = Common.FindPerimeter(_lines).ToString();
		AreaOut.Text = Common.FindArea(_lines).ToString();
	}

	private void Interpolate_Click(object sender, RoutedEventArgs e)
	{
		_drawer.LagrangePolynomials.Clear();

		var pattern = isPatternValid()
			? CreateUserResolver()
			: GraphicLibrary.Models.Line.GetPatternResolver16();

		var poly = new LagrangePolynomial(_points, (float)StepSelector.Value, System.Drawing.Color.AliceBlue,pattern);
		_drawer.LagrangePolynomials.Add(poly);

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}



	private bool isPatternValid()
	{
		var userPattern = this.PatterResolver.Text;
		if(string.IsNullOrWhiteSpace(userPattern) || userPattern.Any(c => c != '+' && c != '-')) {
			this.PatterResolver.Background = new SolidColorBrush(Colors.LightCoral);
			return false;
		}

		this.PatterResolver.Background = new SolidColorBrush(Colors.LightGreen);
		return true;
	}
	private IEnumerator<bool> CreateUserResolver()
	{
		var userPattern = this.PatterResolver.Text;
		while(true) {
			foreach(char c in userPattern) {
				if(c == '+') yield return true;
				if(c == '-') yield return false;
			}
		}
	}
}

