using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;
using static System.MathF;
using System.Security.Cryptography.X509Certificates;
using GraphicLibrary.MathModels;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary;

public class Common
{
	// https://stackoverflow.com/a/22501616/11325184
	public static BitmapImage BitmapToImageSource(Bitmap bitmap)
	{
		using(MemoryStream memory = new MemoryStream()) {
			bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
			memory.Position = 0;
			BitmapImage bitmapimage = new BitmapImage();
			bitmapimage.BeginInit();
			bitmapimage.StreamSource = memory;
			bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapimage.EndInit();

			return bitmapimage;
		}
	}

	public PointF PointsAverage(PointF first, PointF second)
	{
		return (first + second) / 2;
	}

	// System.Windows.Point -> System.Drawing.PointF
	public static PointF WindowsToDrawing(System.Windows.Point p)
		=> new PointF((float)p.X, (float)p.Y);

	// радиус окружности по теореме пифагора
	public static float GetCirleRadius(PointF center, PointF onCirle)
	{
		var dX = onCirle.X - center.X;
		var dY = onCirle.Y - center.Y;
		return Sqrt(dX * dX + dY * dY);
	}

	// Находит угол отклонения точки по её положению относительно центра окружности
	public static float FindAngleOfPointOnCircle(PointF target, PointF center)
	{
		int part = 0; // найдем четверть окружности
		float rx = target.X - center.X, ry = target.Y - center.Y; // relativeX, relativeY
		if(rx >= 0) part = ry >= 0 ? 1 : 4;
		if(rx < 0) part = ry >= 0 ? 2 : 3;

		/* Теперь вычислим изначальный и новый углы
		 * К углу относительно ближайшей оси добавить остальную часть
		 * (PI/2) - число радиан в четверти
		 */
		return (PI / 2) * (part - 1) + (part == 1 || part == 3 ?
			Atan(Abs(ry) / Abs(rx)) : Atan(Abs(rx) / Abs(ry)));
	}
	// Находит координаты точки окружности, отклоненной на соответствующий угл.
	public static PointF FindPointOnCircle(PointF center, float radius, float angleR)
	{
		angleR = angleR % (PI * 2); // 2 радиана = полный цикл
		if(angleR < 0) angleR = (PI * 2) + angleR; // с целью упрощения сведем все к положительным значениям

		int part = (int)(angleR / (PI * 2)); // чертверть окружности

		float X = radius * (part % 2 == 1 ? Cos(angleR) : Sin(angleR));
		float Y = radius * (part % 2 == 1 ? Sin(angleR) : Cos(angleR));

		return center + (part % 2 == 1 ? new PointF(X, Y) : new PointF(Y, X));
	}

	/* Реализация этого алгоритма не является очевидной...
	 * Имеем: 
	 *	Расстояние между target и relativeTo - константа. Она же радиус.
	 *	Положение задается уравнением r^2 = x^2 + y^2.
	 *	
	 * Поскольку изначально мы не знаем даже угла точки на окружности - сначала вычислим его.
	 * Потом добавим к нему angleR. Далее, определив четверть, сможем решить обычное уравнение.
	 */
	public static PointF RotatePoint(PointF target, PointF relativeTo, float angleR)
	{
		if(angleR == 0) return target; // поворот на ноль
		if(target.Equals(relativeTo)) return target; // поворот относительно себя самой не изменяет точку

		angleR = angleR % (PI * 2); // 2 радиана = полный цикл
		if(angleR < 0) angleR = (PI * 2) + angleR; // с целью упрощения сведем все к положительным значениям

		/* Теперь вычислим изначальный и новый углы
		 * К углу относительно ближайшей оси добавить остальную часть
		 * (PI/2) - число радиан в четверти
		 */
		var origAngle = FindAngleOfPointOnCircle(target, relativeTo);
		var newAngle = (origAngle + angleR) % (PI * 2);

		// сразу получим результат в абсолютных координатах
		return FindPointOnCircle(relativeTo, Common.GetCirleRadius(relativeTo, target), newAngle);
	}

