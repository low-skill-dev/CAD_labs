using GraphicLibrary;
using GraphicLibrary.MathModels;
using GraphicLibrary.Models;
using System.Drawing;
using Xunit;
using Xunit.Abstractions;
using static System.MathF;
using Color = System.Drawing.Color;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibraryTests;

public class CommonTests
{
	private static IEnumerable<T> IncapsulateEnumerator<T>(IEnumerator<T> enumer)
	{
		while(enumer.MoveNext()) {
			yield return enumer.Current;
		}
	}


	private readonly ITestOutputHelper _output;
	public CommonTests(ITestOutputHelper output)
	{
		_output = output;
	}


	#region Rotate
	private static Point rel = new(0, 0);

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
		var p1 = new Point((int)Round(Sqrt((x * x) + (y * y))), 0);
		var angle = PI / 4;

		var expected = new Point(10, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, RoundP((System.Drawing.Point)actual));
	}

	[Fact]
	public void CanRotate90()
	{
		var p1 = new Point(10, 0);
		var angle = PI / 2;

		var expected = new Point(0, 10);
		var actual = Common.RotatePoint(p1, rel, angle);

		Assert.Equal(expected, Round2((System.Drawing.Point)actual));
	}

	[Fact]
	public void CanRotate180()
	{
		var p1 = new System.Drawing.PointF(10, 10);
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
		var angle = (PI * 2) - (1 / 1000); // almost 360

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
		var expected = new Point(rel.X - relY, rel.Y + relX);

		Assert.Equal(expected, new Point(600 + 20, 250 - 49));
		Assert.Equal(expected, Round2(actual));
	}


	[Fact]
	public void CanRotateLine()
	{
		var p1 = new Point(-5, -10);
		var p2 = new Point(-10, -5);
		Line line = new(p1, p2, System.Drawing.Color.White);


		line.Rotate(PI, rel);
		_ = new Point(5, 10);
		var p2e = new Point(10, 5);

		//Assert.Equal(p1e, line.Start);
		Assert.Equal(Round2(p2e), Round2(line.End));
	}


	// Горизонтальную линию в вертикальную
	[Fact]
	public void CanRotateLine_2()
	{
		var p1 = new System.Drawing.PointF(-1000, 1000);
		var p2 = new System.Drawing.PointF(1000, 1000);

		Line line = new(p1, p2, System.Drawing.Color.White);

		float angleD = 90;
		var angleR = angleD * float.Pi / 180;

		line.Rotate(angleR, rel);

		var p1e = new System.Drawing.PointF(-1000, -1000);
		var p2e = new System.Drawing.PointF(-1000, 1000);

		Assert.Equal(p1e, Round2(line.Start));
		Assert.Equal(p2e, Round2(line.End));
	}


	// Горизонтальную линию в вертикальную c относительной точкой
	[Fact]
	public void CanRotateLine_3()
	{
		var p1 = new System.Drawing.PointF(-1000, 2000);
		var p2 = new System.Drawing.PointF(1000, 2000);
		var rel = new System.Drawing.PointF(0, 1000);

		Line line = new(p1, p2, System.Drawing.Color.White);

		float angleD = 90;
		var angleR = angleD * float.Pi / 180;

		line.Rotate(angleR, rel);

		var p1e = new System.Drawing.PointF(-1000, 0);
		var p2e = new System.Drawing.PointF(-1000, 2000);

		Assert.Equal(Round2(p1e), Round2(line.Start));
		Assert.Equal(Round2(p2e), Round2(line.End));

		line.Rotate(angleR, rel);
		line.Rotate(angleR, rel);
		line.Rotate(angleR, rel);

		Assert.Equal(Round2(p1), Round2(line.Start));
		Assert.Equal(Round2(p2), Round2(line.End));
	}
	#endregion

	#region Move
	[Fact]
	public void CanMove()
	{
		Line lineActual = new(new Point(10, 10), new Point(20, 20), System.Drawing.Color.White);
		lineActual.Move(10, 5);

		Line lineExpected = new(new Point(20, 15), new Point(30, 25), System.Drawing.Color.White);

		Assert.Equal(lineExpected.Start, lineActual.Start);
		Assert.Equal(lineExpected.End, lineActual.End);
	}
	#endregion

	#region FindEquation
	[Fact]
	public void CanFindEquation_1()
	{
		var p1 = new System.Drawing.PointF(1, 1);
		var p2 = new System.Drawing.PointF(PI, PI);
		var (k, b) = Common.FindLinearEquation(p1, p2);

		Assert.Equal(1, (double)k, 3);
		Assert.Equal(0, (double)b, 3);


		p1 = new System.Drawing.PointF(1, 1);
		p2 = new System.Drawing.PointF(-PI, -PI);
		(k, b) = Common.FindLinearEquation(p1, p2);

		Assert.Equal(1, (double)k, 3);
		Assert.Equal(0, (double)b, 3);
	}
	[Fact]
	public void CanFindEquation_2()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var p2 = new System.Drawing.PointF(2 * PI, PI);
		var (k, b) = Common.FindLinearEquation(p1, p2);

		Assert.Equal((double)0.5, (double)k, 3);
		Assert.Equal(0, (double)b, 3);
	}

	[Fact]
	public void CanFindEquation_3()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var p2 = new System.Drawing.PointF(PI, 0); // horizontal line
		var (k, b) = Common.FindLinearEquation(p1, p2);

		Assert.Equal(0, (double)k, 3);
		Assert.Equal(0, (double)b, 3);


		p1 = new System.Drawing.PointF(0, PI);
		p2 = new System.Drawing.PointF(PI, PI); // horizontal line
		(k, b) = Common.FindLinearEquation(p1, p2);

		Assert.Equal(0, (double)k, 3);
		Assert.Equal(PI, (double)b, 3);
	}


	[Fact]
	public void CanFindEquation_4()
	{
		var p1 = new System.Drawing.PointF(PI, 0);
		var p2 = new System.Drawing.PointF(PI, 10); // vertical line
		var (k, b) = Common.FindLinearEquation(p1, p2);

		Assert.True(float.IsInfinity(k));
		Assert.True(float.IsInfinity(b));

		p1 = new System.Drawing.PointF(PI, 10);
		p2 = new System.Drawing.PointF(PI, 0); // vertical line
		(k, b) = Common.FindLinearEquation(p1, p2);

		Assert.True(float.IsInfinity(k));
		Assert.True(float.IsInfinity(b));
	}
	#endregion

	#region MirrorPoint
	[Fact]
	public static void CanMirrorPoint_1()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new System.Drawing.PointF(10, 10);
		var expected = new System.Drawing.PointF(20, 20);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}


	[Fact]
	public static void CanMirrorPoint_2()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new System.Drawing.PointF(10, 5);
		var expected = new System.Drawing.PointF(20, 10);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}

	[Fact]
	public static void CanMirrorPoint_3()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new System.Drawing.PointF(0, 0);
		var expected = new System.Drawing.PointF(0, 0);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}

	[Fact]
	public static void CanMirrorPoint_4()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new System.Drawing.PointF(10, 0);
		var expected = new System.Drawing.PointF(20, 0);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}

	[Fact]
	public static void CanMirrorPoint_5()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new System.Drawing.PointF(0, 10);
		var expected = new System.Drawing.PointF(0, 20);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}


	[Fact]
	public static void CanMirrorPoint_6()
	{
		var p1 = new System.Drawing.PointF(-200, -100);
		var rel = new System.Drawing.PointF(200, 100);
		var expected = new System.Drawing.PointF(600, 300);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}

	[Fact]
	public static void CanMirrorPointByLine_1()
	{
		var p1 = new System.Drawing.PointF(0, 0);
		var rel = new LineF(new PointF(10, -10), new PointF(10, 10));
		var expected = new System.Drawing.PointF(20, 0);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}
	[Fact]
	public static void CanMirrorPointByLine_2()
	{
		var p1 = new PointF(0, -1000);
		var rel = new LineF(new PointF(10, -10), new(10, 10));
		var expected = new PointF(20, -1000);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}

	[Fact]
	public static void CanMirrorPointByLine_3()
	{
		var p1 = new PointF(-10, -10);
		var rel = new LineF(new PointF(-1, 1), new(0, 0)); // 45 deg line
		var expected = new System.Drawing.PointF(10, 10);
		var actual = Common.MirrorPoint(p1, rel);

		Assert.Equal(Round2(expected), Round2(actual));
	}
	#endregion

	#region Besie Points
	[Fact]
	public void CanCreate()
	{

		var original = new PointF[] {
			new(1,1), new(5,5), new(7,7), new(20,20) // y=x line
		};

		var polim = new GraphicLibrary.Models.InterpolatedPoints(original, 0, Color.Black);

		var Xs = Enumerable.Range(0, 100);
		var recreated = Xs.Select(x => new System.Drawing.PointF(x, polim.CalcLagrangeY(x)));

		Assert.All(recreated, x => Round(x.X, 3).Equals(Round(x.Y, 3)));
	}

	[Fact]
	public void CanCalc()
	{
		var original = new PointF[] {
			new(10,10), new(15,12), new(20,20) // sqre part
		};

		var polim = new GraphicLibrary.Models.InterpolatedPoints(original, 0, Color.Black);

		for(float i = 5; i < 25; i++) {
			var val = polim.CalcLagrangeY(i);
			_output.WriteLine(val.ToString("0"));
		}
	}
	#endregion

	#region FindCenterAndRadius
	[Fact]
	public void CanBuildCircle()
	{
		var original = new PointF[] {
			Common.FindPointOnCircle(new(0,0),1,-PI/6), new(1,0), new(0,1)
		};

		var actual = Common.FindCenter(original[0], original[1], original[2]);

		Assert.True(actual.Equals(new PointF(0, 0)));
	}
	#endregion

	#region canFindAngle
	private static readonly PointF center = new(0, 0);

	[Fact]
	public void CanFindAngle1()
	{
		PointF point;
		float angle;

		point = new PointF(10, 0);
		angle = Common.FindAngleOfPointOnCircle(point, center);
		Assert.Equal(0, angle);

		point = new PointF(0, 10);
		angle = Common.FindAngleOfPointOnCircle(point, center);
		Assert.Equal(PI / 2, angle);

		point = new PointF(-10, 0);
		angle = Common.FindAngleOfPointOnCircle(point, center);
		Assert.Equal(PI, angle);

		point = new PointF(0, -10);
		angle = Common.FindAngleOfPointOnCircle(point, center);
		Assert.Equal(PI + (PI / 2), angle);


		float r = 10;
		var diag = Common.GetCirleRadius(center, point);
		Assert.Equal(r, diag);

		var x = Sqrt(6f);
		var y = Sqrt(4f);

		point = new PointF(x, y);
		angle = Common.FindAngleOfPointOnCircle(point, center);

		Assert.True(angle > 0 && angle < (PI / 2));
	}
	#endregion
}