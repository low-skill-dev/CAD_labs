using static System.MathF;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary;


// https://habr.com/ru/articles/309210/
public static class Spline
{
	// Обеспечивает периодичность для заданного индекса
	private static int GetPositiveIndex(int index, int fromTotal)
	{
		index %= fromTotal;

		return index >= 0 ? index : fromTotal + index;
	}

	/// <summary>
	/// Формирует точки, обеспечивающие создание сплайна, проходящего через коллекцию точек orig.<br/>
	/// Де-факто, осуществляет преобразование Фурье для сигнала периодиностью len(orig).<br/>
	/// http://dha.spb.ru/PDF/discreteSplines.pdf#page=6
	/// </summary>
	/// <param name="orig">Точки, через который должен проходить искомый сплайн.</param>
	/// <param name="degree">Порядок искомого сплайна.</param>
	/// <param name="stepsPerSpline">Число линеаризирующих точек между полюсами.</param>
	/// <returns>Координаты полюсов для построения искомого сплайна.</returns>
	private static PointF[] RecalculateVectors(PointF[] orig, int degree, int stepsPerSpline)
	{
		var m = orig.Length;
		var N = stepsPerSpline * m; // Всего вершин.

		// Вычисляем знаменатель.
		var tr = new float[m];
		tr[0] = 1;
		for(var k = 1; k < m; k++) {
			for(var q = 0; q < stepsPerSpline; q++) {
				tr[k] += Pow(2 * stepsPerSpline * Sin(PI * ((q * m) + k) / N), -2 * degree);
			}
			tr[k] *= Pow(2 * Sin(PI * k / m), 2 * degree);
		}

		// Вычисляем числитель.
		var zre = new PointF[m];
		var zim = new PointF[m];
		for(var j = 0; j < m; j++) {
			zre[j] = new PointF(0, 0);
			zim[j] = new PointF(0, 0);
			for(var k = 0; k < m; k++) {
				zre[j] += orig[k] * Cos(-2 * PI * j * k / m);
				zim[j] += orig[k] * Sin(-2 * PI * j * k / m);
			}
		}

		// Считаем результат.
		var result = new PointF[m];
		for(var p = 0; p < m; p++) {
			result[p] = new PointF(0, 0);
			for(var k = 0; k < m; k++) {
				var d = (zre[k] * Cos(2 * PI * k * p / m)) - (zim[k] * Sin(2 * PI * k * p / m));
				d *= 1f / tr[k];
				result[p] += d;
			}
			result[p] /= m;
		}

		return result;
	}

	/// <summary>
	/// Вычисляет коэфициенты дискретного периодического Q-сплайна 1-ого порядка.<br/>
	/// http://www.math.spbu.ru/ru/mmeh/AspDok/pub/2010/Chashnikov.pdf#page=6
	/// </summary>
	/// <param name="stepsBetweenBasePoints">Число узлов между полюсами.</param>
	/// <param name="basePointsCount">Число полюсов.</param>
	/// <returns>Коэфициенты дискретного периодического Q-сплайна 1-го порядка.</returns>
	private static float[] CalculateQSpline(int stepsBetweenBasePoints, int basePointsCount)
	{
		var N = stepsBetweenBasePoints * basePointsCount;
		var qSpline = new float[N];

		for(var j = 0; j < N; j++) {
			if(j >= 0 && j <= stepsBetweenBasePoints - 1) {
				qSpline[j] = ((1f * stepsBetweenBasePoints) - j) / stepsBetweenBasePoints;
			} else
			if(j >= stepsBetweenBasePoints && j <= N - stepsBetweenBasePoints) {
				qSpline[j] = 0;
			} else
			if(j >= N - stepsBetweenBasePoints + 1 && j <= N - 1) {
				qSpline[j] = ((1f * j) - N + stepsBetweenBasePoints) / stepsBetweenBasePoints;
			}
		}

		return qSpline;
	}

	/// <summary>
	/// Вычисляет точки дискретного периодического сплайна с векторными коэфициентами.<br/>
	/// https://www.math.spbu.ru/ru/mmeh/AspDok/pub/2010/Chashnikov.pdf#page=7.
	/// </summary>
	/// <param name="basePoints">Полюса сплайна.</param>
	/// <param name="bSplineCoeffs">Коэффициенты B-сплайна 1-ого порядка.</param>
	/// <param name="splineDegree">Порядок сплайна.</param>
	/// <param name="stepsBetweenBasicPoints">Число узлов между полюсами сплайна.</param>
	/// <param name="m">Число полюсов.</param>
	/// <returns></returns>
	private static PointF[] CalculateSSpline(PointF[] basePoints, float[] bSplineCoeffs, int splineDegree, int stepsBetweenBasicPoints)
	{
		var m = basePoints.Length;
		var N = stepsBetweenBasicPoints * m;
		var sSpline = new PointF[splineDegree + 1][];
		for(var i = 1; i <= splineDegree; i++) {
			sSpline[i] = new PointF[N];
		}

		for(var j = 0; j < N; j++) {
			sSpline[1][j] = new PointF(0, 0);
			for(var p = 0; p < m; p++) {
				sSpline[1][j] += basePoints[p] * bSplineCoeffs[GetPositiveIndex(j - (p * stepsBetweenBasicPoints), N)];
			}
		}

		for(var v = 2; v <= splineDegree; v++) {
			for(var j = 0; j < N; j++) {
				sSpline[v][j] = new PointF(0, 0);
				for(var k = 0; k < N; k++) {
					sSpline[v][j] += bSplineCoeffs[k] * sSpline[v - 1][GetPositiveIndex(j - k, N)];
				}
				sSpline[v][j] /= stepsBetweenBasicPoints;
			}
		}

		return sSpline[splineDegree];
	}

	/// <summary>
	///  Вычисляет узловые точки дискретного сплайна с периодичностью N = stepsPerSpline * len(originalPoints).
	/// </summary>
	/// <param name="originalPoints">Полюса искомого сплайна.</param>
	/// <param name="splineDegree">Порядок искомого сплайна.</param>
	/// <param name="stepsPerSpline">Число линеаризирующих точек между полюсами.</param>
	/// <param name="splineMustIncludeOriginalPoints">Является ли искомый сплайн интерполирующим (true) или аппроксимирующим(false).</param>
	/// <returns>Точки искомого сплайна на плоскости.</returns>
	/// <exception cref="ArgumentNullException"></exception>
	/// <exception cref="ArgumentException"></exception>
	public static PointF[] Calculate(PointF[] originalPoints, int splineDegree, int stepsPerSpline = 5, bool splineMustIncludeOriginalPoints = true)
	{
		#region pre-conditions
		if(originalPoints == null) {
			throw new ArgumentNullException(nameof(originalPoints));
		}
		if(originalPoints.Length <= 2) {
			throw new ArgumentException("There must be at least 2 original points.");
		}
		if(splineDegree <= 0) {
			throw new ArgumentException("Spline degree must be more than zero.");
		}
		if(stepsPerSpline < 1) {
			throw new ArgumentException("There must be at least 1 step per line.");
		}
		#endregion

		var vectors = splineMustIncludeOriginalPoints
			? RecalculateVectors(originalPoints, splineDegree, stepsPerSpline)
			: originalPoints;

		var m = originalPoints.Length;
		var qSpline = CalculateQSpline(stepsPerSpline, m);
		var resultPoints = CalculateSSpline(vectors, qSpline, splineDegree, stepsPerSpline);

		return resultPoints;
	}
}