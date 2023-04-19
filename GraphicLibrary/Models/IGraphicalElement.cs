using GraphicLibrary.MathModels;
using PointF = GraphicLibrary.MathModels.PointF;

namespace GraphicLibrary.Models;
public interface IGraphicalElement
{
	public void Move(float dX, float dY);
	public void Rotate(float angleR, PointF relativeTo);
	public void Scale(float scale, PointF relativeTo);
	public void Mirror(PointF relativeTo);
	public void Mirror(LineF relativeTo);
	public IGraphicalElement Clone();
}
