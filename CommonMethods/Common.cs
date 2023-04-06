using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Linq;
using System.IO;
using GraphicLibrary.Models;
using static System.MathF;
using System.Security.Cryptography.X509Certificates;

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

	// System.Windows.Point -> System.Drawing.PointF
	public static PointF WindowsToDrawing(System.Windows.Point p)
		=> new System.Drawing.PointF((float)p.X, (float)p.Y);

	// радиус окружности по теореме пифагора
	public static float GetCirleRadius(PointF center, PointF onCirle)
	{
		var dX = onCirle.X - center.X;
		var dY = onCirle.Y - center.Y;
		return Sqrt(dX * dX + dY * dY);
	}

	// Находит угол отклонения точки по её положению относительно центра окружности
	public static float FindAngleOfPointOnCircle(System.Drawing.PointF target, System.Drawing.PointF relativeTo)
	{
		int part = 0; // найдем четверть окружности
		float rx = target.X - relativeTo.X, ry = target.Y - relativeTo.Y; // relativeX, relativeY
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

		return center + (part % 2 == 1 ? new SizeF(X, Y) : new SizeF(Y, X));
	}

	/* Реализация этого алгоритма не является очевидной...
	 * Имеем: 
	 *	Расстояние между target и relativeTo - константа. Она же радиус.
	 *	Положение задается уравнением r^2 = x^2 + y^2.
	 *	
	 * Поскольку изначально мы не знаем даже угла точки на окружности - сначала вычислим его.
	 * Потом добавим к нему angleR. Далее, определив четверть, сможем решить обычное уравнение.
	 */
	public static PointF RotatePoint(System.Drawing.PointF target, System.Drawing.PointF relativeTo, float angleR)
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
	public static PointF ScalePoint(System.Drawing.PointF target, System.Drawing.PointF relativeTo, float scale)
	{
		var radius = GetCirleRadius(relativeTo, target);
		var angle = FindAngleOfPointOnCircle(target, relativeTo);
		return FindPointOnCircle(relativeTo, radius * scale, angle);
	}
}
