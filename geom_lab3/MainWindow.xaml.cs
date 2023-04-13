using System;
using System.Collections.Generic;
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
using GraphicLibrary;
using GraphicLibrary.Models;
using static System.MathF;
using PointF = GraphicLibrary.MathModels.PointF;
using LineF = GraphicLibrary.MathModels.LineF;
using OpenTK.Mathematics;
using FastTestConsoleApp;
using System.Runtime.InteropServices;

namespace geom_lab3;

/* I. Номера по журналу 1,4,7,10,13,16,19,22,25.
 * Линию, соединяющую первую и последнюю введенные точки, считать осью тела вращения.
 * Повернуть относительно нее все введенные точки на 360градусов через некоторый шаг
 * (определяемым пользователем, по умолчанию - 30градусов).
 * 
 * Сначала введем 2D фигуру, скопировав, преимущественно, прошлые лабы (добавив ссылку на проект).
 * Далее проведем компрессию - самую левую точку сместим к левому краю, самую нижнюю - вниз,
 * ибо абсолютные координаты точек нам более не интересны.
 * 
 * Создаздим окно OpenGL в Core-режиме, используем шейдер со сплошным цветом.
 * Прорисуем полученные точки.
 */
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

	public List<PointF> Points => _points;


	private System.Windows.Point _center => new(ShowedImage.Width / 2, ShowedImage.Height / 2);

	private LineF? _mirrorAxeLine;

	[DllImport("kernel32.dll")] public static extern bool AllocConsole();
	public MainWindow()
	{
		AllocConsole();
		Console.WriteLine("Console is working!");

		InitializeComponent();
		LoopButton.IsEnabled = false;

		_currentState = States.WaitingFirstPoint;
		_drawer = new((int)this.ShowedImage.Width, (int)this.ShowedImage.Height);

		_drawer.RenderFrame();
		this.ShowedImage.Source = Common.BitmapToImageSource(_drawer.CurrentFrame);

		StepSelector_PreviewMouseUp(null!, null!);
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
		try {
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
		} catch { }
	}

	private void Interpolate_Click(object sender, RoutedEventArgs e)
	{
		try {
			_drawer.InterpolatedPoints.Clear();

			var pattern = isPatternValid()
				? CreateUserResolver()
				: GraphicLibrary.Models.Line.GetPatternResolver16();

			var poly = new InterpolatedPoints(_points, (float)StepSelector.Value, System.Drawing.Color.Aqua, pattern) {
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

	private void StepSelector_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.SliderSelected.Text = ((int)Math.Round(StepSelector.Value)).ToString();
		this.SliderDegreeSelected.Text = ((int)Math.Round(DegreeSelector.Value)).ToString();
		this.LagrangeSliderSelected.Text = ((int)Math.Round(LagrangeStepSelector.Value)).ToString();
		this.BesieSliderSelected.Text = ((int)Math.Round(BesieStepSelector.Value)).ToString();
		this.BesieBendSelected.Text = BesieBendSelector.Value.ToString("0.00");
	}

	private void DrawLagrangeBT_Click(object sender, RoutedEventArgs e)
	{
		try {
			_drawer.LagrangePolys.Clear();

			var pattern = GraphicLibrary.Models.Line.GetDefaultPatternResolver();

			var poly = new InterpolatedPoints(_points, (float)LagrangeStepSelector.Value, System.Drawing.Color.FloralWhite, pattern) {
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

			var poly = new InterpolatedPoints(_points, (float)BesieStepSelector.Value, System.Drawing.Color.HotPink, pattern) {
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

	private void CreateOpenGlBT_Click(object sender, RoutedEventArgs e)
	{
		if(string.IsNullOrWhiteSpace(RotatePerStepTB.Text)) {
			RotatePerStepTB.Text = "90";
		}

		if(_currentState == States.WaitingNextPoint) {
			LoopButton_Click(null!, null!);
		}

		float stepD = 0;
		try {
			stepD = float.Parse(RotatePerStepTB.Text);
			var clr = System.Drawing.Color.LightGreen;
			RotatePerStepTB.Background = new SolidColorBrush(Color.FromArgb(clr.A, clr.R, clr.G, clr.B));
			stepD = stepD % 360;
		} catch {
			var clr = System.Drawing.Color.LightCoral;
			RotatePerStepTB.Background = new SolidColorBrush(Color.FromArgb(clr.A, clr.R, clr.G, clr.B));
			return;
		}

		var points = _points.Select(x => x.Clone()).ToArray();

		/* Проведем мысленное моделирование. Возьмем нашу фигуру. 
		 * Положим её на плоскость XY таким образом, что линия, соединяющая её начальную и конечную точки
		 * совпадает с осью X. Этого можно добиться вращением исходной фигуры в двухмерной плоскости.
		 *
		 * Тогда, вращение каждой точки вокруг оси Х будет представлять собой уравнение окружности, радиус
		 * в котором равен координате Y исходной точки.
		 * Тогда каждую точку сможет представить в плоскости YZ, где начальная координата Y=Y, Z=0.
		 */
		var first = points.At(0);
		var last = points.At(-1);
		var (k, b) = Common.FindLinearEquation(new(first, last)); // получим ось


		/* Сначала положим первую точку на ось Х.
		 * Заодно сдвинем её в начало координат для упрощения.
		 * Потом довернем все остальные точки.
		 */
		var delta = new PointF(first.X, first.Y);
		points = points.Select(p => p - delta).ToArray();
		first = points.At(0);
		last = points.At(-1);

		var dA = -1 * Common.FindAngleOfPointOnCircle(first, last);
		points = points.Select(p => Common.RotatePoint(p, first, dA)).ToArray();


		/* Перейдем в трехмерное пространство
		 */
		var points3 = points.Select(p => new Vector3(p.X, p.Y, 0f)).ToArray();

		/* Теперь имеет каждую точку, у которой Х - константа, YZ - плоскость вращения.
		 * Будем вращать! Ось вращения - начало координат.
		 */
		float stepR = stepD * float.Pi / 180;
		List<Vector3[]> resultingCopies = new((int)Ceiling(2 * PI / stepR));
		var start = new PointF(0f, 0f);
		for(float angleR = 0; angleR <= 2 * PI; angleR += stepR) {
			resultingCopies.Add(points3.Select(p => {
				var pointYZ = Common.RotatePoint(new PointF(p.Y, p.Z), start, angleR);
				return new Vector3(p.X, pointYZ.X, pointYZ.Y);
			}).ToArray());
		} // после данного цикла имеет полный набор точек в трехмерном пространстве


		/* Но дело в том, что сама по-себе фигура нам не интересна - нам нужна её поверхность.
		 * Для этог оследует соединять точки копий между собой.
		 * 
		 * Для этого необходимо соединять предыдущую точку с настоящей.
		 * Проведем следующее преобразование.
		 * 
		 * Имеем каждую точку, у которой Х - константа, YZ - плоскость вращения.
		 * Будем вращать! Ось вращения - начало координат.
		 */

		List<Vector3> resultingConnected = new((int)Ceiling(points3.Length * 2 * (2 * PI / stepR)));
		for(float angleR = 0; angleR < 2 * PI; angleR += stepR) {
			// нужно 4-х угольник для каждой пары точек
			var current = points3;
			for(int i = 0; i < current.Length; i++) {
				// треугольник 4-х угольника 2
				resultingConnected.Add(RotateYZ(current.At(i), angleR));
				resultingConnected.Add(RotateYZ(current.At(i + 1), angleR));
				resultingConnected.Add(RotateYZ(current.At(i), angleR + stepR));

				// треугольник 4-х угольника 2
				resultingConnected.Add(RotateYZ(current.At(i+1), angleR));
				resultingConnected.Add(RotateYZ(current.At(i), angleR + stepR));
				resultingConnected.Add(RotateYZ(current.At(i+1), angleR + stepR));
			}
		}

		/* Нормализуем значения
		 * OpenGL оперирует в пространстве от -1 до 1
		 */
		var flat = resultingCopies.SelectMany(x => x); ;
		var maxX = Abs(flat.Max(x => Abs(x.X)));
		var maxY = Abs(flat.Max(x => Abs(x.Y)));
		var maxZ = Abs(flat.Max(x => Abs(x.Y)));

		var result = flat.Select(x => new Vector3(x.X / maxX, x.Y / maxY, x.Z / maxZ)).SelectMany(x => new float[] { x.X, x.Y, x.Z }).ToArray();

		var t = result.Distinct().ToArray();

		var resultNew = resultingConnected.Select(x => new Vector3(x.X / maxX, x.Y / maxY, x.Z / maxZ)).SelectMany(x => new float[] { x.X, x.Y, x.Z }).ToArray();

		MyWindow objWin = new(1700, 680, resultNew);
		objWin.Run();
	}

	public static Vector3 RotateYZ(Vector3 v, float angleR, PointF? relativeToYZ = null)
	{
		var pointYZ = Common.RotatePoint(new PointF(v.Y, v.Z), relativeToYZ ?? new(0f, 0f), angleR);
		return new Vector3(v.X, pointYZ.X, pointYZ.Y);
	}
}

