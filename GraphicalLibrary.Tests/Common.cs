using System.Drawing;
using System.Windows.Media;
using GraphicLibrary;
using GraphicLibrary.Models;
using Xunit;

using static System.MathF;

namespace GraphicLibraryTests;

public class CommonTests
{
	#region Rotate
	private static Point rel = new Point(0, 0);

	private static PointF Round2(PointF point)
	{
		return new PointF(Round(point.X, 2), Round(point.Y, 2));
	}
	private static PointF RoundP(PointF point)
	{
		return new PointF(Round(point.X), Round(point.Y));
	}

	[Fact]
	public void CanRotate0()
	{
		var p1 = new Point(10, 10);
		var angle = 0;

		var expected = new Point(10, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void CanRotate45()
	{
		int x = 10, y = 10;
		var p1 = new Point((int)Round(Sqrt(x*x+y*y)),0);
		var angle = PI/4;

		var expected = new Point(10, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, RoundP(actual));
	}

	[Fact]
	public void CanRotate90()
	{
		var p1 = new Point(10, 0);
		var angle = PI/2;

		var expected = new Point(0, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, Round2(actual));
	}

	[Fact]
	public void CanRotate180()
	{
		var p1 = new PointF(10, 10);
		var angle = PI; // 180 deg

		var expected = new Point(-10, -10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, Round2(actual));
	}

	[Fact]
	public void CanRotate180_2()
	{
		var p1 = new Point(-5, -10); // part 3
		var angle = PI; // 180 deg

		var expected = new Point(5, 10); // part 1
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, Round2(actual));
	}


	[Fact]
	public void CanRotate359()
	{
		var p1 = new Point(10, 10);
		var angle = PI*2 - 1/1000; // almost 360

		var expected = new Point(10, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, actual);
	}

	[Fact]
	public void CanRotate_1()
	{
		var p1 = new Point { X = 551, Y = 230 };
		var rel = new Point { X = 600, Y = 250 };

		var angle = PI / 2; // 90 deg	
		var actual = Common.RotatePoint(p1, rel, angle);

		var relX = p1.X - rel.X;
		var relY = p1.Y - rel.Y;
		var expected = new Point(rel.X-relY, rel.Y + relX);

		Assert.Equal(expected, new Point(600 + 20, 250 - 49));
		Assert.Equal(expected, Round2(actual));
	}


	[Fact]
	public void CanRotateLine()
	{
		var p1 = new Point(-5, -10);
		var p2 = new Point(-10, -5);
		Line line = new(p1, p2,  System.Drawing.Color.White);


		line.Rotate(PI, rel);

		var p1e = new Point(5, 10);
		var p2e = new Point(10, 5);

		//Assert.Equal(p1e, line.Start);
		Assert.Equal(Round2(p2e), Round2(line.End));
	}


	// Горизонтальную линию в вертикальную
	[Fact]
	public void CanRotateLine_2()
	{
		var p1 = new PointF(-1000, 1000);
		var p2 = new PointF(1000, 1000);

		Line line = new(p1, p2, System.Drawing.Color.White);

		float angleD = 90;
		float angleR = angleD * float.Pi / 180;

		line.Rotate(angleR, rel);

		var p1e = new PointF(-1000, -1000);
		var p2e = new PointF(-1000, 1000);

		Assert.Equal(p1e, Round2(line.Start));
		Assert.Equal(p2e, Round2(line.End));
	}


	// Горизонтальную линию в вертикальную c относительной точкой
	[Fact]
	public void CanRotateLine_3()
	{
		var p1 = new PointF(-1000, 2000);
		var p2 = new PointF(1000, 2000);
		var rel = new PointF(0, 1000);

		Line line = new(p1, p2, System.Drawing.Color.White);

		float angleD = 90;
		float angleR = angleD * float.Pi / 180;

		line.Rotate(angleR, rel);

		var p1e = new PointF(-1000, 0);
		var p2e = new PointF(-1000, 2000);

		Assert.Equal(Round2(p1e), Round2(line.Start));
		Assert.Equal(Round2(p2e), Round2(line.End));

		line.Rotate(angleR, rel);
		line.Rotate(angleR, rel);
		line.Rotate(angleR, rel);

		Assert.Equal(Round2(p1), Round2(line.Start));
		Assert.Equal(Round2(p2), Round2(line.End));
	}
	#endregion

	[Fact]
	public void CanMove()
	{
		Line lineActual = new(new Point(10, 10), new Point(20, 20), System.Drawing.Color.White);
		lineActual.MoveCoordinates(10, 5);

		Line lineExpected = new(new Point(20, 15), new Point(30, 25), System.Drawing.Color.White);

		Assert.Equal(lineExpected.Start, lineActual.Start); 
		Assert.Equal(lineExpected.End, lineActual.End);
	} 
}