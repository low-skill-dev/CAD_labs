using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.MathModels;
public struct LagrangeSumPart
{
	public float Xi,Xj, Yi;

	public float Calc(float x)
	{
		return
			(x - Xj)
			/
			(Xi - Xj);
	}

	public LagrangeSumPart(float xi,float xj, float yi)
	{
		this.Xi = xi;
		this.Xj = xj;
		this.Yi = yi;
	}
}
