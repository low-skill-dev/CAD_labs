using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicLibrary;
using GraphicLibrary.Models;
using Microsoft.VisualBasic.Logging;
using PointF = GraphicLibrary.MathModels.PointF;

namespace InteractiveLibrary;

public class GameException : Exception
{
	public GameException(string message) : base(message) { }
}
public class GameWonException : Exception
{
	public GameWonException(string message) : base(message) { }
}


public class FallingBall // not struct coz we need refs
{
	public enum BallStates
	{
		Stable,
		Falling,
		Disposed
	}

	public BallStates State;
	public PointF Location;
	public float Speed;
	public float Radius;

	public Color BorderColor;
	public Color FillColor;

	public bool CanTriggerEvent;

	public FallingBall(PointF location, float radius)
		: this()
	{
		Location = location;
		Radius = radius;

		State = BallStates.Stable;
		Speed = 0;
		BorderColor = Color.Blue;
		FillColor = Color.Aqua;

		CanTriggerEvent = true;
	}

	private FallingBall()
	{
	}
}

public class PongCatcher : BitmapDrawer
{
	private IEnumerator<bool> _defaultPattern { get; init; }
	private PointF _rightAndDown => new PointF(
		this.PlatformWidth/2,
		this.PlatformHeight/2);
	private Circle? DefeatCircle { get; set; }

	public int PlatformWidth { get; private set; }
	public int PlatformHeight { get; private set; }
	public PointF PlatformLocation { get; private set; }
	public Color PlatformBorderColor { get; private set; }
	public Color PlatformFillColor { get; private set; }

	public int NumberOfBalls { get; private set; }
	public float BallAcceleration { get; private set; }
	public float BallRadius { get; private set; }
	public Color BallBorderColor { get; private set; }
	public Color BallFillColor { get; private set; }
	public bool DisableBallFilling { get; set; }

	public bool UseRandomColors { get; set; }

	public List<FallingBall> Balls { get; private set; } = null!;
	public List<FallingBall> FallingBalls { get; private set; }

	public PongCatcher(int frameWidth, int frameHeight, int ballsCount,
		int? platformWidth = null, int? platformHeight = null, float? ballAcceleration = null, float? ballRadius = null,
		Color? ballBorderColor = null, Color? ballFillColor = null, Color? platformBorderColor = null, Color? platformFillColor = null,
		bool? useRandomColors = null)
		: base(frameWidth, frameHeight)
	{
		this.PlatformWidth = platformWidth ?? frameWidth / 5;
		this.PlatformHeight = platformHeight ?? frameHeight / 20;
		this.PlatformBorderColor = platformBorderColor ?? Color.Red;
		this.PlatformFillColor = platformFillColor ?? Color.OrangeRed;
		this.PlatformLocation = new(0, frameHeight - 1 - PlatformHeight);

		this.BallAcceleration = ballAcceleration ?? frameHeight / 4;
		this.BallRadius = ballRadius ?? frameWidth / 2 / (float)ballsCount;
		this.BallBorderColor = ballBorderColor ?? Color.Blue;
		this.BallFillColor = ballFillColor ?? Color.Aqua;




		this.UseRandomColors = useRandomColors ?? false;

		this._defaultPattern = ALinearElement.GetDefaultPatternResolver();
		this.FallingBalls = new();
	}

	#region public
	public void SetPlatformX(float x, bool correctLeftToCenter = true)
	{
		if(correctLeftToCenter) x -= PlatformWidth / 2;

		if(x < 0) x = 0;
		if(x > FrameWidth - 1 - PlatformWidth) x = FrameWidth - 1 - PlatformWidth;

		this.PlatformLocation = new PointF(x, this.PlatformLocation.Y);
	}
	public void StartNewGame()
	{
		ResetCurrentState();
	}
	public void RenderCurrentState()
	{
		base.Reset();

		foreach(var ball in Balls) {
			base.AddCircle(new(ball.Location, ball.Radius, ball.BorderColor, _defaultPattern));
			if(!DisableBallFilling)
				base.AddFiller(new(ball.Location, ball.FillColor));
		}
		foreach(var ball in FallingBalls) {
			base.AddCircle(new(ball.Location, ball.Radius, ball.BorderColor, _defaultPattern));
			if(!DisableBallFilling)
				base.AddFiller(new(ball.Location, ball.FillColor));
		}

		if(DefeatCircle is not null) {
			AddCircle(DefeatCircle);
		}

		var platX = PlatformLocation.X;
		var platformStart = new PointF(platX, this.FrameHeight - 1 - PlatformHeight);
		var platformEnd = new PointF(platX + PlatformWidth, this.FrameHeight - 1);
		base.AddRectangle(new(platformStart, platformEnd, PlatformBorderColor, _defaultPattern));
		base.AddFiller(new(platformStart + _rightAndDown, PlatformFillColor));

		base.RenderFrame();
	}
	#endregion


	#region private
	private Color GetRandomColor()
	{
		return Color.FromArgb(Random.Shared.Next(256), Random.Shared.Next(256), Random.Shared.Next(256));
	}
	private void ResetCurrentState()
	{
		int totalBalls = (int)(base.FrameWidth / 2 / BallRadius); // floor
		Balls = new(totalBalls);
		FallingBalls = new(3);
		DefeatCircle = null;

		var locY = BallRadius;
		for(int i = 0; i < totalBalls; i++) {
			float locX = BallRadius + i * 2 * BallRadius;

			Balls.Add(new(new(locX, locY), BallRadius) {
				BorderColor = UseRandomColors ? GetRandomColor() : this.BallBorderColor,
				FillColor = UseRandomColors ? GetRandomColor() : this.BallFillColor
			});
		}
	}

	private void SetRandomBallToFalling()
	{
		if(Balls.Count == 0) return;

		var rnd = Random.Shared.Next(Balls.Count);

		var pop = Balls[rnd];
		Balls.RemoveAt(rnd);
		FallingBalls.Add(pop);
	}

	// Должен ли круг начинать падение другого?
	public void UpdateState(float step)
	{
		// Начать падение шарика если нужно
		var trigger = FallingBalls.FirstOrDefault(x => x.CanTriggerEvent == true && x.Location.Y > FrameHeight / 2);
		if(trigger is not null) {
			SetRandomBallToFalling();
			trigger.CanTriggerEvent = false;
		} else
		if(FallingBalls.Count == 0) {
			SetRandomBallToFalling();
		}

		// step определяет "промежуток во времени" для смещения.
		for(int i = 0; i < FallingBalls.Count; i++) {
			var ball = FallingBalls[i];
			ball.Speed += BallAcceleration * step;
			ball.Location += new PointF(0, ball.Speed);
		}

		// регистрация попадания
		var minX = PlatformLocation.X;
		var maxX = PlatformLocation.X + PlatformWidth;
		for(int i = 0; i < FallingBalls.Count; i++) {
			var ball = FallingBalls[i];
			if(ball.State == FallingBall.BallStates.Disposed) continue;

			var loc = ball.Location;
			if(loc.Y > PlatformLocation.Y) {
				if(loc.X >= minX && loc.X <= maxX) {
					ball.State = FallingBall.BallStates.Disposed;
					FallingBalls.Remove(ball);
					i--;
				} else {
					DefeatCircle = new Circle(new(loc.X, PlatformLocation.Y), ball.Radius + 2, Color.Yellow);
					throw new GameException("Ты проиграл!");
				}
			}
		}

		if(Balls.Count == 0 && FallingBalls.Count == 0)
			throw new GameWonException("Ты выиграл!");
	}
	#endregion
}
