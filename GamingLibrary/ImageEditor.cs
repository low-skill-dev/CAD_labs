using GraphicLibrary;
using GraphicLibrary.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointF = GraphicLibrary.MathModels.PointF;

namespace InteractiveLibrary;

public class ControlledDot
{
	public PointF Dot { get; set; }
	public float ControlledRadius { get; set; }
	public bool IsSelected { get; set; }
}

public class ImageEditor : BitmapDrawer
{
	private IEnumerator<bool> pattern;

	public float DotRadius { get; set; } = 5*2;
	public Color DotColor { get; set; } = Color.Yellow;
	public Color SelectedDotColor { get; set; } = Color.White;

	public Color LineColor { get; set; } = Color.LightGreen;

	public List<(PointF Point, bool IsSelected)> ControlledDots { get; private set; }

	public ImageEditor(int frameWidth, int frameHeight)
		: base(frameWidth, frameHeight)
	{
		ControlledDots = new();
		pattern = ALinearElement.GetDefaultPatternResolver();
	}


	public void AddControlledDot(PointF point)
	{
		this.ControlledDots.Add(new(point, false));
	}

	public void RenderCurrentState()
	{
		if(ControlledDots.Count == 0) return;

		base.Circles.Clear();
		base.Lines.Clear();

		base.Circles.Add(new(ControlledDots[0].Point, DotRadius, ControlledDots[0].IsSelected ? SelectedDotColor : DotColor, pattern));
		for(int i = 1; i < ControlledDots.Count; i++) {
			var prev = ControlledDots[i - 1];
			var curr = ControlledDots[i];
			base.Circles.Add(new(curr.Point, DotRadius, curr.IsSelected ? SelectedDotColor : DotColor, pattern));
			base.Lines.Add(new(prev.Point, curr.Point, LineColor, pattern));
		}

		base.RenderFrame();
	}

	public int? FindSelectedByClickId(PointF coordinates, bool markAsSelected = true)
	{
		/* если от какой-либо точки радиус до coordinates меньше, чем DotRadius
		 * то регистрируем попадание
		 */

		for(int i = ControlledDots.Count - 1; i >= 0; i--) {
			var curr = ControlledDots[i];

			if(Common.GetCirleRadius(curr.Point, coordinates) <= DotRadius) {
				if(markAsSelected) curr.IsSelected = true;
				return i;
			}
		}

		return null;
	}
}