	/* Задача масштабирования ялвяется, по-сути, подзадачей к вращению точки и де-факто уже была решена выше.
	 * Получим угол поворот относительно точки relativeTo, увеличим радиус в Scale раз, вернем новую точку...
	 */
	public static PointF ScalePoint(PointF target, PointF relativeTo, float scale)
	{
		var radius = GetCirleRadius(relativeTo, target);
		var angle = FindAngleOfPointOnCircle(target, relativeTo);
		return FindPointOnCircle(relativeTo, radius * scale, angle);
	}

	/* Задача отражения точки относительно другой точки с наличием вышеописанных функций является тривиальной
	 * и представляет собой ничто иное, как поворот точки на 180 градусов, а равно на 3.1415 радиан
	 * относительно центра окружности в точке relativeTo.
	 */
	public static PointF MirrorPoint(PointF target, PointF relativeTo)
	{
		return RotatePoint(target, relativeTo, PI);
	}

	/* Построим уравнение прямой, задаваемой отрезком из двух точек,
	 * формула для которого известна. Имеем каноническое уравнение (x-x1)/(x2-x1) == (y-y1)/(y2-y1).
	 * Пусть mx=x2-x1, my=y2-y1. Имеем (x-x1)/mx = (y-y1)/my. Домножим на my. 
	 * Имеем (x-x1)my/mx=y-y1. Перенесем. Имеем (x-x1)my/mx - y1 = y. Раскроем. Пусть k=my/mx.
	 * Имеем kx - kx1 -y1. Получим уравнение вида kx+b=y.
	 * Упростим вышесказанное до k=(y2-y1)/(x2-x1), b=(y-kx);
	 */
	public static (float k, float b) FindLinearEquation(PointF p1, PointF p2)
	{
		float
			x1 = p1.X, y1 = p1.Y,
			x2 = p2.X, y2 = p2.Y,
			k = (y2 - y1) / (x2 - x1),
			b = (p1.Y - k * p1.X);

		return (k, b);
	}
	public static (float k, float b) FindLinearEquation(LineF line)
		=> FindLinearEquation(line.Start, line.End);

	/* Находит уравнение прямой, проходяшей через точку p и имеющей угловой коэффициент k.
	 * y = k * (x-x1) + y1.
	 * y = kx - kx1 + y1
	 * -b = kx1 - y1
	 */
	public static (float k, float b) FindLinearEquation(float k, PointF p)
	{
		return (k, -(k * p.X - p.Y));
	}

	/* Задача отражения точки относительно линии является надзадачей к задаче отражения относительно точки.
	 * Понятно, что любое отражение является, в конечном итоге, отражением относительно точки, в данном случае
	 * нам нужно найти таковую точку на прямой, задаваемой отрезком relativeTo и отразить target относительно
	 * неё вызовом вышестоящего метода. Точка на прямой должна определяться перпендикуляром. Найти таковой
	 * перпендикуляр не очень сложно - сначала построим уравнение прямой, задаваемой отрезком из двух точек,
	 * формула для которого известна (см. выше). Теперь необходимо решить следующее уравнение - находящее точку
	 * на данной прямой y=kx+b, ближайшей к данной точке. Из аналитической геометрии мы имеем возможность найти
	 * уравнение прямой, перпендикулярой данной и проходящую через заданную точку target. Поскольку мы уже знаем
	 * угловой коэффициент прямой relativeTo, мы, де-факто, можем сразу же взять таковой коэффициент для искомой
	 * прямой, в виде -1/k. Следовательно мы можем записать уравнение искомой перпендикулярной прямой, проходящей
	 * через точку target(x1,y1) в виде y-y1 = -1/k * (x-x1), а равно  y = -1/k * (x-x1) + y1.
	 * Теперь остается решить уравнение пересечения. Имеем систему kx+b-y=0, kNx+bN-y=0. Вычтем уравнения.
	 * Имеем (k-kN)x + (b-bN) = 0; Имеем X = -(b-bN)/(k-kN). Далее находим Y подставновкой. Таким образом получим
	 * точку отражения данной точки относительно данной прямой.
	 * 
	 * Отметим, что в данном случае имеется вырожденный случай, если relativeTo представляет собой вертикальную
	 * линию - для неё невозможно построить уравнение. В этом случае k и b будут представлять собой бесконечности.
	 * Этот случай следует рассмотреть отдельно. По сути, в данном случае необходимо просто переместить точку в
	 * сторону прямой на 2*(разницу по Х).
	 */
	public static PointF MirrorPoint(PointF target, LineF relativeTo)
	{
		if(relativeTo.Start.X == relativeTo.End.X) {
			return MirrorPoint(target, new PointF(relativeTo.Start.X, target.Y));
		}

		var (k, b) = FindLinearEquation(relativeTo);

		var (kNormal, bNormal) = FindLinearEquation(-1 / k, target);

		var xMirror = -(b - bNormal) / (k - kNormal);
		var yMirror = kNormal * xMirror + bNormal; // y = kx + b

		return MirrorPoint(target, new PointF(xMirror, yMirror));
	}


