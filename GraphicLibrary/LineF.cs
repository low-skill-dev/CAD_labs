using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary;
public struct LineF
{
    public PointF Start;
    public PointF End;

    public LineF(PointF start, PointF end)
    {
        Start = start;
        End = end;
    }
}
