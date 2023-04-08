using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public class LagrangePolynomial:ALinearElement
{
	public IList<PointF> Points { get; private set; }
	public float LineStep { get; set; }
	public LagrangePolynomial(IList<PointF> points, float linearization, Color color, IEnumerator<bool>? patternResolver = null)
		:base(color,patternResolver)
	{
		this.Points = points;
		LineStep = linearization;
	}

	/* Для задачи генерации кривой безье через заданные точки существует т.н. интерполяционный многочлен Лагранжа
	 * Многочлен Лагранжа представляет собой функцию такую, что если (x1...xk; y1...yk) - базовые точки, т.е. точки,
	 * через которые должна проходить кривая безье, то L(xk) = yk, при этом в остальных точках значение соответствует
	 * искомой кривой.
	 * L(x) = y(x) = sum(i=0...n-1) { yi*li(x)) }
	 * li = product(i=0...n-1, j=0...n-1, i!=j) { (x-xj)/(xi-xj) }
	 * ru.wikipedia.org/wiki/Интерполяционный_многочлен_Лагранжа
	 */
	public float CalcY(float x)
	{
		float prod, sum = 0;
		for(int i = 0; i < Points.Count; i++) {
			prod = 1;
			for(int j = 0; j < Points.Count; j++) {
				if(i != j) {
					var xi = Points[i].X;
					var xj = Points[j].X; 
					prod *=
						(x - xj)
						/
						(xi - xj);
				}
			}
			var yi = Points[i].Y;
			sum += yi * prod;
		}
		return sum;
	}

	
	public override void Move(float dX, float dY)
	{
		throw new NotImplementedException();
	}

	public override void Rotate(float angleR, PointF relativeTo)
	{
		throw new NotImplementedException();
	}

	public override void Scale(float scale, PointF relativeTo)
	{
		throw new NotImplementedException();
	}

	public override void Mirror(PointF relativeTo)
	{
		throw new NotImplementedException();
	}

	public override void Mirror(LineF relativeTo)
	{
		throw new NotImplementedException();
	}

	public override IGraphicalElement Clone()
	{
		throw new NotImplementedException();
	}
}