	/* Задача нахождения периметра для произвольного контура является тривиальной и состоит не более, чем в суммировании
	 * длинн отрезков. Длинна отрезка может быть получена уже описанным выше методом получения радиуса окружности, если
	 * представить, что начало отрезка - центр окружности, а конец - точка на ней.
	 */
	public static float FindPerimeter(IEnumerable<LineF> lines)
	{
		return lines.Sum(line => GetCirleRadius(line.Start, line.End));
	}
	private static IEnumerable<PointF> LinesToPoints(IEnumerable<LineF> lines)
	{
		foreach(var line in lines) {
			yield return line.Start;
			yield return line.End;
		}
	}
	/* Задача нахождения площади замкнутого контура. Применим метод трапеций.
	 * https://algolist.manual.ru/maths/geom/polygon/area.php
	 */
	public static float FindArea(IEnumerable<LineF> lines)
	{
		var points = lines.Select(x => x.End).ToArray();

		float sum = 0;
		for(int i = 1; i < points.Length; i++) {
			var pC = points[i];
			var pP = points[i - 1];

			sum += (pC.X + pP.X) * (pC.Y - pP.Y); // все кроме первой
		}

		var pF = points[0];
		var pL = points[points.Length - 1];
		sum += (pL.X + pF.X) * (pL.Y - pF.Y); // включая первую

		return Abs(sum) / 2;
	}



	// ru.wikipedia.org/wiki/Интерполяционный_многочлен_Лагранжа
	//private static float LagrangeFind(IList<PointF> points)
	//{
	//	var multiplicatinos = new List<List<LineEquation>>(points.Count);

	//	for(int j = 0; j < points.Count; j++) {
	//		for(int i = 0; i < points.Count; i++) {
	//			if(i == j) continue;
	//			if(multiplicatinos[j] is null) multiplicatinos[j] = new(points.Count - 1);

	//			/* li(x) = (x-xj)/(xi-xj) = x/(xi-xj) - xj/(xi-xj)
	//			 * k = 1/(xi-xj), b = -xj/(xi-xj)
	//			 */
	//			float xi = points[i].X, xj = points[j].X;
	//			multiplicatinos[j].Add(new LineEquation {
	//				K = 1 / (xi - xj),
	//				B = -xj / (xi - xj)
	//			}); // получим базисный полином l i-тый от х
	//		}
	//	}
	//}
	/* Для задачи генерации кривой безье через заданные точки существует т.н. интерполяционный многочлен Лагранжа
	 * Многочлен Лагранжа представляет собой функцию, что если (x1...xk; y1...yk) - базовые точки, т.е. точки,
	 * через которые должна проходить кривая безье, то L(xk) = yk, при этом в остальных точках значение соответствует
	 * искомой кривой.
	 */
	public static IEnumerator<PointF> WithControlPoints(IEnumerable<PointF> originalPoints)
	{
		//var enumer = originalPoints.GetEnumerator();

		//if(!enumer.MoveNext()) yield break;
		//PointF prev = enumer.Current;
		//yield return prev;

		//if(!enumer.MoveNext()) yield break;
		//PointF start = enumer.Current;
		//yield return prev;

		//PointF end;
		//float dX, dY;
		//while(enumer.MoveNext()) {
		//	end = enumer.Current;

		//	/*                                #
		//	 *         #
		//	 *  #
		//	 */
		//	// предыдущая точка нижу начальной
		//	if(prev.Y < start.Y) {
		//		dY = (start.Y) + (start.Y - end.Y)
		//	}


		//	dX = (curr.X - prev.X) / 2;
		//	dY = (curr.Y - prev.Y) / 2;

		//	yield return new PointF(dX, dY);
		//	yield return curr;
		//	prev = curr;
		//}

		throw new NotImplementedException();
	}

}
