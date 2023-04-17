using coursework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursework.DataModels
{
	public class SimplePolyLine
	{

	}
    public class Saved
	{
		public int Width { get; set; }
		public int Height { get; set; }
		public int BackgroundColorArgb { get; set; }
		public PolyLineF[] PolyLines { get; init; }
		public CircleF[] Circles { get; set; }
		public ArcF[] Arcs { get; set; }
		public FillerF[] Fillers { get; set; }

		//public Saved(int Width, int Height, int BackgroundColorArgb/*, List<PolyLineF> polyLines*/, List<CircleF> Circles, List<ArcF> Arcs, List<FillerF> Fillers)
		//{
		//	this.Width = Width;
		//	this.Height = Height;
		//	this.BackgroundColorArgb = BackgroundColorArgb;
		//	//this.PolyLines = polyLines;
		//	this.Circles = Circles.ToArray();
		//	this.Arcs = Arcs.ToArray();
		//	this.Fillers = Fillers.ToArray();
		//}
	}
}
