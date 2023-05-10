using GraphicLibrary;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using PointF = GraphicLibrary.MathModels.PointF;
namespace lab7;


public partial class MainWindow : Window
{
	//TODO: delete this
	public static float TrapezoidalArea(GraphicLibrary.MathModels.PointF[] points)
	{
		float area = 0;
		for(int i = 0; i < points.Length; i++) {
			int j = (i + 1) % points.Length;
			float height = MathF.Abs(points[j].Y - points[i].Y);
			float base1 = points[i].X;
			float base2 = points[j].X;
			float trapArea = (base1 + base2) * height / 2;
			area += trapArea;
		}
		return area;
	}

	private enum States
	{
		WaitingFirstPoint,
		WaitingNextPoint,
		LoopCompleted,
	}


	private States _currentState;
	private readonly BitmapDrawer _drawer;
	private System.Windows.Point? _prevMirror;
	private System.Windows.Point? _prevPoint;
	private System.Windows.Point? _firstPoint;
	private readonly List<LineF> _lines = new();

	public List<PointF> Points { get; } = new();


	private System.Windows.Point _center => new(ShowedImage.Width / 2, ShowedImage.Height / 2);

	private readonly LineF? _mirrorAxeLine;

	public MainWindow()
	{
		InitializeComponent();
		LoopButton.IsEnabled = false;

		_currentState = States.WaitingFirstPoint;
		_drawer = new((int)ShowedImage.Width, (int)ShowedImage.Height);

		_drawer.RenderFrame();
		ShowedImage.Source = Common.BitmapToImageSource(_drawer.CurrentFrame.Bitmap);

		StepSelector_PreviewMouseUp(null!, null!);
		DebugOut.Text = $"Ожидание первой точки.";
	}

	private void ClearButton_Click(object sender, RoutedEventArgs e)
	{
		Points.Clear();
		_lines.Clear();

		_drawer.Reset();

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;

		_currentState = States.WaitingFirstPoint;
		DebugOut.Text = $"Ожидание первой точки.";
		LoopButton.IsEnabled = false;
		_firstPoint = null;
	}

	[Obsolete]
	private void ShowedImage_Click(object sender, MouseButtonEventArgs e)
	{
		if(_currentState == States.LoopCompleted) {
			return;
		}

		var p = e.GetPosition(ShowedImage);
		var pos = new PointF((float)p.X, (float)p.Y);
		Points.Add(pos);

		if(_currentState == States.WaitingFirstPoint) {
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
			_currentState = States.WaitingNextPoint;
		} else if(_currentState == States.WaitingNextPoint) {
			DebugOut.Text = $"({(int)pos.X}; {(int)pos.Y}) ... Ожидание следующей точки.";
			_drawer.AddLine(Points[^2], Points[^1], null, ALinearElement.GetDefaultPatternResolver());
			_lines.Add(new(Points[^2], Points[^1]));
		}

		if(Points.Count > 2) {
			LoopButton.IsEnabled = true;
		}

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	[Obsolete]
	private void LoopButton_Click(object sender, RoutedEventArgs e)
	{
		try {
			_currentState = States.LoopCompleted;
			DebugOut.Text = DebugOut.Text.Split("...")[0] + $"... Контур замкнут.";
			LoopButton.IsEnabled = false;

			_drawer.AddLine(
				Points[^1], Points[0],
				null, ALinearElement.GetDefaultPatternResolver());

			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;

			PerimeterOut.Text = Common.FindPerimeter(_lines).ToString();
			AreaOut.Text = Common.FindArea(_lines).ToString();


			var actual = Common.FindArea(_lines);
			var expected = TrapezoidalArea(_lines.Select(x => x.End).ToArray());
			var sqrE = this.Height / 2 * this.Width / 2;
			if((int)MathF.Round(actual) != (int)MathF.Round(expected)) {
				//throw new AggregateException();
			}

		} catch { }
	}

	private void Interpolate_Click(object sender, RoutedEventArgs e)
	{
		try {
			_drawer.InterpolatedPoints.Clear();

			var pattern = isPatternValid()
				? CreateUserResolver()
				: GraphicLibrary.Models.Line.GetPatternResolver16();

			var poly = new InterpolatedPoints(Points, (float)StepSelector.Value, System.Drawing.Color.Aqua, pattern) {
				Degree = (int)Math.Round(DegreeSelector.Value),
				DebugDraw = DebugCurveOut.IsChecked ?? false
			};
			_drawer.InterpolatedPoints.Add(poly);

			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		} catch { }
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

	private void StepSelector_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		SliderSelected.Text = ((int)Math.Round(StepSelector.Value)).ToString();
		SliderDegreeSelected.Text = ((int)Math.Round(DegreeSelector.Value)).ToString();
		LagrangeSliderSelected.Text = ((int)Math.Round(LagrangeStepSelector.Value)).ToString();
		BesieSliderSelected.Text = ((int)Math.Round(BesieStepSelector.Value)).ToString();
		BesieBendSelected.Text = BesieBendSelector.Value.ToString("0.00");
	}

	private void DrawLagrangeBT_Click(object sender, RoutedEventArgs e)
	{
		try {
			_drawer.LagrangePolys.Clear();

			var pattern = GraphicLibrary.Models.Line.GetDefaultPatternResolver();

			var poly = new InterpolatedPoints(Points, (float)LagrangeStepSelector.Value, System.Drawing.Color.FloralWhite, pattern) {
				Degree = (int)Math.Round(DegreeSelector.Value),
				DebugDraw = DebugCurveOut.IsChecked ?? false
			};

			_drawer.LagrangePolys.Add(poly);

			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		} catch { }
	}

	private void DrawBesie2_Click(object sender, RoutedEventArgs e)
	{
		try {
			_drawer.Besie2Polys.Clear();

			var pattern = isPatternValid()
				? CreateUserResolver()
				: GraphicLibrary.Models.Line.GetPatternResolver16();

			var poly = new InterpolatedPoints(Points, (float)BesieStepSelector.Value, System.Drawing.Color.HotPink, pattern) {
				BendingFactor = (float)BesieBendSelector.Value,
				DebugDraw = DebugCurveOut.IsChecked ?? false
			};
			_drawer.Besie2Polys.Add(poly);

			_drawer.RenderFrame();
			ShowedImage.Source = _drawer.CurrentFrameImage;
		} catch { }
	}

	private void DebugCurveOut_Checked(object sender, RoutedEventArgs e)
	{
		var val = DebugCurveOut.IsChecked ?? false;

		_drawer.InterpolatedPoints.ForEach(poly => poly.DebugDraw = val);
		_drawer.LagrangePolys.ForEach(poly => poly.DebugDraw = val);
		_drawer.Besie2Polys.ForEach(poly => poly.DebugDraw = val);

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}

	private void WithSplineCorrection_Checked(object sender, RoutedEventArgs e)
	{
		var val = WithSplineCorrection.IsChecked ?? false;

		_drawer.InterpolatedPoints.ForEach(poly => poly.FirstSplineCorrection = val);
		_drawer.LagrangePolys.ForEach(poly => poly.FirstSplineCorrection = val);
		_drawer.Besie2Polys.ForEach(poly => poly.FirstSplineCorrection = val);

		_drawer.RenderFrame();
		ShowedImage.Source = _drawer.CurrentFrameImage;
	}
}

