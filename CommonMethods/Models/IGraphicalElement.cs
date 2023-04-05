using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicLibrary.Models;
public interface IGraphicalElement
{
	public void MoveCoordinates(int dX, int dY);
	public void Rotate(float angleR, Point relativeTo);
	public IGraphicalElement Clone();
}
