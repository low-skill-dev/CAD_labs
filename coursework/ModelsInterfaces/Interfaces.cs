using GraphicLibrary;
using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coursework.ModelsInterfaces
{
    public interface IMoveable
    {
        public void Move(PointF diff);
    }
    public interface IScaleable
    {
        public void Scale(float scale, PointF relativeTo);
    }
    public interface IRotateable
    {
        public void Rotate(float angleR, PointF relativeTo);
    }
	public interface IColored
	{
		public int ColorArgb { get; set; }
	}
    public interface IPatterned
    {
        public string Pattern { get; set; }
		public IEnumerator<bool> PatternResolver => Common.CreatePatternResolver(Pattern);
	}

    public abstract class AEditorElement : IMoveable, IScaleable, IRotateable, IColored
	{
        public const int LightGreenArgb = -7278960;
		public const int LightCoralArgb = -1015680;
		public const string DefaultPattern = "+";

		public abstract int ColorArgb { get; set; }

		public abstract void Move(PointF diff);
        public abstract void Rotate(float angleR, PointF relativeTo);
        public abstract void Scale(float scale, PointF relativeTo);
        public abstract AEditorElement Clone();
	}
}
