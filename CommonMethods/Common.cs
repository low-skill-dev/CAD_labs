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

	public static Point WindowsToDrawing(System.Windows.Point p)
		=> new System.Drawing.Point((int)p.X, (int)p.Y);

	/* Реализация этого алгоритма не является очевидной...
	 * Имеем: 
	 *	Расстояние между target и relativeTo - константа. Она же радиус.
	 *	Положение задается уравнением r^2 = x^2 + y^2.
	 *	
	 * Поскольку изначально мы не знаем даже угла точки на окружности - сначала вычислим его.
	 * Потом добавим к нему angleR. Далее, определив четверть, сможем решить обычное уравнение.
	 *	
	 */
	public static Point RotatePoint(System.Drawing.Point target, System.Drawing.Point relativeTo, float angleR)
	{
		if(angleR == 0) return target;
		if(target.Equals(relativeTo)) return target;


		angleR = angleR % (PI * 2); // 2 радиана = полный цикл
		if(angleR < 0) angleR = (PI * 2) + angleR; // с целью упрощения сведем все к положительным значениям

		// найдем четверть окружности
		int part = 0;
		Point rLoc = target - (Size)relativeTo; // relative Location
		int rx = rLoc.X, ry = rLoc.Y;
		if(rx >= 0 && ry >= 0) part = 1;
		else if(rx <= 0 && ry >= 0) part = 2;
		else if(rx <= 0 && ry <= 0) part = 3;
		else part = 4;

		// теперь вычислим изначальный угл
		float absX = Abs(rx), absY = Abs(ry);
		// К углу относительно ближайшей оси добавить остальную часть
		// (PI/2) - число радиан в четверти
		var origAngle = (PI / 2) * (part - 1) + (
			part == 1 || part == 3 ? Atan(absY / absX) : Atan(absX / absY));

		var newAngle = (origAngle + angleR) % (PI * 2);

		var absoluteResult = FindPointOnCircle(relativeTo, Circle.GetCirleRadius(relativeTo, target), newAngle);

		var result = new Point((int)Math.Round(absoluteResult.X), (int)Math.Round(absoluteResult.Y));
		return result;
	}

	public static PointF FindPointOnCircle(PointF center, float radius, float angleR)
	{
		angleR = angleR % (PI * 2); // 2 радиана = полный цикл
		if(angleR < 0) angleR = (PI * 2) + angleR; // с целью упрощения сведем все к положительным значениям

		int part = (int)(angleR / (PI * 2));

		float X = radius * (part % 2 == 1 ? Cos(angleR) : Sin(angleR));
		float Y = radius * (part % 2 == 1 ? Sin(angleR) : Cos(angleR));

		return center + (part % 2 == 1 ? new SizeF(X, Y) : new SizeF(Y, X));
	}
}
