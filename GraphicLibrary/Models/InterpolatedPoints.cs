using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public class InterpolatedPoints: ALinearElement
{
	public IList<PointF> Points { get; private set; }
	public float StepsBetweenPoints { get; set; }
	public int Degree { get; set; } = 2;
	public float BendingFactor { get; set; } = 0.5f;
	public bool FirstSplineCorrection { get; set; } = true;
	public bool DebugDraw { get; set; } =
#if DEBUG
		true
#else
		false
#endif
		;

	public InterpolatedPoints(IList<PointF> points, float stepsBetweenPoints, Color color, IEnumerator<bool>? patternResolver = null)
		:base(color,patternResolver)
	{
		this.Points = points;
		this.StepsBetweenPoints = stepsBetweenPoints;
	}

	/* Для задачи генерации кривой безье через заданные точки существует т.н. интерполяционный многочлен Лагранжа
	 * Многочлен Лагранжа представляет собой функцию такую, что если (x1...xk; y1...yk) - базовые точки, т.е. точки,
	 * через которые должна проходить кривая безье, то L(xk) = yk, при этом в остальных точках значение соответствует
	 * искомой кривой.
	 * L(x) = y(x) = sum(i=0...n-1) { yi*li(x)) }
	 * li = product(i=0...n-1, j=0...n-1, i!=j) { (x-xj)/(xi-xj) }
	 * ru.wikipedia.org/wiki/Интерполяционный_многочлен_Лагранжа
	 */
	public float CalcLagrangeY(float x)
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
		var d = new PointF(dX, dY);
		for(int i = 0;i < Points.Count;i++) {
			Points[i] += d;
		}
	}
	public override void Rotate(float angleR, MathModels.PointF relativeTo)
	{
		for(int i = 0; i < Points.Count; i++) {
			Points[i] = Common.RotatePoint(Points[i],relativeTo,angleR);
		}
	}
	public override void Scale(float scale, MathModels.PointF relativeTo)
	{
		for(int i = 0; i < Points.Count; i++) {
			Points[i] = Common.ScalePoint(Points[i], relativeTo, scale);
		}
	}
	public override void Mirror(MathModels.PointF relativeTo)
	{
		for(int i = 0; i < Points.Count; i++) {
			Points[i] = Common.MirrorPoint(Points[i], relativeTo);
		}
	}
	public override void Mirror(MathModels.LineF relativeTo)
	{
		for(int i = 0; i < Points.Count; i++) {
			Points[i] = Common.MirrorPoint(Points[i], relativeTo);
		}
	}
	public override InterpolatedPoints Clone()
	{
		var clone = new PointF[Points.Count]; Points.CopyTo(clone, 0);
		return new InterpolatedPoints(clone, this.StepsBetweenPoints, this.Color, this.PatternResolver);
	}
}
