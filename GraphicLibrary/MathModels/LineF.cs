using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = GraphicLibrary.MathModels.PointF;

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
		return this.Start.Equals(other.Start)
			&& this.End.Equals(other.End);
	}

	public bool Equals(PointF other)
	{
		return this.Start.Equals(other)
			&& this.End.Equals(other);
	}
}
