using GraphicLibrary.MathModels;
using static System.MathF;

namespace GraphicLibrary;
public static class IListExtensions
{
	// Обеспечивает периодичность для заданного индекса
	private static int GetPositiveIndex(int index, int fromTotal)
	{
		index %= fromTotal;

		return index >= 0 ? index : fromTotal + index;
	}
	public static T At<T>(this IList<T> list, int index)
	{
		return list[GetPositiveIndex(index, list.Count)];
	}
}

public static class Besie
{
	public static List<PointF> Calculate(IList<PointF> basePoints, int stepsPerSpline, float bendingFactor, bool withFirstSplineCorrection = true)
	{
		if(basePoints is null) {
			throw new ArgumentNullException(nameof(basePoints));
		}

		if(basePoints.Count < 3) {
			throw new ArgumentException("Points counts must be 3 or more.");
		}

		var between = PointsBetween(basePoints, bendingFactor, withFirstSplineCorrection);

		var result = new List<PointF>((basePoints.Count + 1) * stepsPerSpline);

		PointF p0, p1, p2;
		var stepT = 1f / stepsPerSpline;
		for(var i = 0; i < basePoints.Count; i++) {
			p0 = basePoints.At(i);
			p1 = between.At(i);
			p2 = basePoints.At(i + 1);

			for(float t = 0; t <= 1; t += stepT) {
				result.Add(
					(Pow(1 - t, 2) * p0) +
					(2 * t * (1 - t) * p1) +
					(Pow(t, 2) * p2));
			}
		}

		return result;
	}

	/* Для построения сплайна Безье 2-го порядка нужно добавить по 1 точке между каждой парой,
	 * таким образом эта точка станет опорной, но проходить через неё сплайн не будет.
	 * Опорная точка строится с учетом пары точек (curr, next) + одной предшествующей (prev).
	 * Точка должна продолжать прямую prev -> curr1 с некоторым смещением в сторону next (не реализовано).
	 */
	private static List<PointF> PointsBetween(IList<PointF> basePoints, float bendingFactor, bool withFirstSplineCorrection = true)
	{
		// коэфициент того, насколько отрезок prev->curr1 влияет на получаемую точку.
		var continuation = bendingFactor;

		var result = new List<PointF>(basePoints.Count);

		var prev = basePoints.At(0 - 1);
		var curr = basePoints.At(0 + 0);
		_ = basePoints.At(0 + 1);
		var d = continuation * (curr - prev);
		result.Add(curr + d);

		for(var i = 1; i < basePoints.Count; i++) {
			prev = result.At(-1);
			curr = basePoints.At(i);
			_ = basePoints.At(i + 1);

			d = continuation * (curr - prev);

			/* Данная байда должна учитывать влияние следующей точки...
			 * Но работает она рандомно, поэтому нахер её. <<И так сойдет...>>
			 */
			//var dN = (next - prev);
			//while(Abs(dN.X) > Abs(d.X)) {
			//	dN.X /= 2;
			//}
			//while(Abs(dN.Y) > Abs(d.Y)) {
			//	dN.Y /= 2;
			//}
			//d += dN;

			result.Add(curr + d);
		}

		// Теперь скорректруем первый сплайн
		if(withFirstSplineCorrection) {
			prev = result.At(0 - 1);
			curr = basePoints.At(0 + 0);
			_ = basePoints.At(0 + 1);
			d = continuation * (curr - prev);
			result[0] = curr + d;
		}

		return result;
	}
}
