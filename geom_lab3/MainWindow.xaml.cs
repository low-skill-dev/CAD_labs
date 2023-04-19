using GraphicLibrary;
using GraphicLibrary.Models;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using static System.MathF;
using LineF = GraphicLibrary.MathModels.LineF;
using PointF = GraphicLibrary.MathModels.PointF;

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

	[DllImport("kernel32.dll")] public static extern bool AllocConsole();
	public MainWindow()
	{
		_ = AllocConsole();
		Console.WriteLine("Console is working!");

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


	private SolidColorBrush redBrush => new(Color.FromArgb(
		 System.Drawing.Color.LightCoral.A,
		 System.Drawing.Color.LightCoral.R,
		 System.Drawing.Color.LightCoral.G,
		 System.Drawing.Color.LightCoral.B));
	private SolidColorBrush greenBrush => new(Color.FromArgb(
		 System.Drawing.Color.LightCoral.A,
		 System.Drawing.Color.LightCoral.R,
		 System.Drawing.Color.LightCoral.G,
		 System.Drawing.Color.LightCoral.B));

	[Obsolete]
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
			stepD = float.Parse(RotatePerStepTB.Text) % 360;
			RotatePerStepTB.Background = greenBrush;
		} catch {
			RotatePerStepTB.Background = redBrush;
			return;
		}
		var stepR = stepD * float.Pi / 180;
		var points = Points.ToArray();

		/* Проведем мысленное моделирование. Возьмем нашу фигуру. Положим её на плоскость XY таким образом, 
		 * что линия, соединяющая её начальную и конечную точки совпадает с осью X. Этого можно добиться 
		 * вращением исходной фигуры в двухмерной плоскости. Тогда, вращение каждой точки вокруг оси Х будет 
		 * представлять собой уравнение окружности, радиус в котором равен координате Y исходной точки.
		 * Тогда, каждую точку сможем представить в плоскости YZ, где начальная координата Y=Y, Z=0.
		 */
		var first = points.At(0); var last = points.At(-1);

		/* Сначала положим первую точку на ось Х. Заодно сдвинем её в начало координат для упрощения.
		 * Потом довернем все остальные точки.
		 */
		var dXY = new PointF(first.X, first.Y);
		points = points.Select(p => p - dXY).ToArray();
		first = points.At(0); last = points.At(-1);
		var dA = -1 * Common.FindAngleOfPointOnCircle(first, last);
		points = points.Select(p => Common.RotatePoint(p, first, dA)).ToArray();

		// Перейдем в трехмерное пространство
		var points3 = points.Select(p => new Vector3(p.X, p.Y, 0f)).ToArray();

		/* Cама по-себе фигура нам не интересна - нам нужна её поверхность. Для этого следует соединять точки 
		 * копий фигуры между собой. Если пронумеровать последовательные точки плоской фигуры как 0, 1, 2...N, а
		 * функцию вращения точки на stepR обозначения за R то построение поверхности между ними будет выглядеть
		 * как построение двух треугольников в 3-х мерном пространстве, выраженных следующими координатами
		 * вершин: 0 -> 1 -> R(1) и R(0) -> R(1) -> 1. Очевидно, что если мы имеем случай, где точка 0 или 1 лежит 
		 * на оси Х, то второй треугольник не нужен - он будет дублировать первый. Имеем каждую точку, 
		 * у которой Х - константа, YZ - плоскость вращения. Будем вращать! Ось вращения - начало координат.
		 */
		List<Vector3> resultingConnected = new((int)Ceiling(points3.Length * 2 * (2 * PI / stepR)));
		for(float angleR = 0; angleR < 2 * PI; angleR += stepR) {
			// создаваемая поверхность представляет 4-х угольник, т.е. 2 треугольника
			var current = points3;
			for(var i = 0; i < current.Length - 1; i++) { // -1 т.к. первую и последнюю не соединяем - они лежат но оси!
														  // 1-й треугольник 4-х угольника
				var p1 = RotateYZ(current.At(i), angleR);
				var p2 = RotateYZ(current.At(i + 1), angleR);
				var p3 = RotateYZ(current.At(i + 1), angleR + stepR);

				resultingConnected.Add(p1);
				resultingConnected.Add(p2);
				resultingConnected.Add(p3);

				// 2-й треугольник 4-х угольника
				var p4 = RotateYZ(current.At(i), angleR + stepR);

				if(p1.Equals(p4)) {
					continue;  // одна из точек лежит на оси (первый и последний треуг)		
				}

				resultingConnected.Add(p1);
				resultingConnected.Add(p3);
				resultingConnected.Add(p4);
			}
		}

		// Нормализуем значения - OpenGL оперирует в пространстве от -1 до 1
		var maxX = Abs(resultingConnected.Max(x => Abs(x.X)));
		var maxY = Abs(resultingConnected.Max(x => Abs(x.Y)));
		var maxZ = Abs(resultingConnected.Max(x => Abs(x.Z)));
		var result = resultingConnected.Select(x => new Vector3(x.X / maxX, x.Y / maxY, x.Z / maxZ)).SelectMany(x => new float[] { x.X, x.Y, x.Z }).ToArray();

		var objWin = new MyWindow(1700, 680, result);
		objWin.Run();
	}

	public static Vector3 RotateYZ(Vector3 v, float angleR, PointF? relativeToYZ = null)
	{
		var pointYZ = Common.RotatePoint(new PointF(v.Y, v.Z), relativeToYZ ?? new(0f, 0f), angleR);
		return new Vector3(v.X, pointYZ.X, pointYZ.Y);
	}
}

