using GraphicLibrary.MathModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.MathF;
using static InteractiveLibrary.Square.CollisionDirection;
using GraphicLibrary;
using GraphicLibrary.Models;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace InteractiveLibrary;


public class Square
{
	// Текущее положение в пространстве XY
	public PointF Location { get; set; }

	// Смещение квадрата за тик.
	// Задает одновременно и скорость и направление.
	public PointF Direction { get; set; }

	// Определяет текущую скорость путем решения теоремы пифагора
	public float CurrentSpeed => Sqrt(Pow(Direction.X, 2) + Pow(Direction.Y, 2));

	public float MaxSpeed { get; set; } = 3000;

	public Square()
	{
		Location = new(15f,15f);
		Direction = new(15f, 15f);
	}

	// Задает сторону, с которой квадрат уперся в стену
	public enum CollisionDirection
	{
		Top,
		Bottom,
		Left,
		Right
	}

	// Изменяет направление и скорость
	public void ChangeDirection(CollisionDirection collision, float speedCoeff)
	{
		float newSpeed, xPart, xSpeed, ySpeed;
		switch(collision) {
			case Top:
			case Bottom:
				// задает текущую скорость путем решения теоремы пифагора
				newSpeed = CurrentSpeed * speedCoeff;
				if(newSpeed > MaxSpeed) newSpeed = MaxSpeed;
				xPart = Random.Shared.NextSingle();
				xSpeed = newSpeed * xPart;
				// x^2 + y^2 = currentSpeed^2
				// y^2 = currentSpeed^2 - x^2
				ySpeed = Sqrt(Pow(newSpeed, 2) - Pow(xSpeed, 2));
#if DEBUG
				var actual = (int)Round(Sqrt(Pow(xSpeed, 2) + Pow(ySpeed, 2)));
				var expected = (int)Round(newSpeed);
				if(actual != expected) {
					throw new AggregateException();
				}
#endif
				Direction = new(xSpeed,
					(collision == Bottom ? 1 : -1) * Abs(ySpeed));
				break;
			case Left:
			case Right:
				newSpeed = CurrentSpeed * speedCoeff;
				if(newSpeed > MaxSpeed) newSpeed = MaxSpeed;
				xPart = Random.Shared.NextSingle();
				xSpeed = CurrentSpeed * xPart;
				ySpeed = newSpeed * (1 - Pow(xPart, 2));
				Direction = new(
					(collision == Left ? 1 : -1) * Abs(xSpeed), ySpeed);
				break;
		}
	}
}


/* Класс хранит текущее состояние системы - игры, в которой пользователь
 * путем перемещения своего квадрата с помощью мыши убегает от двух других квадратов. 
 * Внедряет в себя BitmapDrawer для отображения текущего состояния.
 */
public class SquaresRunner : BitmapDrawer
{
	public PointF MouseAt { get; set; } = new PointF(0, 0);
	public float SquareSide { get; set; } = 50;
	public List<Square> Enemies { get; init; } = new();
	public float SpeedCoef { get; set; } = 1.2f;

	public SquaresRunner(int width, int height)
		: base(width, height)
	{
		Enemies.Add(new() { Location = new PointF(width - SquareSide, height - SquareSide), Direction = new(0, -height / 2) });
		Enemies.Add(new() { Location = new PointF(width - 2*SquareSide, height - SquareSide), Direction = new(-width/2, -0) });
	}

	private Rectangle CenterAndSizeToRect(PointF center, PointF size, System.Drawing.Color? color = null)
	{
		var upperLeft = center - size / 2;
		var lowerRight = center + size / 2;

		return new Rectangle(upperLeft, lowerRight, color ?? System.Drawing.Color.Red, ALinearElement.GetDefaultPatternResolver());
	}

	public void ResetCurrentState()
	{
		this.Enemies.Clear();
	}

	public void RenderCurrentState()
	{
		base.Reset();

		this.Enemies.ForEach(
			x => base.Rectangles.Add(CenterAndSizeToRect(x.Location, new PointF(SquareSide, SquareSide), System.Drawing.Color.Red)));

		base.Rectangles.Add(CenterAndSizeToRect(MouseAt, new PointF(SquareSide, SquareSide), System.Drawing.Color.Green));

		base.RenderFrame();
	}

	private bool isCaptured(PointF EnemyLocation)
	{
		var minX = EnemyLocation.X - SquareSide;
		var maxX = EnemyLocation.X + SquareSide;
		var minY = EnemyLocation.Y - SquareSide;
		var maxY = EnemyLocation.Y + SquareSide;

		if(MouseAt.X < minX || MouseAt.X > maxX || MouseAt.Y < minY || MouseAt.Y > maxY) {
			return false;
		} else {
			return true;
		}
	}

	public void UpdateState(float ticks = 1)
	{
		foreach(var en in Enemies) {		
			if(en.Location.X + SquareSide / 2 > base.FrameWidth) {
				en.ChangeDirection(Right, SpeedCoef);
			} else
			if(en.Location.X - SquareSide / 2 < 0) {
				en.ChangeDirection(Left, SpeedCoef);
			} else
			if(en.Location.Y + SquareSide / 2 > base.FrameHeight) {
				en.ChangeDirection(Top, SpeedCoef);
			} else
			if(en.Location.Y - SquareSide / 2 < 0) {
				en.ChangeDirection(Bottom, SpeedCoef);
			}

			en.Location += en.Direction * ticks;

			if(isCaptured(en.Location))
				throw new GameException("Вы проиграли.");
		}
	}
}
