namespace GraphicLibrary.MathModels;
public class LineF : IEquatable<LineF>, IEquatable<PointF>
{
	public PointF Start;
	public PointF End;

	public PointF Left => Start.X < End.X ? Start : End;
	public PointF Right => Start.X > End.X ? Start : End;

	public LineF(PointF start, PointF end)
	{
		Start = start;
		End = end;
	}
	public LineF(System.Drawing.PointF start, System.Drawing.PointF end)
	{
		Start = start;
		End = end;
	}
	public LineF(System.Windows.Point start, System.Windows.Point end)
	{
		Start = start;
		End = end;
	}

	public bool Equals(LineF other)
	{
		return Start.Equals(other.Start)
			&& End.Equals(other.End);
	}

	public bool Equals(PointF other)
	{
		return Start.Equals(other)
			&& End.Equals(other);
	}
}
